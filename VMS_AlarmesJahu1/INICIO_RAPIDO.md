# ⚡ INÍCIO RÁPIDO - VMS v2.0

## 🎯 5 Passos para Começar

---

## 1️⃣ Obter a DLL Intelbras

**IMPORTANTE**: Sem isso, nada funciona!

- Baixe `dhnetsdk.dll` do site da Intelbras
- Ou ligue: 0800 570 0810

**Onde colocar**: `VMS_AlarmesJahu.App/dhnetsdk.dll`

📖 Detalhes: [`IMPORTANTE_DLL.md`](IMPORTANTE_DLL.md)

---

## 2️⃣ Abrir o Projeto

```bash
# Abra no Visual Studio 2022:
VMS_AlarmesJahu.sln

# Ou via terminal:
cd VMS_AlarmesJahu_COMPLETO
dotnet restore
```

---

## 3️⃣ Compilar

```bash
# No Visual Studio:
Build → Build Solution (Ctrl+Shift+B)

# Ou via terminal:
dotnet build
```

---

## 4️⃣ Executar

```bash
# No Visual Studio:
Debug → Start Debugging (F5)

# Ou via terminal:
dotnet run --project VMS_AlarmesJahu.App
```

---

## 5️⃣ Adicionar seu Primeiro DVR

### Via IP Direto (Rede Local):

```
1. Clique em "Dispositivos"
2. Clique no botão "+"
3. Preencha:
   Nome: DVR Principal
   Tipo: IP Direto
   Host: 192.168.1.108
   Porta: 37777
   Usuário: admin
   Senha: [sua senha]
   Canais: 16
4. Salvar → Conectar
```

### Via P2P Cloud (Remoto):

```
1. Clique em "Dispositivos"
2. Clique no botão "+"
3. Preencha:
   Nome: DVR Remoto
   Tipo: Cloud P2P
   Número de Série: 1ZRI1004554LZ
   Porta: 37777
   Usuário: admin
   Senha: [sua senha]
   Canais: 16
4. Salvar → Conectar
```

**⚠️ Para P2P funcionar**:
- DVR deve estar ONLINE
- Cloud P2P deve estar HABILITADO no DVR
- Número de série deve estar CORRETO

---

## ✅ Pronto!

Se tudo funcionou:
- ✅ DVR aparece como "🟢 Conectado"
- ✅ Vídeos aparecem na tela
- ✅ Logs mostram "Login P2P bem-sucedido" ou "Login OK"

---

## ❌ Não Funcionou?

### Erro: "dhnetsdk.dll não encontrada"
➜ Veja: [`IMPORTANTE_DLL.md`](IMPORTANTE_DLL.md)

### Erro: "Falha na conexão P2P"
➜ Veja: [`DIAGNOSTICO_P2P.md`](DIAGNOSTICO_P2P.md)

### Erro: "Senha incorreta"
➜ Teste fazer login LOCALMENTE primeiro (via IP)

### Outros Problemas
➜ Consulte: [`CHECKLIST_DIAGNOSTICO.md`](CHECKLIST_DIAGNOSTICO.md)

---

## 📚 Documentação Completa

| Arquivo | Descrição |
|---------|-----------|
| [`README.md`](README.md) | Visão geral do projeto |
| [`INSTALACAO.md`](INSTALACAO.md) | Guia completo de instalação |
| [`DIAGNOSTICO_P2P.md`](DIAGNOSTICO_P2P.md) | Solução de problemas P2P |
| [`CHECKLIST_DIAGNOSTICO.md`](CHECKLIST_DIAGNOSTICO.md) | Checklist de diagnóstico |
| [`IMPORTANTE_DLL.md`](IMPORTANTE_DLL.md) | Sobre a DLL necessária |
| [`README_CORRECAO.md`](README_CORRECAO.md) | Correções da v2.0 |

---

## 🆘 Precisa de Ajuda?

1. **Verifique os logs**: `logs/vms-YYYY-MM-DD.log`
2. **Consulte a documentação** acima
3. **Suporte Intelbras**: 0800 570 0810

---

**Tempo estimado**: 10-15 minutos (incluindo download da DLL)

**Boa sorte! 🚀**
