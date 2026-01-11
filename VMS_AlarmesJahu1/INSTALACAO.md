# 📦 VMS - Video Management System v2.0

## Sistema de Monitoramento de Vídeo com Suporte P2P Cloud Intelbras

---

## 🚀 GUIA DE INSTALAÇÃO RÁPIDA

### Pré-requisitos

1. **Windows 10/11** (64-bit)
2. **.NET 8.0 Runtime** ou superior
3. **Visual Studio 2022** (para desenvolvimento)
4. **SDK Intelbras** (dhnetsdk.dll) - incluído no projeto

---

## 📋 INSTALAÇÃO

### Opção 1: Executar o Projeto (Desenvolvimento)

```bash
# 1. Abrir o projeto no Visual Studio 2022
# Abra o arquivo: VMS_AlarmesJahu.App.sln

# 2. Restaurar pacotes NuGet
# Botão direito na Solution → Restore NuGet Packages

# 3. Compilar
# Build → Build Solution (ou Ctrl+Shift+B)

# 4. Executar
# Debug → Start Debugging (ou F5)
```

### Opção 2: Executável Standalone

```bash
# 1. Compilar em modo Release
dotnet publish -c Release -r win-x64 --self-contained

# 2. O executável estará em:
# bin\Release\net8.0-windows\win-x64\publish\

# 3. Copie toda a pasta 'publish' para onde desejar

# 4. Execute VMS_AlarmesJahu.exe
```

---

## 🔧 CONFIGURAÇÃO INICIAL

### 1. Primeiro Uso

1. Execute o programa
2. A interface principal será exibida
3. Clique em "Dispositivos" no menu lateral

### 2. Adicionar DVR via IP Direto

1. Clique no botão **"+"** (Adicionar)
2. Preencha:
   - **Nome**: Nome do DVR (ex: "DVR Recepção")
   - **Tipo de Conexão**: **IP Direto**
   - **Host/IP**: IP do DVR (ex: 192.168.1.108)
   - **Porta**: 37777 (padrão)
   - **Usuário**: admin
   - **Senha**: senha do DVR
   - **Canais**: 16 (ou número de câmeras)
3. Clique em **"Salvar"**
4. Clique em **"Conectar"**

### 3. Adicionar DVR via P2P Cloud

1. Clique no botão **"+"** (Adicionar)
2. Preencha:
   - **Nome**: Nome do DVR (ex: "DVR Matriz")
   - **Tipo de Conexão**: **Cloud P2P**
   - **Número de Série**: 1ZRI1004554LZ (exemplo)
   - **Porta**: 37777 (padrão)
   - **Usuário**: admin
   - **Senha**: senha do DVR
   - **Canais**: 16 (ou número de câmeras)
3. Clique em **"Salvar"**
4. Clique em **"Conectar"**

**IMPORTANTE**: Para P2P funcionar, o DVR deve:
- ✅ Estar ONLINE e com internet
- ✅ Ter Cloud P2P **HABILITADO** (Menu → Rede → Cloud/P2P)
- ✅ Estar registrado no Cloud Intelbras

---

## 🎯 RECURSOS

### ✅ Conexão Dual
- Conexão via **IP Direto** (rede local)
- Conexão via **Cloud P2P** (acesso remoto)
- Suporte a **múltiplos DVRs** simultaneamente

### ✅ Visualização
- Grade de vídeos configurável (1x1, 2x2, 3x3, 4x4)
- Modo **Fullscreen** com duplo clique
- **Snapshot** (captura de imagem)
- Reconexão automática

### ✅ Gerenciamento
- Dashboard com estatísticas
- Gerenciamento de dispositivos
- Logs detalhados
- Banco de dados local (LiteDB)

---

## 📁 ESTRUTURA DO PROJETO

```
VMS_AlarmesJahu_COMPLETO/
├── VMS_AlarmesJahu.App/
│   ├── Sdk/                    # SDK Intelbras (CORRIGIDO)
│   │   └── IntelbrasSdk.cs
│   ├── Models/                 # Modelos de dados
│   │   └── Device.cs
│   ├── ViewModels/             # ViewModels (MVVM)
│   │   ├── MainViewModel.cs
│   │   ├── DevicesViewModel.cs
│   │   ├── DashboardViewModel.cs
│   │   └── ViewModelBase.cs
│   ├── Views/                  # Views (XAML)
│   │   ├── MainWindow.xaml
│   │   ├── DevicesView.xaml
│   │   ├── DashboardView.xaml
│   │   └── MosaicView.xaml
│   ├── Services/               # Serviços
│   │   └── ConnectionManager.cs
│   ├── Data/                   # Acesso a dados
│   │   └── DeviceRepository.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   └── VMS_AlarmesJahu.App.csproj
├── INSTALACAO.md               # Este arquivo
├── DIAGNOSTICO_P2P.md          # Guia de troubleshooting P2P
├── CHECKLIST_DIAGNOSTICO.md    # Checklist de diagnóstico
└── README.md                   # Documentação geral
```

---

## ⚙️ DEPENDÊNCIAS (NuGet)

O projeto usa os seguintes pacotes:

- **CommunityToolkit.Mvvm** 8.2.2 - MVVM Toolkit
- **LiteDB** 5.0.17 - Banco de dados local
- **MaterialDesignThemes** 5.0.0 - Interface Material Design
- **Serilog** 3.1.1 - Logging
- **Serilog.Sinks.Console** 5.0.1 - Logs no console
- **Serilog.Sinks.File** 5.0.0 - Logs em arquivo

Todos são restaurados automaticamente ao compilar.

---

## 🔍 TROUBLESHOOTING

### Erro: "dhnetsdk.dll não encontrada"

**Solução:**
- A DLL `dhnetsdk.dll` deve estar na pasta do executável
- Certifique-se de que o arquivo foi incluído no Build
- No Visual Studio: Propriedades do arquivo → Copy to Output Directory → Copy if newer

### Erro: "Falha na conexão P2P"

**Consulte**: `DIAGNOSTICO_P2P.md` e `CHECKLIST_DIAGNOSTICO.md`

**Principais causas:**
1. DVR offline ou sem internet
2. Cloud P2P desabilitado no DVR
3. Número de série incorreto
4. Credenciais inválidas
5. Porta incorreta

### Logs não aparecem

Os logs ficam em: `logs/vms-YYYY-MM-DD.log`

Também são exibidos no console (se executar via Visual Studio)

---

## 📝 ATALHOS DO TECLADO

- **F11** - Fullscreen (na visualização de vídeo)
- **ESC** - Sair do Fullscreen
- **Duplo Clique** - Fullscreen em um canal específico
- **Ctrl+S** - Salvar configurações

---

## 🆘 SUPORTE

### Documentação
- `README.md` - Visão geral do projeto
- `DIAGNOSTICO_P2P.md` - Solução de problemas P2P
- `CHECKLIST_DIAGNOSTICO.md` - Checklist de diagnóstico

### Contatos
- **Intelbras**: 0800 570 0810
- **Site**: https://www.intelbras.com/pt-br

---

## 📄 LICENÇA

Este software é proprietário e de uso restrito.

---

## 🔄 VERSÃO

**Versão**: 2.0.0  
**Data**: Janeiro 2026  
**Compatibilidade**: .NET 8.0, Windows 10/11  
**SDK Intelbras**: v3.x ou superior

---

## ✅ NOVIDADES DA VERSÃO 2.0

- ✅ **Suporte P2P Cloud** com diagnóstico avançado
- ✅ **Logs detalhados** mostrando exatamente onde falha
- ✅ **Timeouts otimizados** para conexões P2P (15 segundos)
- ✅ **20+ códigos de erro** traduzidos
- ✅ **Reconexão automática** em caso de queda
- ✅ **Validações robustas** de entrada de dados
- ✅ **Interface melhorada** com Material Design

---

**Bom uso! 🚀**

Se encontrar problemas, consulte os arquivos de diagnóstico incluídos.
