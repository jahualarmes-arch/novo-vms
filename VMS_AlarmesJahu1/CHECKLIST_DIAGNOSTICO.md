# ✅ CHECKLIST RÁPIDO - Diagnóstico P2P Cloud

Use este checklist para identificar rapidamente o problema. Marque cada item conforme verifica.

---

## 🔌 1. VERIFICAÇÕES BÁSICAS

### DVR
- [ ] DVR está **ligado** (luzes acesas)
- [ ] Cabo de rede está **conectado** no DVR
- [ ] LED de rede está **piscando** (indica atividade)
- [ ] DVR está **acessível localmente** (via monitor ou IP)

### Internet
- [ ] Roteador/modem está **funcionando**
- [ ] Outros dispositivos **têm internet** no local
- [ ] **Ping** no gateway funciona
- [ ] DVR consegue acessar sites externos (teste no menu do DVR)

---

## ⚙️ 2. CONFIGURAÇÃO DO DVR

### Cloud P2P
- [ ] Menu → Rede → **Cloud/P2P** está **HABILITADO** ✅
- [ ] Status mostra "**Online**" ou "**Conectado**"
- [ ] Número de série está **visível** e **correto**
- [ ] DVR está **registrado** no Cloud Intelbras

### Credenciais
- [ ] Usuário: `admin` (padrão) ou outro usuário criado
- [ ] Senha: testada e **funcionando localmente**
- [ ] Não há bloqueio de conta (muitas tentativas erradas)

### Rede
- [ ] Porta do dispositivo: **37777** (padrão) ou outra configurada
- [ ] DHCP habilitado **OU** IP fixo configurado corretamente
- [ ] DNS configurado (8.8.8.8 ou do provedor)
- [ ] Gateway configurado corretamente

---

## 💻 3. VERIFICAÇÕES NO COMPUTADOR/SOFTWARE

### Arquivo DLL
- [ ] `dhnetsdk.dll` está na **pasta do executável**
- [ ] DLL é da **versão correta** (x86/x64)
- [ ] DLL **não está corrompida** (redownload se necessário)

### Permissões
- [ ] Programa tem permissão de **executar**
- [ ] Firewall do Windows **não está bloqueando**
- [ ] Antivírus **não está bloqueando** a DLL

### Dados de Conexão
- [ ] Número de série: **SEM ESPAÇOS** antes/depois
- [ ] Número de série: em **LETRAS MAIÚSCULAS**
- [ ] Número de série: **exatamente igual** ao do DVR
- [ ] Porta: **37777** ou a configurada no DVR
- [ ] Usuário: **admin** ou outro criado
- [ ] Senha: **correta** e sem espaços extras

---

## 🧪 4. TESTES

### Teste Local (mesma rede)
- [ ] Consegue conectar via **IP local** (192.168.x.x)?
- [ ] Vídeo aparece ao conectar localmente?
- [ ] Credenciais funcionam na conexão local?

Se SIM para todos acima → Problema é no P2P Cloud  
Se NÃO → Problema é nas credenciais ou configuração básica

### Teste P2P
- [ ] Executou o `TesteP2P.exe` para diagnóstico?
- [ ] Verificou os **logs detalhados**?
- [ ] Código de erro é **específico** (não genérico)?

---

## 📊 5. ANÁLISE DO ERRO

### Código de Erro 3 (Erro de rede)
- [ ] Computador tem internet?
- [ ] DVR tem internet?
- [ ] Firewall está bloqueando?

### Código de Erro 6 (Senha incorreta)
- [ ] Senha está **exatamente** como no DVR?
- [ ] Testou fazer login **localmente** com a mesma senha?
- [ ] Não há **espaços** extras na senha?

### Código de Erro 7 (Timeout)
- [ ] DVR está muito **lento** para responder?
- [ ] Internet do DVR está muito **lenta**?
- [ ] Muitos dispositivos conectados no DVR?

### Código de Erro 11 (Dispositivo offline)
- [ ] DVR está realmente **online**?
- [ ] Cloud P2P está **habilitado**?
- [ ] Status do Cloud no DVR mostra "**Online**"?
- [ ] DVR foi **reiniciado** recentemente?

### Código de Erro 33 (Número de série inválido)
- [ ] Número de série está **correto**?
- [ ] DVR está **registrado** no Cloud?
- [ ] Número de série foi **digitado manualmente** (não copiado)?

---

## 🔄 6. SOLUÇÕES TENTADAS

### Básicas
- [ ] **Reiniciei** o DVR
- [ ] **Reiniciei** o roteador/modem
- [ ] **Reiniciei** o computador
- [ ] Aguardei **5 minutos** após reiniciar tudo

### Avançadas
- [ ] **Desabilitei** temporariamente o firewall
- [ ] **Desabilitei** temporariamente o antivírus
- [ ] Testei em **outra rede** (4G/outro local)
- [ ] **Re-registrei** o DVR no Cloud P2P
- [ ] **Atualizei** o firmware do DVR
- [ ] **Reinstalei** o software/aplicativo

### Extremas (último recurso)
- [ ] **Reset de fábrica** do DVR (⚠️ apaga tudo!)
- [ ] **Reconfigurei** tudo do zero
- [ ] Testei com **outro DVR** (se disponível)

---

## 📞 7. PRECISO DE AJUDA?

Se marcou TODOS os itens acima e ainda não funciona:

1. **Anote** o código de erro exato
2. **Copie** os logs completos
3. **Tire print** da tela de erro
4. **Tire foto** da etiqueta do DVR (com número de série)
5. **Contacte** o suporte Intelbras: 0800 570 0810

---

## 💡 DICA RÁPIDA

**90% dos problemas P2P são um destes:**

1. 🔴 DVR não tem internet → Verifique cabo e roteador
2. 🔴 Cloud P2P desabilitado → Habilite no menu do DVR
3. 🔴 Número de série errado → Compare com a etiqueta física
4. 🔴 Senha incorreta → Teste localmente primeiro
5. 🔴 DLL ausente → Copie dhnetsdk.dll para a pasta

**Comece por estes 5 itens antes de qualquer outra coisa!**

---

## ✅ TUDO FUNCIONANDO?

Se conseguiu conectar:
- [ ] Conexão P2P estabelecida ✅
- [ ] Vídeo aparecendo ✅
- [ ] Sem travamentos ✅
- [ ] Reconexão automática funciona ✅

**Parabéns! 🎉**

---

**Versão**: 1.0  
**Última atualização**: Janeiro 2026  

Mantenha este checklist para referência futura!
