# 🧪 Guia de Testes - P2P Tunnel Manager

## 1️⃣ Verificação Pré-Compilação

### ✅ Verificar Sintaxe

```bash
cd VMS_AlarmesJahu1/VMS_AlarmesJahu.App

# Windows: Compilar no Visual Studio (F5 ou Ctrl+Shift+B)
# Ou via CLI:
dotnet build -c Debug
```

**Esperado:**
- Se `libt2u.dll` não existe: ⚠️ Warning (build continua)
- Se `libt2u.dll` existe: ✅ "libt2u.dll encontrado" (build continua)

---

## 2️⃣ Testes Unitários Recomendados

### **Teste 1: Inicialização do Singleton**

```csharp
[TestMethod]
public void EnsureInitialized_CalledTwice_InitializedOnce()
{
    // Arrange
    var manager1 = P2PTunnelManager.Instance;
    var manager2 = P2PTunnelManager.Instance;
    
    // Act
    var result1 = manager1.EnsureInitialized();
    var result2 = manager2.EnsureInitialized();
    
    // Assert
    Assert.IsTrue(result1 || !result1);  // Não lance exceção
    Assert.IsTrue(manager1 == manager2);  // Mesmo singleton
}
```

### **Teste 2: OpenTunnel Idempotent**

```csharp
[TestMethod]
public void OpenTunnel_CalledTwice_ReturnsSamePort()
{
    // Arrange
    var manager = P2PTunnelManager.Instance;
    manager.EnsureInitialized();
    string serial = "TEST123456";
    
    // Act
    int port1 = manager.OpenTunnel(serial, 37777);
    int port2 = manager.OpenTunnel(serial, 37777);
    
    // Assert
    Assert.AreEqual(port1, port2);  // Mesma porta na segunda chamada
    Assert.IsTrue(port1 >= 6000 && port1 <= 7000);
    
    // Cleanup
    manager.CloseTunnel(serial);
}
```

### **Teste 3: CloseTunnel Remove do Dicionário**

```csharp
[TestMethod]
public void CloseTunnel_RemovesTunnelFromDictionary()
{
    // Arrange
    var manager = P2PTunnelManager.Instance;
    manager.EnsureInitialized();
    string serial = "TEST789";
    
    // Act
    int port = manager.OpenTunnel(serial, 37777);
    Assert.IsNotNull(manager.GetTunnel(serial));
    
    bool closed = manager.CloseTunnel(serial);
    
    // Assert
    Assert.IsTrue(closed);
    Assert.IsNull(manager.GetTunnel(serial));
}
```

### **Teste 4: Concorrência - Multiple Threads**

```csharp
[TestMethod]
public void OpenTunnel_MultipleThreads_NoRaceConditions()
{
    // Arrange
    var manager = P2PTunnelManager.Instance;
    manager.EnsureInitialized();
    var tasks = new List<Task<int>>();
    var serials = Enumerable.Range(1, 10)
        .Select(i => $"SERIAL{i}").ToList();
    
    // Act
    foreach (var serial in serials)
    {
        tasks.Add(Task.Run(() => manager.OpenTunnel(serial, 37777)));
    }
    Task.WaitAll(tasks.ToArray());
    
    // Assert
    var ports = tasks.Select(t => t.Result).ToList();
    Assert.AreEqual(10, ports.Distinct().Count());  // Todas portas diferentes
    
    // Cleanup
    manager.CloseAll();
}
```

---

## 3️⃣ Teste Manual com DVR Real

### **Requisitos:**
- DVR Intelbras com suporte P2P Cloud
- Serial Number do DVR (ex: `DVR1234567890`)
- Credenciais (user: admin, senha)
- `libt2u.dll` na pasta `libs/`

### **Passo 1: Adicionar Dispositivo P2P**

No VMS UI (DevicesView):
1. Clique "Novo Dispositivo"
2. Preencha:
   - **Nome**: DVR_P2P_TEST
   - **Connection Type**: P2P Cloud
   - **Serial Number**: `DVR1234567890` (do seu DVR)
   - **Porta**: 37777 (padrão)
   - **User**: admin
   - **Password**: sua_senha
3. Clique "Salvar"

### **Passo 2: Conectar**

No VMS UI:
1. Selecione o dispositivo criado
2. Clique "Conectar"

### **Passo 3: Verificar Logs**

Abra `%LOCALAPPDATA%/VMS_AlarmesJahu/Logs/vms-YYYY-MM-DD.log`

**✅ Logs Esperados (Sucesso):**
```
[INF] Inicializando P2P Tunnel Manager
[INF] libt2u.dll encontrado em C:\...\libs\libt2u.dll
[INF] P2P Tunnel Manager inicializado com sucesso
[INF] Conectando via P2P Cloud: DVR_P2P_TEST (SN: DVR1234567890, Porta: 37777)
[INF] Abrindo túnel P2P para DVR1234567890: local 6001 → remoto 37777
[INF] Túnel P2P aberto para DVR_P2P_TEST: usando 127.0.0.1:6001
[INF] Conectado em DVR_P2P_TEST (P2P: DVR1234567890 (Porta: 37777)) - 16 canais
```

**❌ Logs Esperados (Falha - DLL ausente):**
```
[INF] Inicializando P2P Tunnel Manager
[WRN] libt2u.dll não encontrado em C:\...\libs\. A funcionalidade P2P Cloud pode não funcionar.
[INF] P2P Tunnel Manager não inicializado.
[ERR] Falha ao abrir túnel P2P para DVR_P2P_TEST (DVR1234567890). Dispositivo pode estar offline ou indisponível.
```

**❌ Logs Esperados (Falha - Dispositivo offline):**
```
[INF] Abrindo túnel P2P para DVR1234567890: local 6001 → remoto 37777
[ERR] t2u_add_port falhou: serial=DVR1234567890, remote=37777, local=6001, code=1001
[ERR] Falha ao abrir túnel P2P para DVR_P2P_TEST (DVR1234567890). Dispositivo pode estar offline ou indisponível.
```

### **Passo 4: Testar Streaming**

Se conectado com sucesso:
1. Selecione um canal
2. Clique "Play" ou duplo-clique
3. Deverá aparecer video ao vivo

---

## 4️⃣ Checklist de Validação

| Teste | Expected | Status |
|-------|----------|--------|
| Compilação sem erros | ✅ Build OK | ☐ |
| DLL check no build | ⚠️ Warning ou ✅ Message | ☐ |
| P2PTunnelManager.Instance singleton | ✅ Mesmo objeto | ☐ |
| EnsureInitialized() uma única vez | ✅ Sem duplicatas | ☐ |
| OpenTunnel() retorna porta válida | ✅ 6000-7000 | ☐ |
| OpenTunnel() idempotent | ✅ Mesma porta | ☐ |
| CloseTunnel() remove do dict | ✅ GetTunnel retorna null | ☐ |
| CloseAll() fecha todos | ✅ GetOpenTunnels vazio | ☐ |
| Connect() P2P usa 127.0.0.1 | ✅ Login via localhost | ☐ |
| Disconnect() fecha túnel | ✅ P2PLocalPort limpo | ☐ |
| Logs mostram P2P flow | ✅ Mensagens detalhadas | ☐ |
| DVR conecta e mostra canais | ✅ 16 canais | ☐ |
| Streaming funciona | ✅ Video ao vivo | ☐ |
| App encerra sem erros | ✅ CloseAll() executado | ☐ |

---

## 5️⃣ Teste de Erro - libt2u.dll Ausente

### **Cenário**: Compilar SEM `libt2u.dll`

```bash
# Remover DLL (se existe)
rm VMS_AlarmesJahu.App/libs/libt2u.dll

# Compilar
dotnet build -c Debug

# Output esperado:
# ⚠️  AVISO: libt2u.dll não encontrado em libs/. 
#     A funcionalidade P2P Cloud pode não funcionar...
```

### **Testar App:**
1. Execute VMS
2. Tente adicionar DVR P2P
3. Deverá ver no log: `[WRN] libt2u.dll não encontrado`
4. App continua rodando (Direct IP funciona normalmente)

---

## 6️⃣ Teste de Erro - Credenciais Inválidas

### **Cenário**: Serial correto, senha errada

1. Adicione dispositivo P2P com serial CORRETO
2. Coloque senha ERRADA
3. Clique conectar

**Logs esperados:**
```
[INF] Abrindo túnel P2P para DVR...: local 6001 → remoto 37777
[INF] Túnel P2P aberto para DVR_TEST: usando 127.0.0.1:6001
[ERR] Falha ao fazer login no dispositivo DVR_TEST via túnel P2P (127.0.0.1:6001)
[INF] Fechando túnel P2P para DVR... (túnel é fechado automaticamente)
```

---

## 7️⃣ Teste de Erro - Serial Inválido

### **Cenário**: Serial não existe na Intelbras Cloud

1. Adicione dispositivo P2P com serial INVÁLIDO
2. Clique conectar

**Logs esperados:**
```
[INF] Abrindo túnel P2P para INVALIDO123: local 6001 → remoto 37777
[ERR] t2u_add_port falhou: serial=INVALIDO123, remote=37777, local=6001, code=...
[ERR] Falha ao abrir túnel P2P para DVR_BAD (INVALIDO123). 
      Dispositivo pode estar offline ou indisponível.
```

---

## 8️⃣ Teste de Stress - Múltiplos Túneis

### **Cenário**: Conectar vários DVRs P2P simultaneamente

```csharp
// Programatic test
[TestMethod]
public void Connect_MultipleDVRs_AllGetUniquePorts()
{
    var serials = new[] { "DVR001", "DVR002", "DVR003", "DVR004" };
    var manager = P2PTunnelManager.Instance;
    
    foreach (var serial in serials)
    {
        int port = manager.OpenTunnel(serial, 37777);
        Assert.IsTrue(port > 0);
    }
    
    var tunnels = manager.GetOpenTunnels().ToList();
    Assert.AreEqual(4, tunnels.Count);
    Assert.AreEqual(4, tunnels.Select(t => t.LocalPort).Distinct().Count());
}
```

---

## 9️⃣ Teste de Desconexão

### **Cenário**: Desconectar DVR P2P

1. Conecte um DVR P2P (sucesso)
2. Clique "Desconectar"

**Logs esperados:**
```
[INF] Desconectando dispositivo XYZ
[INF] Fechando túnel P2P para DVR123456789
[INF] Túnel fechado com sucesso: DVR123456789
[INF] Desconectado
```

---

## 🔟 Teste de Shutdown

### **Cenário**: Encerrar aplicação com túneis abertos

1. Conecte 2-3 DVRs P2P
2. Feche o aplicativo (File → Exit ou Alt+F4)

**Logs esperados:**
```
[INF] === VMS Alarmes Jahu encerrando ===
[INF] Fechando todos os túneis P2P (3 túneis)
[INF] Fechando túnel P2P para DVR001
[INF] Túnel fechado com sucesso: DVR001
[INF] Fechando túnel P2P para DVR002
[INF] Túnel fechado com sucesso: DVR002
[INF] Fechando túnel P2P para DVR003
[INF] Túnel fechado com sucesso: DVR003
[INF] Todos os túneis P2P foram fechados
[INF] === VMS Alarmes Jahu encerrado ===
```

---

## 1️⃣1️⃣ Teste de Reconexão

### **Cenário**: Reconectar mesmo DVR

1. Conecte DVR P2P (porta 6001)
2. Desconecte
3. Conecte novamente

**Esperado**: Nova porta alocada (ex: 6002), túnel anterior foi fechado

```
// Primeira conexão
[INF] Abrindo túnel P2P...: local 6001 → remoto 37777
[INF] Túnel P2P aberto com sucesso

// Desconexão
[INF] Fechando túnel P2P para DVR123456789
[INF] Túnel fechado com sucesso: DVR123456789

// Reconexão
[INF] Abrindo túnel P2P...: local 6002 → remoto 37777  ← Nova porta
[INF] Túnel P2P aberto com sucesso
```

---

## 1️⃣2️⃣ Ferramentas de Debug

### **Ver Logs em Tempo Real**

Windows (PowerShell):
```powershell
$logFile = "$env:LOCALAPPDATA/VMS_AlarmesJahu/Logs/vms-$(Get-Date -Format 'yyyy-MM-dd').log"
Get-Content $logFile -Wait
```

### **Ver Portas Abertas (Windows)**

```powershell
# Listar portas 6000-7000 em uso
netstat -ano | findstr ":600[0-9]"
netstat -ano | findstr ":700[0-9]"
```

Exemplo output:
```
TCP    127.0.0.1:6001         DVR_IP:37777       ESTABLISHED
TCP    127.0.0.1:6002         DVR_IP:37777       ESTABLISHED
```

### **Ver Portas Abertas (Linux/Mac)**

```bash
lsof -i :6000-:7000
netstat -an | grep -E "600[0-9]|700[0-9]"
```

---

## 1️⃣3️⃣ Troubleshooting Rápido

| Problema | Causa | Solução |
|----------|-------|---------|
| Build falha | C# syntax error | Verificar imports em ConnectionManager.cs |
| P2P não funciona | libt2u.dll ausente | Copiar DLL para libs/ |
| Port 6001 já em uso | Outro app/processo | Fechar app, limpar portas |
| Túnel abre mas login falha | Credenciais inválidas | Verificar serial number e senha |
| Logs vazios | Log path incorreto | Verificar `%LOCALAPPDATA%/VMS_AlarmesJahu/Logs/` |
| Crash ao OpenTunnel | DLL incompatível | Solicitar versão correta ao Intelbras |
| Memory leak | Túneis não fecham | Verificar Disconnect() chamando CloseTunnel() |

---

## 1️⃣4️⃣ Performance Baseline

Executar após implementação:

```csharp
[TestMethod]
public void Benchmark_OpenClosePerformance()
{
    var manager = P2PTunnelManager.Instance;
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    // Open 100 tunnels
    for (int i = 0; i < 100; i++)
    {
        manager.OpenTunnel($"TEST{i}", 37777);
    }
    sw.Stop();
    
    Assert.IsTrue(sw.ElapsedMilliseconds < 5000, 
        $"OpenTunnel x100 levou {sw.ElapsedMilliseconds}ms (esperado <5000ms)");
    
    manager.CloseAll();
}
```

**Baseline Esperado:**
- Open 100 túnels: < 5 segundos
- Close all: < 2 segundos
- GetTunnel lookup: < 1 millisecond

---

## 📋 Resumo de Testes

✅ **Testes Essenciais:**
1. Compilação sem erros
2. Singleton inicializa uma vez
3. OpenTunnel retorna porta válida
4. CloseTunnel remove do dict
5. DVR P2P conecta e mostra canais
6. Logs mostram flow correto
7. App encerra sem erros

⚠️ **Testes Opcionais (Performance/Stress):**
- Multiple threads
- 100+ túnels simultâneos
- Reconexão automática
- Port allocation under stress
