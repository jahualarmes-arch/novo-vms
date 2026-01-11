using System;
using System.Collections.Concurrent;
using System.Threading;
using Serilog;

namespace VMS_AlarmesJahu.App.Services.P2P;

/// <summary>
/// Gerencia inicialização singleton do P2P (t2u_init) e túneis (t2u_add_port / t2u_del_port).
/// Reaproveita túneis por (SN, remotePort) com referência.
/// </summary>
public sealed class P2PCloudManager : IDisposable
{
    public static P2PCloudManager Instance { get; } = new();

    private readonly object _initLock = new();
    private bool _initialized;

    // (sn|remotePort) -> tunnel info
    private readonly ConcurrentDictionary<string, TunnelInfo> _tunnels = new();

    // Parâmetros padrão (conforme documentação)
    public string ServerHost { get; set; } = "38.250.250.12";
    public int ServerPortUdp { get; set; } = 1250;
    public string ServerKey { get; set; } = "intelbras";

    private P2PCloudManager() { }

    public void EnsureInitialized()
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;

            try
            {
                // t2u_init retorna 0/1 em implementações diferentes; tratamos >=0 como OK.
                var rc = Libt2uNative.t2u_init(ServerHost, ServerPortUdp, ServerKey);
                if (rc < 0)
                {
                    Log.Error("P2P t2u_init falhou rc={Rc} (verifique libt2u.dll no diretório do app)", rc);
                    _initialized = false;
                    return;
                }

                _initialized = true;
                Log.Information("✅ P2P inicializado: {Host}:{Port} key={Key}", ServerHost, ServerPortUdp, ServerKey);
            }
            catch (DllNotFoundException)
            {
                Log.Error("❌ libt2u.dll não encontrada. Coloque a DLL junto do executável (bin\\...\\net8.0-windows).");
                _initialized = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Erro ao inicializar P2P");
                _initialized = false;
            }
        }
    }

    public bool IsOnline(string serialNumber)
    {
        EnsureInitialized();
        if (!_initialized) return false;

        try
        {
            var rc = Libt2uNative.t2u_query(serialNumber);
            return rc == 1;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro t2u_query({SN})", serialNumber);
            return false;
        }
    }

    /// <summary>
    /// Abre (ou reaproveita) túnel para SN:remotePort e retorna a porta local.
    /// </summary>
    public int AcquireTunnel(string serialNumber, int remotePort)
    {
        EnsureInitialized();
        if (!_initialized) return -100;

        var key = $"{serialNumber}|{remotePort}";
        var info = _tunnels.GetOrAdd(key, _ => new TunnelInfo(serialNumber, remotePort));

        lock (info.Lock)
        {
            if (info.LocalPort > 0)
            {
                info.RefCount++;
                return info.LocalPort;
            }

            try
            {
                // local_port=0 => auto
                var localPort = Libt2uNative.t2u_add_port(serialNumber, remotePort, 0);
                if (localPort <= 0)
                {
                    Log.Warning("P2P t2u_add_port falhou SN={SN} remote={Remote} rc={Rc}", serialNumber, remotePort, localPort);
                    _tunnels.TryRemove(key, out _);
                    return localPort; // erro
                }

                info.LocalPort = localPort;
                info.RefCount = 1;

                Log.Information("✅ Túnel P2P aberto: SN={SN} remote={Remote} => 127.0.0.1:{Local}", serialNumber, remotePort, localPort);
                return localPort;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao abrir túnel P2P SN={SN} remote={Remote}", serialNumber, remotePort);
                _tunnels.TryRemove(key, out _);
                return -200;
            }
        }
    }

    public void ReleaseTunnel(string serialNumber, int remotePort, int localPort)
    {
        var key = $"{serialNumber}|{remotePort}";
        if (!_tunnels.TryGetValue(key, out var info)) return;

        lock (info.Lock)
        {
            if (info.LocalPort != localPort) return;

            info.RefCount--;
            if (info.RefCount > 0) return;

            try
            {
                var rc = Libt2uNative.t2u_del_port(localPort);
                Log.Information("Túnel P2P fechado: 127.0.0.1:{Local} rc={Rc}", localPort, rc);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao fechar túnel P2P local={Local}", localPort);
            }
            finally
            {
                _tunnels.TryRemove(key, out _);
            }
        }
    }

    public void Dispose()
    {
        // Fecha todos os túneis conhecidos
        foreach (var kv in _tunnels)
        {
            var info = kv.Value;
            lock (info.Lock)
            {
                if (info.LocalPort > 0)
                {
                    try { Libt2uNative.t2u_del_port(info.LocalPort); } catch { /* ignore */ }
                }
            }
        }
        _tunnels.Clear();

        if (_initialized)
        {
            try { Libt2uNative.t2u_exit(); } catch { /* ignore */ }
            _initialized = false;
        }
    }

    private sealed class TunnelInfo
    {
        public string SerialNumber { get; }
        public int RemotePort { get; }
        public int LocalPort { get; set; }
        public int RefCount { get; set; }
        public object Lock { get; } = new();

        public TunnelInfo(string sn, int remotePort)
        {
            SerialNumber = sn;
            RemotePort = remotePort;
        }
    }
}
