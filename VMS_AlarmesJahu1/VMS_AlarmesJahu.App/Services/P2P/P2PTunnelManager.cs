using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Serilog;

namespace VMS_AlarmesJahu.App.Services.P2P;

/// <summary>
/// Gerenciador singleton de túneis P2P para conexões Intelbras.
/// Gerencia abertura/fechamento de túneis locais usando libt2u.dll.
/// 
/// Thread-safe com pool de portas locais (6000-7000).
/// </summary>
public sealed class P2PTunnelManager : IDisposable
{
    private static readonly Lazy<P2PTunnelManager> _instance = 
        new(() => new P2PTunnelManager());
    
    public static P2PTunnelManager Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, TunnelInfo> _tunnels = new();
    private readonly ReaderWriterLockSlim _initLock = new();
    private bool _initialized;
    private bool _disposed;
    private int _nextLocalPort = 6000;
    private const int MaxLocalPort = 7000;

    private P2PTunnelManager() { }

    /// <summary>
    /// Inicializa o subsistema P2P (uma única vez).
    /// Carrega libt2u.dll e chama t2u_init.
    /// </summary>
    public bool EnsureInitialized()
    {
        _initLock.EnterUpgradeableReadLock();
        try
        {
            if (_initialized)
                return true;

            _initLock.EnterWriteLock();
            try
            {
                if (_initialized)
                    return true;

                Log.Information("Inicializando P2P Tunnel Manager");

                // Verificar se libt2u.dll existe
                var dllPath = Path.Combine(AppContext.BaseDirectory, "libt2u.dll");
                if (!File.Exists(dllPath))
                {
                    Log.Warning("libt2u.dll não encontrado em {Path}. P2P Tunnel não será funcional.", dllPath);
                    Log.Information("Se precisar de P2P Cloud, certifique-se de que libt2u.dll está na pasta de saída.");
                    return false;
                }

                Log.Information("libt2u.dll encontrado em {Path}", dllPath);

                // Chamar t2u_init
                try
                {
                    var result = t2u_init();
                    if (result != 0)
                    {
                        Log.Error("t2u_init falhou com código {ErrorCode}", result);
                        return false;
                    }

                    _initialized = true;
                    Log.Information("P2P Tunnel Manager inicializado com sucesso");
                    return true;
                }
                catch (DllNotFoundException ex)
                {
                    Log.Error(ex, "Falha ao carregar libt2u.dll. P2P não será funcional.");
                    return false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro ao inicializar P2P");
                    return false;
                }
            }
            finally
            {
                _initLock.ExitWriteLock();
            }
        }
        finally
        {
            _initLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Abre um túnel local para um dispositivo P2P Cloud.
    /// 
    /// Mapeia serialNumber → porta local (entre 6000-7000).
    /// Se já existe túnel aberto para este serial, retorna a porta local existente.
    /// 
    /// Retorna: porta local (>= 6000), ou -1 se falha.
    /// </summary>
    public int OpenTunnel(string serialNumber, int remotePort = 37777)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            Log.Error("serialNumber inválido para OpenTunnel");
            return -1;
        }

        if (!_initialized)
        {
            Log.Error("P2P Tunnel Manager não inicializado. Chame EnsureInitialized() primeiro.");
            return -1;
        }

        // Se já existe túnel aberto, retornar a porta local
        if (_tunnels.TryGetValue(serialNumber, out var existingTunnel))
        {
            Log.Information("Túnel já aberto para {Serial} → porta local {Port}", serialNumber, existingTunnel.LocalPort);
            return existingTunnel.LocalPort;
        }

        // Alocar nova porta local
        int localPort = Interlocked.Increment(ref _nextLocalPort);
        if (localPort > MaxLocalPort)
        {
            Interlocked.Exchange(ref _nextLocalPort, 6000);
            localPort = 6001;
        }

        Log.Information("Abrindo túnel P2P para {Serial}: local {LocalPort} → remoto {RemotePort}", 
            serialNumber, localPort, remotePort);

        try
        {
            var result = t2u_add_port(serialNumber, remotePort, localPort);
            if (result != 0)
            {
                Log.Error("t2u_add_port falhou: serial={Serial}, remote={Remote}, local={Local}, code={Code}", 
                    serialNumber, remotePort, localPort, result);
                return -1;
            }

            var tunnel = new TunnelInfo 
            { 
                SerialNumber = serialNumber, 
                RemotePort = remotePort, 
                LocalPort = localPort,
                OpenedAt = DateTime.Now
            };

            if (!_tunnels.TryAdd(serialNumber, tunnel))
            {
                // Race condition: outro thread já adicionou, usar aquele
                Log.Warning("Race condition ao adicionar túnel para {Serial}, usando existente", serialNumber);
                t2u_del_port(serialNumber, localPort); // Limpar o que acabamos de criar
                
                if (_tunnels.TryGetValue(serialNumber, out var raceTunnel))
                    return raceTunnel.LocalPort;
                    
                return -1;
            }

            Log.Information("Túnel P2P aberto com sucesso: {Serial} → porta local {Port}", serialNumber, localPort);
            return localPort;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao abrir túnel P2P para {Serial}", serialNumber);
            return -1;
        }
    }

    /// <summary>
    /// Fecha o túnel para um dispositivo P2P Cloud.
    /// </summary>
    public bool CloseTunnel(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return false;

        if (!_tunnels.TryRemove(serialNumber, out var tunnel))
        {
            Log.Warning("Túnel não encontrado para {Serial}", serialNumber);
            return false;
        }

        Log.Information("Fechando túnel P2P para {Serial}", serialNumber);

        try
        {
            var result = t2u_del_port(serialNumber, tunnel.LocalPort);
            if (result != 0)
            {
                Log.Error("t2u_del_port falhou: serial={Serial}, port={Port}, code={Code}", 
                    serialNumber, tunnel.LocalPort, result);
                return false;
            }

            Log.Information("Túnel fechado com sucesso: {Serial}", serialNumber);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao fechar túnel P2P para {Serial}", serialNumber);
            return false;
        }
    }

    /// <summary>
    /// Fecha todos os túneis abertos.
    /// </summary>
    public void CloseAll()
    {
        Log.Information("Fechando todos os túneis P2P ({Count} túneis)", _tunnels.Count);

        foreach (var serial in _tunnels.Keys.ToList())
        {
            CloseTunnel(serial);
        }

        _tunnels.Clear();
        Log.Information("Todos os túneis P2P foram fechados");
    }

    /// <summary>
    /// Consulta informações de um túnel.
    /// </summary>
    public TunnelInfo? GetTunnel(string serialNumber)
    {
        _tunnels.TryGetValue(serialNumber, out var tunnel);
        return tunnel;
    }

    /// <summary>
    /// Retorna lista de todos os túneis abertos.
    /// </summary>
    public IEnumerable<TunnelInfo> GetOpenTunnels() => _tunnels.Values.ToList();

    // ===== P/Invoke para libt2u.dll =====

    /// <summary>
    /// Inicializa o subsistema P2P.
    /// Retorna: 0 = sucesso, >0 = erro.
    /// </summary>
    [DllImport("libt2u.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int t2u_init();

    /// <summary>
    /// Abre um túnel (mapeia remotePort no dispositivo → localPort no host).
    /// Retorna: 0 = sucesso, >0 = erro.
    /// </summary>
    [DllImport("libt2u.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int t2u_add_port(string serial, int remotePort, int localPort);

    /// <summary>
    /// Fecha um túnel.
    /// Retorna: 0 = sucesso, >0 = erro.
    /// </summary>
    [DllImport("libt2u.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int t2u_del_port(string serial, int localPort);

    /// <summary>
    /// Consulta status de um túnel.
    /// Retorna: 0 = ativo, >0 = erro/inativo.
    /// </summary>
    [DllImport("libt2u.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int t2u_query(string serial, int localPort);

    // ===== IDisposable =====

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            CloseAll();
        }
        finally
        {
            _initLock?.Dispose();
            _disposed = true;
        }
    }

    // ===== Model =====

    public sealed class TunnelInfo
    {
        public string SerialNumber { get; init; } = "";
        public int RemotePort { get; init; }
        public int LocalPort { get; init; }
        public DateTime OpenedAt { get; init; }
    }
}
