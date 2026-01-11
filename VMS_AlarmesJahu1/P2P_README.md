# P2P Intelbras (Cloud) — Integração no VMS Alarmes Jahu

Este projeto já está com a **integração P2P por túnel (libt2u.dll)** preparada.

## Como funciona (resumo)
1. O VMS inicializa o P2P: servidor **38.250.250.12:1250/UDP** e chave **intelbras**
2. Abre um túnel local para o DVR via `t2u_add_port(SN, portaRemota, 0)`
3. Recebe uma porta local (ex.: 17777)
4. Conecta o SDK normalmente em **127.0.0.1:17777**
5. No logout, fecha o túnel automaticamente

## O que você precisa copiar
Por ser DLL proprietária, **você precisa colocar `libt2u.dll` junto do executável** do VMS:

`VMS_AlarmesJahu.App\bin\Debug\net8.0-windows\libt2u.dll`
ou
`VMS_AlarmesJahu.App\bin\Release\net8.0-windows\libt2u.dll`

> Se a DLL não estiver presente, o VMS continua compilando, mas o P2P Tunnel não funciona (vai logar erro informando).

## Como testar
1) Abra o VMS → Dispositivos  
2) Cadastre um dispositivo com:
- Tipo: **P2PCloud**
- SerialNumber: **SN do DVR**
- Porta: **37777** (padrão SDK)
- Usuário/Senha do DVR

3) Clique **Testar conexão** / Conectar.

## Dicas
- Firewall precisa liberar UDP (P2P).
- Se o DVR não estiver online no P2P, `t2u_query` vai retornar -1/0 e o túnel não abre.
