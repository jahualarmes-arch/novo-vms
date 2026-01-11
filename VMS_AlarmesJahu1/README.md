# 🎥 VMS - Video Management System v2.0

## Sistema Profissional de Monitoramento de Vídeo para DVRs Intelbras

![Status](https://img.shields.io/badge/Status-Ativo-success)
![Versão](https://img.shields.io/badge/Versão-2.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Plataforma](https://img.shields.io/badge/Plataforma-Windows-lightgrey)

---

## 📋 Sobre o Projeto

O **VMS (Video Management System)** é um sistema completo de gerenciamento e visualização de vídeo desenvolvido para trabalhar com DVRs Intelbras. Suporta tanto **conexão direta via IP** quanto **conexão remota via Cloud P2P**.

### ✨ Destaques da Versão 2.0

- ✅ **Conexão P2P Cloud** totalmente funcional com diagnóstico avançado
- ✅ **Logs detalhados** e mensagens de erro descritivas
- ✅ **Timeouts otimizados** para conexões remotas
- ✅ **Reconexão automática** em caso de queda
- ✅ **Interface moderna** com Material Design
- ✅ **Suporte múltiplos DVRs** simultâneos

---

## 🚀 Início Rápido

### Instalação

1. Clone ou extraia o projeto
2. Abra `VMS_AlarmesJahu.App.sln` no Visual Studio 2022
3. **Todas as DLLs necessárias já estão incluídas!** ✅
4. Compile e execute (F5)

**Ou veja**: [`INSTALACAO.md`](INSTALACAO.md) para instruções detalhadas

### Primeiro Uso

1. Execute o VMS
2. Acesse "Dispositivos" no menu
3. Adicione um DVR (IP Direto ou P2P Cloud)
4. Conecte e visualize!

### 🔧 AutoRegister Incluído

Para registro automático de dispositivos em massa:
- Navegue até: `AutoRegister/AutoRegister/bin/x64Release/`
- Execute: `AutoRegister.exe`
- Consulte: [`AUTOREGISTER.md`](AUTOREGISTER.md)

---

## 📸 Screenshots

### Dashboard
```
┌─────────────────────────────────────┐
│  📊 VMS - Dashboard                 │
├─────────────────────────────────────┤
│  Dispositivos: 3 conectados         │
│  Canais Ativos: 48                  │
│  Uptime: 02:34:12                   │
└─────────────────────────────────────┘
```

### Visualização em Grade
```
┌────────┬────────┬────────┬────────┐
│  CH1   │  CH2   │  CH3   │  CH4   │
├────────┼────────┼────────┼────────┤
│  CH5   │  CH6   │  CH7   │  CH8   │
├────────┼────────┼────────┼────────┤
│  CH9   │  CH10  │  CH11  │  CH12  │
├────────┼────────┼────────┼────────┤
│  CH13  │  CH14  │  CH15  │  CH16  │
└────────┴────────┴────────┴────────┘
```

---

## 🎯 Recursos Principais

### Conexão
- ✅ **IP Direto**: Conexão via rede local (LAN)
- ✅ **Cloud P2P**: Acesso remoto via Internet
- ✅ **Multi-DVR**: Múltiplos dispositivos simultâneos
- ✅ **Auto-Reconnect**: Reconexão automática

### Visualização
- ✅ **Grades Configuráveis**: 1x1, 2x2, 3x3, 4x4
- ✅ **Fullscreen**: Modo tela cheia
- ✅ **Snapshot**: Captura de imagens
- ✅ **Streaming Real-time**: Vídeo ao vivo

### Gerenciamento
- ✅ **Dashboard**: Estatísticas e monitoramento
- ✅ **Dispositivos**: CRUD completo de DVRs
- ✅ **Logs**: Sistema de logging robusto
- ✅ **BD Local**: LiteDB para persistência

---

## 🔧 Tecnologias Utilizadas

- **Framework**: .NET 8.0 (WPF)
- **Arquitetura**: MVVM (Model-View-ViewModel)
- **UI**: Material Design Themes
- **Logging**: Serilog
- **Database**: LiteDB
- **SDK**: Intelbras dhnetsdk.dll

### Pacotes NuGet

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="LiteDB" Version="5.0.17" />
<PackageReference Include="MaterialDesignThemes" Version="5.0.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

---

## 📁 Estrutura do Projeto

```
VMS_AlarmesJahu_COMPLETO/
│
├── VMS_AlarmesJahu.App/          # Aplicação principal
│   ├── Sdk/                      # SDK Intelbras
│   ├── Models/                   # Modelos de dados
│   ├── ViewModels/               # ViewModels (MVVM)
│   ├── Views/                    # Views (XAML)
│   ├── Services/                 # Serviços (ConnectionManager, etc)
│   ├── Data/                     # Repositórios e acesso a dados
│   └── Resources/                # Recursos (ícones, imagens)
│
├── INSTALACAO.md                 # Guia de instalação
├── DIAGNOSTICO_P2P.md            # Troubleshooting P2P
├── CHECKLIST_DIAGNOSTICO.md      # Checklist de diagnóstico
├── README_CORRECAO.md            # Detalhes das correções v2.0
└── README.md                     # Este arquivo
```

---

## 🔍 Solução de Problemas

### Problema: "Falha na conexão P2P"

**Leia**: [`DIAGNOSTICO_P2P.md`](DIAGNOSTICO_P2P.md)

**Principais causas**:
1. DVR offline ou sem internet
2. Cloud P2P desabilitado no DVR
3. Número de série incorreto
4. Credenciais inválidas

### Problema: "dhnetsdk.dll não encontrada"

**Solução**:
- Certifique-se de que `dhnetsdk.dll` está na pasta do executável
- Verifique nas propriedades do arquivo: "Copy to Output Directory" = "Copy if newer"

### Mais Problemas?

Consulte:
- [`CHECKLIST_DIAGNOSTICO.md`](CHECKLIST_DIAGNOSTICO.md) - Checklist rápido
- Logs em: `logs/vms-YYYY-MM-DD.log`

---

## 📊 Códigos de Erro Comuns

| Código | Descrição | Solução |
|--------|-----------|---------|
| 3 | Erro de rede | Verifique internet |
| 6 | Senha incorreta | Verifique credenciais |
| 7 | Timeout | DVR não responde |
| 11 | Dispositivo offline | DVR está offline |
| 33 | SN inválido | Verifique número de série |

**Tabela completa**: Veja logs ou [`DIAGNOSTICO_P2P.md`](DIAGNOSTICO_P2P.md)

---

## 🎓 Como Usar

### 1. Conectar via IP Direto

```plaintext
1. Clique em "Dispositivos"
2. Clique em "+" para adicionar
3. Preencha:
   - Nome: "DVR Local"
   - Tipo: "IP Direto"
   - Host: 192.168.1.108
   - Porta: 37777
   - Usuário: admin
   - Senha: *****
4. Salvar e Conectar
```

### 2. Conectar via P2P Cloud

```plaintext
1. Clique em "Dispositivos"
2. Clique em "+" para adicionar
3. Preencha:
   - Nome: "DVR Remoto"
   - Tipo: "Cloud P2P"
   - Número de Série: 1ZRI1004554LZ
   - Porta: 37777
   - Usuário: admin
   - Senha: *****
4. Salvar e Conectar
```

**IMPORTANTE**: Para P2P funcionar:
- DVR deve estar online
- Cloud P2P deve estar habilitado no DVR
- Número de série deve estar correto

---

## 🛡️ Requisitos do Sistema

### Mínimo
- **SO**: Windows 10 (64-bit)
- **RAM**: 4 GB
- **CPU**: Dual-core 2.0 GHz
- **.NET**: 8.0 Runtime

### Recomendado
- **SO**: Windows 11 (64-bit)
- **RAM**: 8 GB ou mais
- **CPU**: Quad-core 2.5 GHz ou superior
- **Rede**: 10 Mbps ou superior

---

## 📚 Documentação

- [`INSTALACAO.md`](INSTALACAO.md) - Guia completo de instalação
- [`DIAGNOSTICO_P2P.md`](DIAGNOSTICO_P2P.md) - Diagnóstico de problemas P2P
- [`CHECKLIST_DIAGNOSTICO.md`](CHECKLIST_DIAGNOSTICO.md) - Checklist rápido
- [`README_CORRECAO.md`](README_CORRECAO.md) - Detalhes das correções v2.0

---

## 🔄 Changelog

### v2.0.0 (Janeiro 2026)
- ✅ Suporte completo P2P Cloud com diagnóstico avançado
- ✅ Logs detalhados com 20+ códigos de erro traduzidos
- ✅ Timeouts otimizados (15 segundos para P2P)
- ✅ Validações robustas de entrada
- ✅ Reconexão automática
- ✅ Interface melhorada com Material Design
- ✅ Documentação completa

### v1.0.0 (2025)
- 🎯 Release inicial
- Conexão via IP Direto
- Visualização em grade
- Gerenciamento de dispositivos

---

## 🤝 Contribuindo

Este é um projeto proprietário, mas sugestões são bem-vindas!

---

## 📞 Suporte

### Intelbras
- **Telefone**: 0800 570 0810
- **Site**: https://www.intelbras.com/pt-br
- **Suporte**: suporte@intelbras.com.br

### Projeto
- **Issues**: Use os arquivos de diagnóstico incluídos
- **Logs**: Verifique `logs/vms-YYYY-MM-DD.log`

---

## 📄 Licença

Este software é proprietário e de uso restrito.

---

## 👥 Créditos

- **Desenvolvimento**: VMS Development Team
- **SDK**: Intelbras
- **UI Framework**: Material Design In XAML Toolkit
- **Logging**: Serilog Team

---

## ⭐ Agradecimentos

- Equipe Intelbras pelo SDK e suporte
- Comunidade .NET pelo ecossistema incrível
- Usuários pelo feedback e testes

---

**Versão**: 2.0.0  
**Última atualização**: Janeiro 2026  
**Status**: ✅ Produção

---

**Bom uso! 🚀**

Para começar, veja [`INSTALACAO.md`](INSTALACAO.md)
