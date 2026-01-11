# P2P Tunnel Manager

## Overview

O **P2PTunnelManager** é um singleton que gerencia túneis P2P para conexões Intelbras Cloud. Ele mapeia portas remotas em dispositivos P2P para portas locais (6000-7000), permitindo que o SDK faça login através de `127.0.0.1:localPort`.

## Architecture

### Initialization
- **Thread-safe**: Usa `ReaderWriterLockSlim` para garantir que `t2u_init()` é chamado apenas uma vez
- **Lazy**: Implementado como `Lazy<T>` para inicialização singleton
- **Non-blocking**: Se `libt2u.dll` não existir, loga warning mas app continua rodando

### Port Allocation
- Pool de portas: **6000-7000** (1001 portas disponíveis)
- Incremento circular: Se atingir MaxPort, volta para 6000
- Colisão evitada: Cada `serial` tem uma única porta local

### Tunnel Lifecycle
1. **OpenTunnel(serial, remotePort)**: 
   - Se já aberto, retorna porta local existente (idempotent)
   - Senão, aloca nova porta local
   - Chama `t2u_add_port(serial, remotePort, localPort)`
   - Armazena no `ConcurrentDictionary`

2. **CloseTunnel(serial)**:
   - Remove do dicionário
   - Chama `t2u_del_port(serial, localPort)`
   - Libera porta para reutilização

3. **CloseAll()**:
   - Chamado em `App.OnExit()`
   - Garante limpeza de recursos

## Integration with ConnectionManager

```csharp
// ConnectionManager.Connect() - P2P Cloud
if (device.ConnectionType == ConnectionType.P2PCloud)
{
    // 1. Abrir túnel
    int localPort = P2PTunnelManager.Instance.OpenTunnel(device.SerialNumber, device.Port);
    if (localPort == -1)
    {
        // Falha: dispositivo offline ou indisponível
        return IntPtr.Zero;
    }
    
    // 2. Login via 127.0.0.1:localPort
    login = IntelbrasSdk.Login("127.0.0.1", localPort, device.User, password, out channelCount);
    
    // 3. Se falhar, fechar túnel
    if (login == IntPtr.Zero)
    {
        P2PTunnelManager.Instance.CloseTunnel(device.SerialNumber);
        return IntPtr.Zero;
    }
    
    // 4. Armazenar P2PLocalPort na conexão
    conn.P2PLocalPort = localPort;
}

// ConnectionManager.Disconnect() - P2P Cloud
if (device.ConnectionType == ConnectionType.P2PCloud)
{
    P2PTunnelManager.Instance.CloseTunnel(device.SerialNumber);
}
```

## Error Handling

| Erro | Causa | Ação |
|------|-------|------|
| `t2u_init()` falha | `libt2u.dll` corrompida ou incompatível | Log error, app roda sem P2P |
| `t2u_add_port()` falha | Dispositivo offline, sem internet, ou porta indisponível | Log error, retorna -1 |
| `t2u_query()` falha | Túnel perdeu conexão | Considerar reconexão automática |
| `DllNotFoundException` | `libt2u.dll` não encontrado | Log warning, app roda sem P2P |

## Build Configuration

No `.csproj`:
```xml
<!-- SDK Intelbras - Todas as DLLs necessárias -->
<ItemGroup>
  <None Include="libs\*.dll" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>

<!-- Aviso se libt2u.dll não existe -->
<Target Name="CheckP2PDependencies" BeforeTargets="Build">
  <Warning Text="⚠️  libt2u.dll não encontrado. P2P Cloud não será funcional." 
    Condition="!Exists('libs\libt2u.dll')" />
</Target>
```

## Testing P2P Tunnels

### Manual Test
```csharp
// Inicializar
var manager = P2PTunnelManager.Instance;
if (!manager.EnsureInitialized())
{
    Console.WriteLine("Falha ao inicializar P2P");
    return;
}

// Abrir túnel
int port = manager.OpenTunnel("DVR123456789", 37777);
if (port == -1)
{
    Console.WriteLine("Falha ao abrir túnel");
    return;
}

Console.WriteLine($"Túnel aberto: 127.0.0.1:{port} → DVR em porta remota 37777");

// Tentar login
var login = IntelbrasSdk.Login("127.0.0.1", port, "admin", "123456", out int channels);
if (login != IntPtr.Zero)
{
    Console.WriteLine($"Login bem-sucedido! {channels} canais");
}

// Fechar túnel
manager.CloseTunnel("DVR123456789");
```

### Unit Tests (Sugerido)
- Mock `libt2u.dll` com stubs que retornam sucesso
- Testar `OpenTunnel()` idempotence
- Testar race conditions com múltiplas threads
- Testar `CloseAll()` cleanup

## Logs

Exemplos de logs importantes:

```
[INF] Inicializando P2P Tunnel Manager
[INF] libt2u.dll encontrado em C:\...\libs\libt2u.dll
[INF] P2P Tunnel Manager inicializado com sucesso
[INF] Abrindo túnel P2P para DVR123456789: local 6001 → remoto 37777
[INF] Túnel P2P aberto com sucesso: DVR123456789 → porta local 6001
[ERR] t2u_add_port falhou: serial=DVR123456789, remote=37777, local=6001, code=1001
[WAR] Túnel não encontrado para DVR123456789
[INF] Fechando todos os túneis P2P (2 túneis)
[INF] Todos os túneis P2P foram fechados
```

## Performance Considerations

- **Port pool**: 1001 portas (6000-7000) — suficiente para ~1000 conexões P2P simultâneas
- **Memory**: Cada `TunnelInfo` = ~32 bytes + string serialNumber (~20-50 bytes) → ~100 bytes por túnel
- **ConcurrentDictionary**: Lock-free reads, thread-safe updates
- **ReaderWriterLockSlim**: Múltiplos readers na inicialização, 1 writer

## Future Enhancements

- [ ] Health check: `t2u_query()` periódico para validar status de túneis
- [ ] Automatic reconnect: Se túnel cair, reopenar automaticamente
- [ ] Pool expansion: Permitir range customizável de portas
- [ ] Metrics: Contar quantos túneis estão abertos, tempo de abertura médio
- [ ] Diagnostics: Expor método `GetDiagnostics()` para troubleshooting
