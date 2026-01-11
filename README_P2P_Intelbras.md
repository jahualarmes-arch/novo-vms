# 🎯 Intelbras P2P Cloud - Solução Completa para VMS

> **Conecte DVRs/NVRs Intelbras e Dahua via P2P Cloud sem IP público, DDNS ou liberação de portas.**

```
╔═══════════════════════════════════════════════════════════════════╗
║  ✅ STATUS: TESTADO E FUNCIONANDO                                 ║
║  📅 Data: Janeiro 2026                                            ║
║  🏢 Testado com: SIMNext + DVR iNVD 1016                         ║
╚═══════════════════════════════════════════════════════════════════╝
```

---

## 📋 Índice

1. [Visão Geral](#1-visão-geral)
2. [Como Funciona](#2-como-funciona)
3. [Arquivos Necessários](#3-arquivos-necessários)
4. [Configurações Descobertas](#4-configurações-descobertas)
5. [API Reference](#5-api-reference)
6. [Guia Rápido](#6-guia-rápido)
7. [Código PowerShell](#7-código-powershell)
8. [Código C#](#8-código-c)
9. [Código Delphi](#9-código-delphi)
10. [Integração com VMS](#10-integração-com-vms)
11. [Múltiplos Dispositivos](#11-múltiplos-dispositivos)
12. [Troubleshooting](#12-troubleshooting)
13. [Histórico de Descobertas](#13-histórico-de-descobertas)

---

## 1. Visão Geral

### O Problema
Clientes de portaria remota frequentemente não têm:
- IP público fixo
- Capacidade de liberar portas no roteador
- Acesso técnico ao equipamento de rede
- Conexões convencionais (Starlink, 4G, CGNAT)

### A Solução
Usar a infraestrutura P2P Cloud da Intelbras para criar túneis TCP locais que redirecionam para DVRs remotos.

### Vantagens

| Recurso | Conexão Direta | P2P Cloud |
|---------|:-------------:|:---------:|
| IP Público | ❌ Necessário | ✅ Não precisa |
| DDNS | ❌ Necessário | ✅ Não precisa |
| Port Forward | ❌ Necessário | ✅ Não precisa |
| Conta Intelbras | - | ✅ Não precisa |
| Funciona com NAT | ❌ Limitado | ✅ Sim |
| Funciona com CGNAT | ❌ Não | ✅ Sim |
| Starlink | ❌ Não | ✅ Sim |
| 4G/5G | ❌ Não | ✅ Sim |

---

## 2. Como Funciona

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│   SEU VMS       │         │  Servidor P2P    │         │   DVR Cliente   │
│   (Portaria)    │         │  Intelbras       │         │   (Remoto)      │
└────────┬────────┘         └────────┬─────────┘         └────────┬────────┘
         │                           │                            │
         │ 1. IBCloud_init()         │                            │
         │──────────────────────────►│                            │
         │                           │    DVR mantém conexão      │
         │ 2. IBCloud_query(serial)  │    permanente com servidor │
         │──────────────────────────►│◄───────────────────────────│
         │                           │                            │
         │ 3. IBCloud_add_port()     │                            │
         │──────────────────────────►│   4. Hole Punching         │
         │                           │───────────────────────────►│
         │                           │                            │
         │      5. TÚNEL P2P ESTABELECIDO                         │
         │◄══════════════════════════════════════════════════════►│
         │                           │                            │
         │ 6. SDK Dahua conecta em 127.0.0.1:17777                │
         │════════════════════════════════════════════════════════►
         │                    (Vídeo flui pelo túnel)             │
```

### Resumo do Fluxo

1. **VMS** chama `IBCloud_init()` para conectar ao servidor P2P
2. **VMS** chama `IBCloud_query(serial)` para verificar se DVR está online
3. **VMS** chama `IBCloud_add_port()` para criar túnel local
4. **Servidor P2P** coordena hole punching entre VMS e DVR
5. **Túnel** é estabelecido (ex: `127.0.0.1:17777` → DVR:37777)
6. **SDK Dahua** conecta no túnel como se fosse conexão direta

---

## 3. Arquivos Necessários

> ⚠️ **CRÍTICO:** Todas as DLLs devem estar na **mesma pasta** do executável!

### DLLs Obrigatórias (32-bit)

| Arquivo | Tamanho | Descrição |
|---------|---------|-----------|
| `IBCloudSDK.dll` | 109 KB | **SDK P2P principal** |
| `libt2u.dll` | 237 KB | Core do túnel P2P |
| `dhnetsdk.dll` | 18 MB | SDK Dahua para vídeo |
| `dhplay.dll` | 807 KB | Player de vídeo |
| `Infra.dll` | 1.1 MB | Infraestrutura |
| `NetFramework.dll` | 590 KB | Framework de rede |
| `Stream.dll` | 295 KB | Streaming |
| `StreamSvr.dll` | 1.2 MB | Servidor de stream |
| `avnetsdk.dll` | 3.2 MB | SDK de rede AV |
| `h264dec.dll` | 567 KB | Decodificador H.264 |
| `hevcdec.dll` | 778 KB | Decodificador H.265 |

### Onde Obter as DLLs

```
Opção 1: C:\Program Files\Intelbras\SIMNext\SIM Next\
Opção 2: Pasta de instalação do MoniCameras
Opção 3: SDK oficial Intelbras/Dahua
```

### ⚠️ Arquitetura 32-bit

A `IBCloudSDK.dll` é **32-bit**. Seu VMS deve ser:
- Compilado como **x86 (32-bit)**, ou
- Usar um processo auxiliar 32-bit para gerenciar túneis

---

## 4. Configurações Descobertas

### Servidores P2P Funcionais

| Servidor | Porta | Status |
|----------|-------|--------|
| `38.250.250.12` | 1250 | ✅ Funciona |
| `38.250.250.18` | 1252 | ✅ Funciona |
| `38.250.250.27` | 1251 | ✅ Funciona |
| `38.250.250.32` | 1252 | ✅ Funciona |
| `38.250.250.33` | 8800 | ✅ Funciona |
| `intelbrasp2p.com.br` | 8800 | ✅ Funciona |

### Chave de Autenticação (svrkey)

```
Chave descoberta: p2pintelbras2014543
Fonte: IBCloudSDK.dll (strings)
```

> **Nota:** A IBCloudSDK.dll já tem a chave hardcoded, então você não precisa especificá-la!

---

## 5. API Reference

### Funções da IBCloudSDK.dll

#### Configuração (chamar ANTES de init!)

```c
int IBCloud_automatic_shutdown(int enable);
// enable: 0 = DESABILITAR shutdown automático (OBRIGATÓRIO!)
// enable: 1 = habilitar shutdown automático

int IBCloud_connection_timeout(int timeout_ms);
// timeout_ms: timeout em milissegundos (recomendado: 60000)
```

#### Inicialização

```c
int IBCloud_init(void);
// Retorno: 0 = sucesso

void IBCloud_exit(void);
// Finaliza SDK e fecha todos os túneis
```

#### Consulta

```c
int IBCloud_status(void);
// Retorno: 0 = conectado, -1 = erro

int IBCloud_query(char* serial);
// serial: número de série do DVR (ex: "OJHL0700323ZS")
// Retorno: 1 = online, -1 = offline/não encontrado
```

#### Túnel

```c
int IBCloud_add_port(char* serial, unsigned short remote_port, unsigned short local_port);
// serial: número de série do DVR
// remote_port: porta no DVR (geralmente 37777)
// local_port: porta local desejada (ex: 17777)
// Retorno: porta local criada ou -1 = erro

void IBCloud_del_port(unsigned short port);
// port: porta local para fechar
```

---

## 6. Guia Rápido

### ⚠️ ORDEM OBRIGATÓRIA DE CHAMADAS

```
╔════════════════════════════════════════════════════════════════════╗
║  1. IBCloud_automatic_shutdown(0)    ← PRIMEIRO! Desabilita shutdown
║  2. IBCloud_connection_timeout(60000) ← Timeout 60 segundos
║  3. IBCloud_init()                    ← Inicializa SDK
║  4. Sleep(3000)                       ← Aguarda 3 segundos
║  5. IBCloud_query(serial)             ← Verifica se DVR está online
║  6. IBCloud_add_port(serial, 37777, 17777) ← Cria túnel
║  7. CLIENT_Login("127.0.0.1", 17777, ...) ← Conecta SDK Dahua
╚════════════════════════════════════════════════════════════════════╝
```

### Mapeamento de Portas

| Serviço | Porta DVR | Porta Local |
|---------|-----------|-------------|
| SDK Dahua | 37777 | 17777 + índice |
| RTSP | 554 | 18554 + índice |
| HTTP | 80 | 18080 + índice |

---

## 7. Código PowerShell

### Teste Rápido (use PowerShell 32-bit!)

```powershell
# IMPORTANTE: Executar no PowerShell 32-bit
# C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe

cd C:\Caminho\Para\DLLs

$code = @"
using System;
using System.Runtime.InteropServices;

public class IBCloud
{
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int IBCloud_init();
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void IBCloud_exit();
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int IBCloud_status();
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int IBCloud_query(string uuid);
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int IBCloud_add_port(string uuid, ushort remote_port, ushort local_port);
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void IBCloud_del_port(ushort port);
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int IBCloud_automatic_shutdown(int enable);
    
    [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int IBCloud_connection_timeout(int timeout_ms);
}
"@

Add-Type -TypeDefinition $code

# ========== CONFIGURAÇÃO (ANTES de init!) ==========
[IBCloud]::IBCloud_automatic_shutdown(0)      # CRÍTICO!
[IBCloud]::IBCloud_connection_timeout(60000)  # 60 segundos

# ========== INICIALIZAÇÃO ==========
$init = [IBCloud]::IBCloud_init()
Write-Host "Init: $init"
Start-Sleep -Seconds 3

# ========== STATUS ==========
$status = [IBCloud]::IBCloud_status()
Write-Host "Status: $status (0=conectado)"

# ========== QUERY DISPOSITIVO ==========
$serial = "OJHL0700323ZS"  # ← TROQUE PELO SEU SERIAL
$query = [IBCloud]::IBCloud_query($serial)
Write-Host "Query: $query (1=encontrado)"

# ========== CRIAR TÚNEL ==========
if ($query -eq 1) {
    $port = [IBCloud]::IBCloud_add_port($serial, 37777, 17777)
    Write-Host "Túnel criado: 127.0.0.1:$port"
    Write-Host ""
    Write-Host "============================================"
    Write-Host "  TÚNEL ATIVO! Conecte seu VMS em:"
    Write-Host "  IP: 127.0.0.1"
    Write-Host "  Porta: $port"
    Write-Host "============================================"
    
    # Manter túnel ativo
    while($true) { Start-Sleep -Seconds 30 }
} else {
    Write-Host "DVR não encontrado ou offline!"
}
```

---

## 8. Código C#

### Classe Completa

```csharp
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VMS.P2P
{
    /// <summary>
    /// Gerenciador de túneis P2P para DVRs Intelbras/Dahua
    /// </summary>
    public class IntelbrasP2P : IDisposable
    {
        #region DLL Imports
        
        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int IBCloud_init();

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IBCloud_exit();

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int IBCloud_status();

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int IBCloud_query(string uuid);

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int IBCloud_add_port(string uuid, ushort remote_port, ushort local_port);

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void IBCloud_del_port(ushort port);

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int IBCloud_automatic_shutdown(int enable);

        [DllImport("IBCloudSDK.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int IBCloud_connection_timeout(int timeout_ms);
        
        #endregion

        #region Constants
        
        private const ushort BASE_PORT = 17777;
        private const int MAX_TUNNELS = 100;
        private const int INIT_WAIT_MS = 3000;
        private const int DEFAULT_TIMEOUT_MS = 60000;
        
        #endregion

        #region Fields
        
        private bool _initialized = false;
        private readonly Dictionary<string, ushort> _tunnels = new Dictionary<string, ushort>();
        private readonly object _lock = new object();
        
        #endregion

        #region Singleton
        
        private static IntelbrasP2P _instance;
        private static readonly object _instanceLock = new object();
        
        public static IntelbrasP2P Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                            _instance = new IntelbrasP2P();
                    }
                }
                return _instance;
            }
        }
        
        #endregion

        #region Properties
        
        public bool IsInitialized => _initialized;
        public int TunnelCount => _tunnels.Count;
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Inicializa o SDK P2P. Chamar UMA ÚNICA VEZ ao iniciar a aplicação.
        /// </summary>
        public bool Initialize()
        {
            lock (_lock)
            {
                if (_initialized) 
                    return true;

                try
                {
                    // CRÍTICO: Desabilitar shutdown ANTES de init!
                    IBCloud_automatic_shutdown(0);
                    
                    // Configurar timeout
                    IBCloud_connection_timeout(DEFAULT_TIMEOUT_MS);
                    
                    // Inicializar SDK
                    int result = IBCloud_init();
                    
                    // Aguardar conexão com servidor P2P
                    Thread.Sleep(INIT_WAIT_MS);
                    
                    _initialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[P2P] Erro ao inicializar: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Finaliza o SDK P2P. Chamar ao encerrar a aplicação.
        /// </summary>
        public void Shutdown()
        {
            lock (_lock)
            {
                if (!_initialized) 
                    return;
                
                CloseAllTunnels();
                
                try
                {
                    IBCloud_exit();
                }
                catch { }
                
                _initialized = false;
            }
        }

        /// <summary>
        /// Verifica se o dispositivo está online no cloud P2P.
        /// </summary>
        public bool IsDeviceOnline(string serial)
        {
            if (!_initialized) 
                return false;
            
            try
            {
                return IBCloud_query(serial) == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cria um túnel TCP para o dispositivo.
        /// </summary>
        /// <param name="serial">Número de série do DVR</param>
        /// <param name="remotePort">Porta no DVR (padrão: 37777)</param>
        /// <returns>Porta local do túnel ou -1 se erro</returns>
        public int CreateTunnel(string serial, ushort remotePort = 37777)
        {
            lock (_lock)
            {
                if (!_initialized) 
                    return -1;

                // Já existe túnel para este serial?
                if (_tunnels.TryGetValue(serial, out ushort existingPort))
                    return existingPort;

                // Verificar se dispositivo está online
                if (!IsDeviceOnline(serial))
                {
                    System.Diagnostics.Debug.WriteLine($"[P2P] Dispositivo offline: {serial}");
                    return -1;
                }

                // Obter próxima porta disponível
                ushort localPort = GetNextAvailablePort();
                if (localPort == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[P2P] Sem portas disponíveis");
                    return -1;
                }

                try
                {
                    // Criar túnel
                    int result = IBCloud_add_port(serial, remotePort, localPort);
                    
                    if (result > 0)
                    {
                        _tunnels[serial] = (ushort)result;
                        System.Diagnostics.Debug.WriteLine($"[P2P] Túnel criado: {serial} -> 127.0.0.1:{result}");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[P2P] Erro ao criar túnel: {ex.Message}");
                }
                
                return -1;
            }
        }

        /// <summary>
        /// Fecha o túnel de um dispositivo específico.
        /// </summary>
        public void CloseTunnel(string serial)
        {
            lock (_lock)
            {
                if (_tunnels.TryGetValue(serial, out ushort port))
                {
                    try
                    {
                        IBCloud_del_port(port);
                    }
                    catch { }
                    
                    _tunnels.Remove(serial);
                    System.Diagnostics.Debug.WriteLine($"[P2P] Túnel fechado: {serial}");
                }
            }
        }

        /// <summary>
        /// Fecha todos os túneis abertos.
        /// </summary>
        public void CloseAllTunnels()
        {
            lock (_lock)
            {
                foreach (var kvp in _tunnels)
                {
                    try
                    {
                        IBCloud_del_port(kvp.Value);
                    }
                    catch { }
                }
                _tunnels.Clear();
            }
        }

        /// <summary>
        /// Obtém a porta do túnel de um dispositivo.
        /// </summary>
        /// <returns>Porta local ou -1 se não existe</returns>
        public int GetTunnelPort(string serial)
        {
            lock (_lock)
            {
                return _tunnels.TryGetValue(serial, out ushort port) ? port : -1;
            }
        }

        /// <summary>
        /// Verifica se existe túnel para o dispositivo.
        /// </summary>
        public bool HasTunnel(string serial)
        {
            lock (_lock)
            {
                return _tunnels.ContainsKey(serial);
            }
        }
        
        #endregion

        #region Private Methods
        
        private ushort GetNextAvailablePort()
        {
            for (ushort p = BASE_PORT; p < BASE_PORT + MAX_TUNNELS; p++)
            {
                if (!_tunnels.ContainsValue(p))
                    return p;
            }
            return 0;
        }
        
        #endregion

        #region IDisposable
        
        public void Dispose()
        {
            Shutdown();
        }
        
        #endregion
    }
}
```

### Exemplo de Uso

```csharp
// ========== INICIALIZAÇÃO (uma vez ao iniciar VMS) ==========
private void Form_Load(object sender, EventArgs e)
{
    if (!IntelbrasP2P.Instance.Initialize())
    {
        MessageBox.Show("Erro ao inicializar módulo P2P");
    }
}

// ========== CONECTAR A UM DVR P2P ==========
private bool ConectarDVR(string serial, string usuario, string senha)
{
    // Criar túnel
    int porta = IntelbrasP2P.Instance.CreateTunnel(serial);
    
    if (porta < 0)
    {
        MessageBox.Show($"DVR {serial} offline ou não encontrado");
        return false;
    }
    
    // Conectar SDK Dahua no túnel local
    int loginId = NETClient.Login(
        "127.0.0.1",      // Sempre localhost!
        (ushort)porta,     // Porta do túnel
        usuario,
        senha,
        out NET_DEVICEINFO deviceInfo,
        out int error
    );
    
    if (loginId > 0)
    {
        // SUCESSO! Salvar loginId e iniciar vídeo
        _loginId = loginId;
        _serial = serial;
        
        // Iniciar preview
        NETClient.RealPlay(loginId, 0, pictureBox.Handle);
        return true;
    }
    else
    {
        // Falha - fechar túnel
        IntelbrasP2P.Instance.CloseTunnel(serial);
        MessageBox.Show($"Erro de login: {error}");
        return false;
    }
}

// ========== DESCONECTAR ==========
private void DesconectarDVR()
{
    if (_loginId > 0)
    {
        NETClient.Logout(_loginId);
        _loginId = 0;
    }
    
    if (!string.IsNullOrEmpty(_serial))
    {
        IntelbrasP2P.Instance.CloseTunnel(_serial);
        _serial = "";
    }
}

// ========== FINALIZAÇÃO (ao fechar VMS) ==========
private void Form_Closing(object sender, FormClosingEventArgs e)
{
    DesconectarDVR();
    IntelbrasP2P.Instance.Shutdown();
}
```

---

## 9. Código Delphi

### Unit Completa

```pascal
unit IntelbrasP2P;

interface

uses
  System.SysUtils, System.Classes, Winapi.Windows, System.SyncObjs,
  System.Generics.Collections;

type
  TIntelbrasP2P = class
  private
    FDLLHandle: THandle;
    FInitialized: Boolean;
    FTunnels: TDictionary<string, Word>;
    FLock: TCriticalSection;
    
    // Ponteiros para funções da DLL
    FIBCloud_init: function: Integer; cdecl;
    FIBCloud_exit: procedure; cdecl;
    FIBCloud_status: function: Integer; cdecl;
    FIBCloud_query: function(uuid: PAnsiChar): Integer; cdecl;
    FIBCloud_add_port: function(uuid: PAnsiChar; remote_port, local_port: Word): Integer; cdecl;
    FIBCloud_del_port: procedure(port: Word); cdecl;
    FIBCloud_automatic_shutdown: function(enable: Integer): Integer; cdecl;
    FIBCloud_connection_timeout: function(timeout_ms: Integer): Integer; cdecl;
    
    function GetNextLocalPort: Word;
    function GetTunnelCount: Integer;
  public
    constructor Create;
    destructor Destroy; override;
    
    function Initialize: Boolean;
    procedure Finalize;
    
    function IsDeviceOnline(const Serial: string): Boolean;
    function CreateTunnel(const Serial: string; RemotePort: Word = 37777): Integer;
    procedure CloseTunnel(const Serial: string);
    procedure CloseAllTunnels;
    
    function GetTunnelPort(const Serial: string): Integer;
    function HasTunnel(const Serial: string): Boolean;
    
    property Initialized: Boolean read FInitialized;
    property TunnelCount: Integer read GetTunnelCount;
  end;

var
  P2PManager: TIntelbrasP2P;

implementation

const
  BASE_LOCAL_PORT = 17777;
  MAX_TUNNELS = 100;
  INIT_WAIT_MS = 3000;
  DEFAULT_TIMEOUT_MS = 60000;

{ TIntelbrasP2P }

constructor TIntelbrasP2P.Create;
begin
  inherited;
  FInitialized := False;
  FTunnels := TDictionary<string, Word>.Create;
  FLock := TCriticalSection.Create;
  FDLLHandle := 0;
end;

destructor TIntelbrasP2P.Destroy;
begin
  Finalize;
  FTunnels.Free;
  FLock.Free;
  inherited;
end;

function TIntelbrasP2P.GetNextLocalPort: Word;
var
  Port: Word;
begin
  for Port := BASE_LOCAL_PORT to BASE_LOCAL_PORT + MAX_TUNNELS - 1 do
  begin
    if not FTunnels.ContainsValue(Port) then
      Exit(Port);
  end;
  Result := 0;
end;

function TIntelbrasP2P.GetTunnelCount: Integer;
begin
  Result := FTunnels.Count;
end;

function TIntelbrasP2P.Initialize: Boolean;
begin
  Result := False;
  FLock.Enter;
  try
    if FInitialized then
      Exit(True);
    
    // Carregar DLL
    FDLLHandle := LoadLibrary('IBCloudSDK.dll');
    if FDLLHandle = 0 then
    begin
      OutputDebugString('[P2P] Erro ao carregar IBCloudSDK.dll');
      Exit;
    end;
    
    // Obter ponteiros das funções
    @FIBCloud_init := GetProcAddress(FDLLHandle, 'IBCloud_init');
    @FIBCloud_exit := GetProcAddress(FDLLHandle, 'IBCloud_exit');
    @FIBCloud_status := GetProcAddress(FDLLHandle, 'IBCloud_status');
    @FIBCloud_query := GetProcAddress(FDLLHandle, 'IBCloud_query');
    @FIBCloud_add_port := GetProcAddress(FDLLHandle, 'IBCloud_add_port');
    @FIBCloud_del_port := GetProcAddress(FDLLHandle, 'IBCloud_del_port');
    @FIBCloud_automatic_shutdown := GetProcAddress(FDLLHandle, 'IBCloud_automatic_shutdown');
    @FIBCloud_connection_timeout := GetProcAddress(FDLLHandle, 'IBCloud_connection_timeout');
    
    // Verificar funções essenciais
    if not Assigned(FIBCloud_init) or not Assigned(FIBCloud_add_port) then
    begin
      FreeLibrary(FDLLHandle);
      FDLLHandle := 0;
      OutputDebugString('[P2P] Funções não encontradas na DLL');
      Exit;
    end;
    
    // CRÍTICO: Desabilitar shutdown automático ANTES de init!
    if Assigned(FIBCloud_automatic_shutdown) then
      FIBCloud_automatic_shutdown(0);
    
    // Configurar timeout
    if Assigned(FIBCloud_connection_timeout) then
      FIBCloud_connection_timeout(DEFAULT_TIMEOUT_MS);
    
    // Inicializar SDK
    FIBCloud_init();
    
    // Aguardar conexão com servidor P2P
    Sleep(INIT_WAIT_MS);
    
    FInitialized := True;
    Result := True;
    OutputDebugString('[P2P] SDK inicializado com sucesso');
  finally
    FLock.Leave;
  end;
end;

procedure TIntelbrasP2P.Finalize;
begin
  FLock.Enter;
  try
    if not FInitialized then
      Exit;
    
    CloseAllTunnels;
    
    if Assigned(FIBCloud_exit) then
      FIBCloud_exit();
    
    if FDLLHandle <> 0 then
    begin
      FreeLibrary(FDLLHandle);
      FDLLHandle := 0;
    end;
    
    FInitialized := False;
    OutputDebugString('[P2P] SDK finalizado');
  finally
    FLock.Leave;
  end;
end;

function TIntelbrasP2P.IsDeviceOnline(const Serial: string): Boolean;
begin
  Result := False;
  if not FInitialized or not Assigned(FIBCloud_query) then
    Exit;
  
  Result := FIBCloud_query(PAnsiChar(AnsiString(Serial))) = 1;
end;

function TIntelbrasP2P.CreateTunnel(const Serial: string; RemotePort: Word): Integer;
var
  LocalPort: Word;
  PortResult: Integer;
  ExistingPort: Word;
begin
  Result := -1;
  
  FLock.Enter;
  try
    if not FInitialized then
      Exit;
    
    // Já existe túnel para este serial?
    if FTunnels.TryGetValue(Serial, ExistingPort) then
      Exit(ExistingPort);
    
    // Verificar se dispositivo está online
    if not IsDeviceOnline(Serial) then
    begin
      OutputDebugString(PChar('[P2P] Dispositivo offline: ' + Serial));
      Exit;
    end;
    
    // Obter próxima porta disponível
    LocalPort := GetNextLocalPort;
    if LocalPort = 0 then
    begin
      OutputDebugString('[P2P] Sem portas disponíveis');
      Exit;
    end;
    
    // Criar túnel
    PortResult := FIBCloud_add_port(PAnsiChar(AnsiString(Serial)), RemotePort, LocalPort);
    
    if PortResult > 0 then
    begin
      FTunnels.Add(Serial, PortResult);
      OutputDebugString(PChar(Format('[P2P] Túnel criado: %s -> 127.0.0.1:%d', [Serial, PortResult])));
      Result := PortResult;
    end;
  finally
    FLock.Leave;
  end;
end;

procedure TIntelbrasP2P.CloseTunnel(const Serial: string);
var
  Port: Word;
begin
  FLock.Enter;
  try
    if FTunnels.TryGetValue(Serial, Port) then
    begin
      if Assigned(FIBCloud_del_port) then
        FIBCloud_del_port(Port);
      FTunnels.Remove(Serial);
      OutputDebugString(PChar('[P2P] Túnel fechado: ' + Serial));
    end;
  finally
    FLock.Leave;
  end;
end;

procedure TIntelbrasP2P.CloseAllTunnels;
var
  Pair: TPair<string, Word>;
begin
  FLock.Enter;
  try
    for Pair in FTunnels do
    begin
      if Assigned(FIBCloud_del_port) then
        FIBCloud_del_port(Pair.Value);
    end;
    FTunnels.Clear;
    OutputDebugString('[P2P] Todos os túneis fechados');
  finally
    FLock.Leave;
  end;
end;

function TIntelbrasP2P.GetTunnelPort(const Serial: string): Integer;
var
  Port: Word;
begin
  FLock.Enter;
  try
    if FTunnels.TryGetValue(Serial, Port) then
      Result := Port
    else
      Result := -1;
  finally
    FLock.Leave;
  end;
end;

function TIntelbrasP2P.HasTunnel(const Serial: string): Boolean;
begin
  FLock.Enter;
  try
    Result := FTunnels.ContainsKey(Serial);
  finally
    FLock.Leave;
  end;
end;

initialization
  P2PManager := TIntelbrasP2P.Create;

finalization
  P2PManager.Free;

end.
```

### Exemplo de Uso

```pascal
// ========== INICIALIZAÇÃO (FormCreate) ==========
procedure TFormMain.FormCreate(Sender: TObject);
begin
  if not P2PManager.Initialize then
    ShowMessage('Erro ao inicializar módulo P2P');
end;

// ========== CONECTAR A UM DVR P2P ==========
function TFormMain.ConectarDVR(const Serial, Usuario, Senha: string): Boolean;
var
  Porta: Integer;
  LoginID: Integer;
  ErrorCode: Integer;
begin
  Result := False;
  
  // Criar túnel P2P
  Porta := P2PManager.CreateTunnel(Serial);
  
  if Porta < 0 then
  begin
    ShowMessage('DVR offline ou não encontrado: ' + Serial);
    Exit;
  end;
  
  // Conectar SDK Dahua no túnel local
  LoginID := CLIENT_Login(
    PAnsiChar('127.0.0.1'),           // Sempre localhost!
    Porta,                             // Porta do túnel
    PAnsiChar(AnsiString(Usuario)),
    PAnsiChar(AnsiString(Senha)),
    @FDeviceInfo,
    @ErrorCode
  );
  
  if LoginID > 0 then
  begin
    // SUCESSO!
    FLoginID := LoginID;
    FSerial := Serial;
    
    // Iniciar preview no primeiro canal
    CLIENT_RealPlay(LoginID, 0, PanelVideo.Handle);
    
    Result := True;
  end
  else
  begin
    // Falha - fechar túnel
    P2PManager.CloseTunnel(Serial);
    ShowMessage('Erro de login: ' + IntToStr(ErrorCode));
  end;
end;

// ========== DESCONECTAR ==========
procedure TFormMain.DesconectarDVR;
begin
  if FLoginID > 0 then
  begin
    CLIENT_Logout(FLoginID);
    FLoginID := 0;
  end;
  
  if FSerial <> '' then
  begin
    P2PManager.CloseTunnel(FSerial);
    FSerial := '';
  end;
end;

// ========== FINALIZAÇÃO (FormDestroy) ==========
procedure TFormMain.FormDestroy(Sender: TObject);
begin
  DesconectarDVR;
  P2PManager.Finalize;
end;
```

---

## 10. Integração com VMS

### Banco de Dados

Adicione campos para suportar conexão P2P:

```sql
CREATE TABLE dispositivos (
    id INTEGER PRIMARY KEY,
    nome VARCHAR(100),
    tipo_conexao VARCHAR(20),   -- 'IP' ou 'P2P'
    -- Campos para conexão IP
    ip VARCHAR(50),
    porta INTEGER,
    -- Campos para conexão P2P
    serial VARCHAR(50),
    -- Campos comuns
    usuario VARCHAR(50),
    senha VARCHAR(100),
    canais INTEGER
);
```

### Fluxo de Conexão

```
┌────────────────────────────────────────────────────────────┐
│              USUÁRIO CLICA EM "CONECTAR"                   │
└────────────────────────────┬───────────────────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────────────┐
│              tipo_conexao = 'P2P'?                         │
├─────────────────┬──────────────────────────────────────────┤
│       SIM       │                  NÃO                     │
└────────┬────────┴─────────────────────┬────────────────────┘
         │                              │
         ▼                              ▼
┌─────────────────────┐      ┌─────────────────────┐
│ porta = CreateTunnel│      │ Usar IP direto      │
│ ip = "127.0.0.1"    │      │ do banco de dados   │
└─────────┬───────────┘      └─────────┬───────────┘
         │                              │
         └──────────────┬───────────────┘
                        │
                        ▼
┌────────────────────────────────────────────────────────────┐
│           CLIENT_Login(ip, porta, user, pass)              │
└────────────────────────────────────────────────────────────┘
```

---

## 11. Múltiplos Dispositivos

O sistema gerencia automaticamente múltiplos túneis simultâneos:

| DVR | Serial | Porta Local | Status |
|-----|--------|-------------|--------|
| Cliente 1 | OJHL0700323ZS | 17777 | ✅ Conectado |
| Cliente 2 | 1ZRI1004554LZ | 17778 | ✅ Conectado |
| Cliente 3 | DOJ0002540617 | 17779 | ✅ Conectado |
| ... | ... | ... | ... |

### Limites

- **Máximo recomendado:** 100 túneis simultâneos
- **Faixa de portas:** 17777 a 17876
- **Memória:** ~1MB por túnel

---

## 12. Troubleshooting

### Tabela de Erros

| Erro | Causa | Solução |
|------|-------|---------|
| `BadImageFormatException` | Arquitetura 32/64 bit errada | Compilar VMS como **32-bit (x86)** |
| `DllNotFoundException` | DLL não encontrada | Copiar **TODAS** as DLLs para pasta do EXE |
| `IBCloud_query = -1` | DVR offline | Verificar se DVR está conectado à internet |
| `IBCloud_add_port = -1` | Túnel já existe ou DVR offline | Usar `GetTunnelPort()` primeiro |
| `CLIENT_Login falha` | Credenciais erradas | Verificar usuário/senha do DVR |
| **Túnel cai após segundos** | Shutdown automático | Chamar `IBCloud_automatic_shutdown(0)` **ANTES** de init! |
| `Timeout de conexão` | Timeout muito curto | Chamar `IBCloud_connection_timeout(60000)` |
| Vídeo não aparece | Handle inválido | Verificar se Handle do controle existe |

### Checklist de Integração

```
[ ] Todas as DLLs na pasta do executável
[ ] Projeto compilado como 32-bit (x86)
[ ] IBCloud_automatic_shutdown(0) chamado ANTES de init
[ ] IBCloud_connection_timeout(60000) configurado
[ ] IBCloud_init() chamado UMA ÚNICA VEZ
[ ] Aguardar 3 segundos após init
[ ] IBCloud_query() antes de criar túnel
[ ] CLIENT_Login() usando IP "127.0.0.1"
[ ] CloseTunnel() ao desconectar
[ ] Shutdown() ao fechar aplicação
```

---

## 13. Histórico de Descobertas

### Problema Original
- Túnel P2P era criado mas caía imediatamente
- SIMNext reportava "Tempo de conexão excedido"

### Processo de Investigação

1. **Wireshark:** Captura de tráfego revelou servidores P2P
   - 38.250.250.12:1250
   - 38.250.250.32:1252

2. **Análise de DLLs:** `strings` em IBCloudSDK.dll revelou:
   - Servidor: `intelbrasp2p.com.br`
   - Chave: `p2pintelbras2014543`

3. **Análise de Exports:** `objdump` revelou funções críticas:
   - `IBCloud_automatic_shutdown`
   - `IBCloud_connection_timeout`

4. **Solução Final:**
   ```
   IBCloud_automatic_shutdown(0)  ← DESABILITA SHUTDOWN!
   IBCloud_connection_timeout(60000)
   IBCloud_init()
   ```

### Teste Final Bem-Sucedido

```
Dispositivo: iNVD 1016
Serial: OJHL0700323ZS
Status no SIMNext: 🟢 ONLINE
Tipo de conexão: IP/Domínio (via túnel P2P)
IP: 127.0.0.1
Porta: 17777
```

---

## 📄 Licença

Este documento é para fins educacionais e de integração.
As DLLs são propriedade da Intelbras/Dahua.

---

## 🙏 Créditos

- Análise de DLLs e protocolo
- Testes com Wireshark
- Engenharia reversa do IBCloudSDK

---

```
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║   📅 Última atualização: Janeiro 2026                            ║
║   ✅ Status: TESTADO E FUNCIONANDO                               ║
║   🎯 Testado com: SIMNext + iNVD 1016                           ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
```
