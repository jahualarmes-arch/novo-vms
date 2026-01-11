# 🔧 AutoRegister - Registro Automático de Dispositivos

## 📋 Visão Geral

O **AutoRegister** é uma ferramenta complementar ao VMS que permite o registro automático de dispositivos Intelbras em massa. Ideal para instalações com múltiplos DVRs/NVRs.

---

## 🎯 Funcionalidades

- ✅ **Descoberta Automática**: Busca dispositivos na rede automaticamente
- ✅ **Registro em Massa**: Registra múltiplos dispositivos de uma vez
- ✅ **Configuração em Lote**: Aplica configurações para vários dispositivos
- ✅ **Exportação/Importação**: CSV para backup e migração
- ✅ **Cloud P2P**: Suporte para registro em Cloud P2P

---

## 📂 Localização

```
VMS_AlarmesJahu_COMPLETO/
└── AutoRegister/
    ├── AutoRegister/              # Projeto C# (Windows Forms)
    │   ├── AutoRegister.exe      # ← Executável pronto (em bin\x64Release)
    │   ├── AutoRegisterDemo.cs   # Código principal
    │   └── ...
    ├── NetSDKCS/                  # Biblioteca SDK C#
    └── AutoRegister.sln          # Solução Visual Studio
```

---

## 🚀 Como Usar

### Opção 1: Executar o Executável Pronto

```bash
# Navegue até:
AutoRegister\AutoRegister\bin\x64Release\

# Execute:
AutoRegister.exe
```

### Opção 2: Compilar do Código Fonte

```bash
# 1. Abra no Visual Studio:
AutoRegister\AutoRegister.sln

# 2. Compile:
Build → Build Solution

# 3. Execute:
Debug → Start (ou F5)
```

---

## 📖 Guia de Uso Passo a Passo

### 1. Descobrir Dispositivos na Rede

```
1. Abra o AutoRegister
2. Clique em "Descobrir Dispositivos" ou "Search Network"
3. Aguarde a busca (pode levar 30-60 segundos)
4. Lista de dispositivos encontrados será exibida
```

### 2. Registrar Dispositivo no Cloud P2P

```
1. Selecione um dispositivo da lista
2. Clique em "Registrar no Cloud" ou "Register to Cloud"
3. Preencha:
   - Usuário: admin (padrão)
   - Senha: senha do dispositivo
   - Servidor: servidor P2P Intelbras
4. Clique em "OK"
5. Aguarde confirmação
```

### 3. Configurar Múltiplos Dispositivos

```
1. Selecione múltiplos dispositivos (Ctrl+Clique)
2. Clique em "Configurar Selecionados"
3. Escolha as configurações:
   - Rede (IP, gateway, DNS)
   - Cloud P2P (habilitar/desabilitar)
   - Usuários e senhas
4. Aplique
```

### 4. Exportar Lista de Dispositivos

```
1. Clique em "Exportar" ou "Export"
2. Escolha local para salvar o CSV
3. Arquivo CSV contém:
   - IP, MAC, Modelo, Serial Number, Status, etc.
```

### 5. Importar Configurações

```
1. Prepare um arquivo CSV com as configurações
2. Clique em "Importar" ou "Import"
3. Selecione o arquivo CSV
4. Confirme a importação
```

---

## 📄 Formato do CSV

### Exemplo de CSV de Dispositivos:

```csv
IP,MAC,SerialNumber,Model,Status,CloudEnabled,Username,Password
192.168.1.108,00:12:34:56:78:9A,1ZRI1004554LZ,MHDX 1116,Online,True,admin,12345
192.168.1.109,00:12:34:56:78:9B,1ZRI1004554MA,MHDX 1216,Online,True,admin,12345
192.168.1.110,00:12:34:56:78:9C,1ZRI1004554MB,NVR 1108,Offline,False,admin,admin
```

---

## 🔑 Campos Importantes

| Campo | Descrição | Exemplo |
|-------|-----------|---------|
| IP | Endereço IP do dispositivo | 192.168.1.108 |
| MAC | Endereço MAC | 00:12:34:56:78:9A |
| SerialNumber | Número de série | 1ZRI1004554LZ |
| Model | Modelo do dispositivo | MHDX 1116 |
| Status | Status atual | Online/Offline |
| CloudEnabled | Cloud P2P habilitado | True/False |
| Username | Usuário de acesso | admin |
| Password | Senha de acesso | ***** |

---

## ⚙️ Configurações Avançadas

### Habilitar Cloud P2P em Massa

```
1. Selecione todos os dispositivos desejados
2. Clique em "Configurações de Cloud"
3. Marque "Habilitar Cloud P2P"
4. Configure servidor P2P:
   - Servidor: p2p.intelbras.com.br (padrão)
   - Porta: 34567 (padrão)
5. Aplique para todos
```

### Alterar Senhas em Massa

```
1. Selecione dispositivos
2. Clique em "Configurações de Segurança"
3. Nova senha para admin: [digite]
4. Confirme
5. Aplique
```

### Configurar Rede em Massa

```
1. Selecione dispositivos
2. Clique em "Configurações de Rede"
3. Configure:
   - DHCP ou IP Fixo
   - Gateway
   - DNS (8.8.8.8 recomendado)
4. Aplique
```

---

## 🔍 Troubleshooting

### Problema: "Nenhum dispositivo encontrado"

**Soluções:**
- Verifique se os dispositivos estão na mesma rede
- Desabilite temporariamente o firewall
- Certifique-se que os dispositivos estão ligados
- Aumente o timeout de busca nas configurações

### Problema: "Falha ao registrar no Cloud"

**Soluções:**
- Verifique credenciais (usuário/senha)
- Certifique-se que o dispositivo tem internet
- Verifique se o servidor P2P está acessível
- Tente registrar manualmente no DVR primeiro

### Problema: "Acesso negado ao configurar"

**Soluções:**
- Verifique se o usuário tem permissões de administrador
- Senha pode estar incorreta
- Dispositivo pode estar bloqueado (muitas tentativas falhas)

---

## 📚 Integração com VMS

### Fluxo de Trabalho Recomendado:

```
1. Use AutoRegister para:
   ✅ Descobrir dispositivos na rede
   ✅ Registrar todos no Cloud P2P
   ✅ Exportar lista em CSV

2. Use o VMS para:
   ✅ Importar lista de dispositivos (futuro recurso)
   ✅ Monitoramento e visualização
   ✅ Gravação e eventos
```

---

## 🛠️ Requisitos do Sistema

- **SO**: Windows 7/8/10/11 (64-bit)
- **Framework**: .NET Framework 4.7.2 ou superior
- **Rede**: Mesma rede dos dispositivos (para descoberta)
- **DLLs**: Incluídas no executável

---

## 📝 Dicas de Uso

### 1. Antes de Registrar em Massa
- Teste com 1-2 dispositivos primeiro
- Verifique se todos têm acesso à internet
- Anote as senhas antes de alterar

### 2. Para Instalações Grandes (10+ DVRs)
- Organize em planilha Excel primeiro
- Use nomenclatura padronizada
- Documente IPs e localizações

### 3. Backup de Configurações
- Sempre exporte CSV antes de alterações em massa
- Mantenha backup das senhas
- Documente servidor P2P usado

---

## ⚠️ Avisos Importantes

### Segurança
- ⚠️ **Não compartilhe** arquivos CSV com senhas
- ⚠️ **Altere senhas padrão** imediatamente
- ⚠️ **Use senhas fortes** para Cloud P2P

### Cloud P2P
- ⚠️ Certifique-se que os dispositivos têm **internet estável**
- ⚠️ Verifique se a **operadora não bloqueia** P2P
- ⚠️ **Registre dispositivos** antes de levá-los para instalação remota

---

## 🔄 Atualizações

O AutoRegister pode ser atualizado separadamente do VMS. Verifique com a Intelbras por atualizações do SDK.

---

## 📞 Suporte

### AutoRegister / SDK
- **Intelbras**: 0800 570 0810
- **Site**: https://www.intelbras.com/pt-br

### VMS
- Consulte documentação do VMS
- Verifique logs em `logs/`

---

## 📖 Documentação Adicional

- Consulte o manual do DVR/NVR específico
- Documentação do SDK Intelbras
- Guia de Cloud P2P da Intelbras

---

**Versão**: 1.0  
**Compatibilidade**: DVRs/NVRs Intelbras MHDX, NVR  
**Última atualização**: Janeiro 2026

---

**Dica**: Use o AutoRegister para configuração inicial em massa, e o VMS para operação diária! 🚀
