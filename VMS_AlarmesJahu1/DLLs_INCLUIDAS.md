# 📚 SDK Intelbras - DLLs Incluídas

## 🎯 Visão Geral

Este projeto agora inclui **TODAS** as DLLs necessárias do SDK Intelbras, totalizando **37 MB** de bibliotecas nativas.

**Você NÃO precisa mais baixar nenhuma DLL separadamente!** ✅

---

## 📦 DLLs Incluídas (16 arquivos)

### 🔵 DLLs Principais (Core)

| Arquivo | Tamanho | Função |
|---------|---------|--------|
| **dhnetsdk.dll** | 15.7 MB | SDK principal - Conexão, login, streaming |
| **dhconfigsdk.dll** | 3.8 MB | Configuração de dispositivos |
| **dhplay.dll** | 4.7 MB | Decodificação e reprodução de vídeo |
| **avnetsdk.dll** | 4.3 MB | Codecs de áudio/vídeo |

### 🟢 DLLs de Funcionalidades Avançadas

| Arquivo | Tamanho | Função |
|---------|---------|--------|
| **fisheye.dll** | 652 KB | Suporte para câmeras fisheye (360°) |
| **ImageAlg.dll** | 50 KB | Algoritmos de processamento de imagem |
| **IvsDrawer.dll** | 207 KB | Desenho de análise inteligente (IVS) |

### 🟡 DLLs de Infraestrutura

| Arquivo | Tamanho | Função |
|---------|---------|--------|
| **Infra.dll** | 1.0 MB | Infraestrutura base |
| **NetFramework.dll** | 699 KB | Framework de rede |
| **Stream.dll** | 455 KB | Gerenciamento de streams |
| **StreamSvr.dll** | 1.9 MB | Servidor de streaming |
| **Json.dll** | 295 KB | Processamento JSON |

### 🔴 DLLs de Segurança (OpenSSL)

| Arquivo | Tamanho | Função |
|---------|---------|--------|
| **libeay32.dll** | 2.3 MB | OpenSSL - Criptografia |
| **ssleay32.dll** | 461 KB | OpenSSL - SSL/TLS |
| **libcurl.dll** | 367 KB | Cliente HTTP/HTTPS |

---

## 📍 Localização

### No Projeto:
```
VMS_AlarmesJahu.App/
└── libs/
    ├── dhnetsdk.dll
    ├── dhconfigsdk.dll
    ├── dhplay.dll
    ├── avnetsdk.dll
    ├── fisheye.dll
    ├── ImageAlg.dll
    ├── Infra.dll
    ├── IvsDrawer.dll
    ├── Json.dll
    ├── libcurl.dll
    ├── libeay32.dll
    ├── NetFramework.dll
    ├── ssleay32.dll
    ├── Stream.dll
    └── StreamSvr.dll
```

### Após Compilar:
```
bin/Release/net8.0-windows/
├── VMS_AlarmesJahu.exe
├── dhnetsdk.dll          ← Copiadas automaticamente
├── dhconfigsdk.dll       ← pelo build
├── dhplay.dll
└── ... (todas as outras DLLs)
```

---

## ✅ Configuração Automática

### No arquivo `.csproj`:

```xml
<!-- SDK Intelbras - Todas as DLLs necessárias -->
<ItemGroup>
  <None Include="libs\*.dll" 
        CopyToOutputDirectory="PreserveNewest" 
        CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

Isso garante que:
- ✅ Todas as DLLs são copiadas ao compilar (Debug/Release)
- ✅ Todas as DLLs são incluídas ao publicar (Publish)
- ✅ Não é necessário copiar manualmente

---

## 🔍 Verificação

### Como Verificar se Está Correto:

```bash
# 1. Após compilar, verifique a pasta de saída:
ls bin/Debug/net8.0-windows/*.dll

# Deve listar 15+ DLLs, incluindo:
# - dhnetsdk.dll
# - dhconfigsdk.dll
# - dhplay.dll
# - etc.
```

### No Visual Studio:

```
1. Build → Build Solution
2. Vá em: bin/Debug/net8.0-windows/
3. Verifique se as DLLs estão lá
```

---

## 🎯 Funções Específicas

### dhnetsdk.dll - SDK Principal

**Responsável por:**
- Login em dispositivos (IP direto e P2P Cloud)
- Conexão e autenticação
- Streaming de vídeo ao vivo
- Controle PTZ
- Captura de snapshots
- Gerenciamento de canais

**Usado por:** `IntelbrasSdk.cs`

### dhconfigsdk.dll - Configuração

**Responsável por:**
- Leitura de configurações do DVR/NVR
- Alteração de configurações
- Gerenciamento de usuários
- Configuração de gravação
- Configuração de alarmes

**Usado por:** AutoRegister e futuras features do VMS

### dhplay.dll - Reprodução

**Responsável por:**
- Decodificação de vídeo (H.264, H.265)
- Renderização na tela
- Controle de reprodução (play, pause, velocidade)
- Decodificação de áudio

**Usado por:** Visualização de vídeo ao vivo

### fisheye.dll - Câmeras 360°

**Responsável por:**
- Dewarping de imagens fisheye
- Visualização panorâmica
- Múltiplas janelas de visão

**Usado por:** Suporte futuro para câmeras fisheye

---

## ⚠️ Avisos Importantes

### Arquitetura

**Todas as DLLs são x64 (64-bit).**

Se precisar de x86 (32-bit):
- Baixe o SDK x86 da Intelbras
- Substitua as DLLs
- Altere o projeto para x86:
  ```xml
  <PlatformTarget>x86</PlatformTarget>
  ```

### Versão

**SDK Intelbras v3.x**

- Compatível com DVRs/NVRs atuais (2020+)
- Para dispositivos mais antigos, pode ser necessário SDK 2.x
- Verifique a compatibilidade no site da Intelbras

### Dependências

**OpenSSL incluído (libeay32.dll, ssleay32.dll)**

- Necessário para conexões seguras (HTTPS, SSL)
- Necessário para Cloud P2P
- **NÃO remover** estas DLLs

---

## 🔄 Atualização

### Como Atualizar o SDK:

1. **Baixe** a nova versão do site da Intelbras
2. **Substitua** as DLLs em `VMS_AlarmesJahu.App/libs/`
3. **Recompile** o projeto
4. **Teste** todas as funcionalidades

### Antes de Atualizar:

- ⚠️ Faça backup das DLLs atuais
- ⚠️ Verifique notas de versão (breaking changes)
- ⚠️ Teste em ambiente de desenvolvimento primeiro

---

## 🐛 Troubleshooting

### Erro: "DLL não encontrada"

**Causa:** DLL não foi copiada para a pasta de saída

**Solução:**
1. Verifique se a DLL existe em `VMS_AlarmesJahu.App/libs/`
2. Clean & Rebuild:
   ```
   Build → Clean Solution
   Build → Rebuild Solution
   ```
3. Verifique o arquivo `.csproj`

### Erro: "BadImageFormatException"

**Causa:** Mistura de arquiteturas (x86 vs x64)

**Solução:**
1. Verifique `PlatformTarget` no `.csproj`: deve ser `x64`
2. Certifique-se que **todas** as DLLs são x64
3. Recompile

### Erro: "Método não implementado" ou "EntryPointNotFoundException"

**Causa:** Versão incompatível do SDK

**Solução:**
1. Verifique a versão das DLLs (clique direito → Propriedades → Detalhes)
2. Baixe a versão compatível do SDK
3. Substitua todas as DLLs de uma vez

---

## 📝 Lista de Verificação

Antes de distribuir o executável:

- [ ] Todas as 15 DLLs estão na pasta de saída
- [ ] Testou em máquina limpa (sem SDK instalado)
- [ ] Conexão P2P funciona
- [ ] Vídeo ao vivo funciona
- [ ] Snapshot funciona
- [ ] Logs não mostram erros de DLL

---

## 📞 Suporte

### SDK Intelbras
- **Telefone**: 0800 570 0810
- **Site**: https://www.intelbras.com/pt-br
- **Downloads**: Área de suporte → SDKs

### Compatibilidade
- Verifique lista de dispositivos compatíveis no site
- Consulte manual do desenvolvedor Intelbras

---

## 📖 Documentação Adicional

- **Manual do Desenvolvedor**: Incluído no download do SDK
- **API Reference**: Consulte NetSDK.cs (no AutoRegister)
- **Exemplos**: AutoRegister é um exemplo funcional

---

## 🎉 Vantagens

### ✅ Todas as DLLs Incluídas

**Antes:**
- ❌ Tinha que baixar SDK separadamente
- ❌ Copiar DLLs manualmente
- ❌ Risco de versões incompatíveis

**Agora:**
- ✅ Tudo incluído no projeto
- ✅ Cópia automática no build
- ✅ Versões garantidamente compatíveis
- ✅ Pronto para distribuir

---

## 📊 Estatísticas

- **Total de DLLs**: 16 arquivos
- **Tamanho Total**: ~37 MB
- **Arquitetura**: x64 (64-bit)
- **Versão SDK**: 3.x
- **Compatibilidade**: Windows 7, 8, 10, 11

---

**Última atualização**: Janeiro 2026  
**Versão do SDK**: 3.x (64-bit)

---

**Nota**: Nunca mais se preocupe com "DLL não encontrada"! 🎉

Tudo está incluído e configurado automaticamente. Basta compilar e usar!
