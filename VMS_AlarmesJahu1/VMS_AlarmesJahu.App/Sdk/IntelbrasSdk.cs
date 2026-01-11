using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Serilog;
using VMS_AlarmesJahu.App.Services.P2P;

namespace VMS_AlarmesJahu.App.Sdk;

/// <summary>
/// Wrapper leve do NetSDK (dhnetsdk.dll) com suporte a:
/// - Login IP (TCP)
/// - Login Cloud P2P (EM_LOGIN_SPAC_CAP_TYPE.P2P = 19)
/// 
/// Importante: o login por P2P do jeito "celular" depende do SDK suportar P2P.
/// Caso a DLL seja antiga, o P2P pode não funcionar.
/// </summary>
public static class IntelbrasSdk
{
    // ===== P2P tunnel bindings (libt2u) =====
    private static readonly ConcurrentDictionary<IntPtr, P2PTunnelBinding> _p2pBindings = new();

    private sealed class P2PTunnelBinding
    {
        public string SerialNumber { get; init; } = "";
        public int RemotePort { get; init; }
        public int LocalPort { get; init; }
    }


    private static bool _initialized;

    // ===== ENUMS =====
    public enum EM_LOGIN_SPAC_CAP_TYPE
    {
        TCP = 0,
        ANY = 1,
        MULTICAST = 3,
        UDP = 4,
        SSL = 7,
        CLOUD = 16,
        P2P = 19
    }

    public enum EM_RealPlayType
    {
        /// <summary>
        /// Real-time preview
        /// </summary>
        Realplay = 0,

        /// <summary>
        /// Multiple-channel preview
        /// </summary>
        Multiplay = 1,

        /// <summary>
        /// Real-time monitor - main stream (equivalente ao Realplay)
        /// </summary>
        Realplay_0 = 2,

        /// <summary>
        /// Real-time monitor - extra stream 1 (substream)
        /// </summary>
        Realplay_1 = 3,

        /// <summary>
        /// Real-time monitor - extra stream 2
        /// </summary>
        Realplay_2 = 4
    }

    // ===== STRUCTS =====
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NET_DEVICEINFO_Ex
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string sSerialNumber;

        public int nAlarmInPortNum;
        public int nAlarmOutPortNum;
        public int nDiskNum;
        public int nDVRType;
        public int nChanNum;

        public byte byLimitLoginTime;
        public byte byLeftLogTimes;
        public byte byLockLeftTime;
        public byte byReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NetworkParam
    {
        public int nWaittime;
        public int nConnectTime;
        public int nConnectTryNum;
        public int nSubConnectSpaceTime;
        public int nGetDevInfoTime;
        public int nConnectBufSize;
        public int nGetConnInfoTime;
        public int nSearchRecordTime;
        public int nsubDisconnetTime;
        public int nGetLocalIpTime;
        public int nNetworkParamSize;
    }

    // ===== CALLBACKS (assinaturas oficiais) =====
    public delegate void fDisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser);
    public delegate void fHaveReConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser);

    // ===== DLL IMPORTS =====
    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern bool CLIENT_InitEx(fDisConnectCallBack cbDisConnect, IntPtr dwUser, IntPtr lpInitParam);

    // Algumas DLLs antigas expõem CLIENT_Init (sem InitParam). Mantemos como fallback.
    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern bool CLIENT_Init(IntPtr cbDisConnect, IntPtr dwUser);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern void CLIENT_Cleanup();

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern uint CLIENT_GetLastError();

    [DllImport("dhnetsdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr CLIENT_LoginEx2(
        string pchDVRIP,
        ushort wDVRPort,
        string pchUserName,
        string pchPassword,
        EM_LOGIN_SPAC_CAP_TYPE emSpecCap,
        IntPtr pCapParam,
        ref NET_DEVICEINFO_Ex lpDeviceInfo,
        ref int error);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool CLIENT_Logout(IntPtr lLoginID);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern void CLIENT_SetAutoReconnect(fHaveReConnectCallBack cbAutoConnect, IntPtr dwUser);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern void CLIENT_SetNetworkParam(IntPtr pNetParam);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool CLIENT_SetConnectTime(int nWaitTime, int nTryTimes);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr CLIENT_RealPlayEx(IntPtr lLoginID, int nChannelID, IntPtr hWnd, EM_RealPlayType rType);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool CLIENT_StopRealPlayEx(IntPtr lRealHandle);

    [DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool CLIENT_GetPicJPEG(IntPtr lPlayHandle, IntPtr pJpegBuf, uint dwBufSize, out uint pJpegSize, int quality);

    public static void Initialize()
    {
        if (_initialized) return;

        Log.Information("Inicializando SDK Intelbras (NetSDK)...");

        // Verifica DLL
        try { _ = CLIENT_GetLastError(); }
        catch (DllNotFoundException)
        {
            Log.Fatal("ERRO CRÍTICO: dhnetsdk.dll não encontrada!");
            throw new Exception("SDK Intelbras não encontrado. Verifique se dhnetsdk.dll está presente.");
        }

        bool ok;
        try
        {
            ok = CLIENT_InitEx(null, IntPtr.Zero, IntPtr.Zero);
        }
        catch (EntryPointNotFoundException)
        {
            // DLL antiga
            ok = CLIENT_Init(IntPtr.Zero, IntPtr.Zero);
        }

        if (!ok)
        {
            var err = CLIENT_GetLastError();
            throw new Exception($"Falha ao inicializar SDK Intelbras. LastError={err} ({GetErrorDescription(err)})");
        }

        // timeouts
        CLIENT_SetConnectTime(15000, 3);

        // parâmetros de rede (ponteiro)
        var netParam = new NetworkParam
        {
            nWaittime = 15000,
            nConnectTime = 15000,
            nConnectTryNum = 3,
            nSubConnectSpaceTime = 2000,
            nGetDevInfoTime = 10000,
            nConnectBufSize = 256 * 1024,
            nGetConnInfoTime = 3000,
            nSearchRecordTime = 10000,
            nsubDisconnetTime = 60000,
            nGetLocalIpTime = 1000,
            nNetworkParamSize = Marshal.SizeOf<NetworkParam>()
        };

        var pNet = Marshal.AllocHGlobal(Marshal.SizeOf<NetworkParam>());
        try
        {
            Marshal.StructureToPtr(netParam, pNet, false);
            CLIENT_SetNetworkParam(pNet);
        }
        finally
        {
            Marshal.FreeHGlobal(pNet);
        }

        // reconexão automática global (opcional)
        CLIENT_SetAutoReconnect(null, IntPtr.Zero);

        _initialized = true;
        Log.Information("SDK Intelbras inicializado com sucesso.");
    }

    public static IntPtr Login(string host, int port, string user, string password, out int channelCount)
    {
        channelCount = 0;
        if (!_initialized) Initialize();

        var info = new NET_DEVICEINFO_Ex { sSerialNumber = string.Empty };
        int err = 0;

        var login = CLIENT_LoginEx2(host, (ushort)port, user, password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref info, ref err);
        if (login == IntPtr.Zero)
        {
            var last = CLIENT_GetLastError();
            Log.Warning("Falha Login IP: {Host}:{Port} user={User} err={Err} last={Last} ({Desc})", host, port, user, err, last, GetErrorDescription(last));
            return IntPtr.Zero;
        }

        channelCount = info.nChanNum;
        Log.Debug("Login OK: {Host}:{Port}, {Channels} canais", host, port, channelCount);
        return login;
    }

    /// <summary>
    /// Login via Cloud P2P usando EM_LOGIN_SPAC_CAP_TYPE.P2P (19).
    /// Se não funcionar, tenta fallback CLOUD (16) e também um modo alternativo (SN em pCapParam).
    /// </summary>
    public static IntPtr LoginP2P(string serialNumber, int port, string user, string password, out int channelCount)
    {
        channelCount = 0;
        if (!_initialized) Initialize();

        serialNumber = (serialNumber ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            Log.Error("Número de série vazio para login P2P.");

        // ===== Tentativa 4: TÚNEL P2P (libt2u.dll) => conectar como local 127.0.0.1:porta =====
        {
            try
            {
                // remotePort = porta do serviço no DVR (padrão SDK: 37777; RTSP: 554; etc.)
                var localPort = P2PCloudManager.Instance.AcquireTunnel(serialNumber, port);
                if (localPort > 0)
                {
                    Log.Information("Tentando login via túnel P2P local 127.0.0.1:{LocalPort}", localPort);

                    var login = Login("127.0.0.1", localPort, user, password, out channelCount);
                    if (login != IntPtr.Zero)
                    {
                        _p2pBindings[login] = new P2PTunnelBinding
                        {
                            SerialNumber = serialNumber,
                            RemotePort = port,
                            LocalPort = localPort
                        };

                        Log.Information("✅ Login via TÚNEL P2P OK. Canais={Channels}", channelCount);
                        return login;
                    }

                    // se falhou o login, fecha o túnel para não vazar recurso
                    P2PCloudManager.Instance.ReleaseTunnel(serialNumber, port, localPort);
                    var last = CLIENT_GetLastError();
                    Log.Warning("Login via túnel P2P falhou: last={Last} ({Desc})", last, GetErrorDescription(last));
                }
                else
                {
                    Log.Warning("Não foi possível abrir túnel P2P (rc={Rc}). Verifique libt2u.dll e se o DVR está online no P2P.", localPort);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro na tentativa de túnel P2P");
            }
        }


        return IntPtr.Zero;
        }

        Log.Information("Tentando login Cloud P2P: SN={SN} Porta={Port} User={User}", serialNumber, port, user);

        // ===== Tentativa 1: P2P com SN no campo host =====
        {
            var info = new NET_DEVICEINFO_Ex { sSerialNumber = string.Empty };
            int err = 0;
            var login = CLIENT_LoginEx2(serialNumber, (ushort)port, user, password, EM_LOGIN_SPAC_CAP_TYPE.P2P, IntPtr.Zero, ref info, ref err);
            if (login != IntPtr.Zero)
            {
                channelCount = info.nChanNum;
                Log.Information("✅ Login P2P OK (modo 1). Canais={Channels}", channelCount);
                return login;
            }

            var last = CLIENT_GetLastError();
            Log.Warning("P2P modo 1 falhou: err={Err} last={Last} ({Desc})", err, last, GetErrorDescription(last));
        }

        // ===== Tentativa 2: P2P com SN em pCapParam (alguns firmwares) =====
        IntPtr snPtr = IntPtr.Zero;
        try
        {
            snPtr = Marshal.StringToHGlobalAnsi(serialNumber);
            var info = new NET_DEVICEINFO_Ex { sSerialNumber = string.Empty };
            int err = 0;
            var login = CLIENT_LoginEx2(string.Empty, (ushort)port, user, password, EM_LOGIN_SPAC_CAP_TYPE.P2P, snPtr, ref info, ref err);
            if (login != IntPtr.Zero)
            {
                channelCount = info.nChanNum;
                Log.Information("✅ Login P2P OK (modo 2). Canais={Channels}", channelCount);
                return login;
            }

            var last = CLIENT_GetLastError();
            Log.Warning("P2P modo 2 falhou: err={Err} last={Last} ({Desc})", err, last, GetErrorDescription(last));
        }
        finally
        {
            if (snPtr != IntPtr.Zero) Marshal.FreeHGlobal(snPtr);
        }

        // ===== Tentativa 3: CLOUD (fallback) =====
        {
            var info = new NET_DEVICEINFO_Ex { sSerialNumber = string.Empty };
            int err = 0;
            var login = CLIENT_LoginEx2(serialNumber, (ushort)port, user, password, EM_LOGIN_SPAC_CAP_TYPE.CLOUD, IntPtr.Zero, ref info, ref err);
            if (login != IntPtr.Zero)
            {
                channelCount = info.nChanNum;
                Log.Information("✅ Login CLOUD OK (fallback). Canais={Channels}", channelCount);
                return login;
            }

            var last = CLIENT_GetLastError();
            Log.Error("❌ Falha Login P2P/CLOUD: last={Last} ({Desc})", last, GetErrorDescription(last));
        }

        return IntPtr.Zero;
    }

    // compat
    public static IntPtr LoginP2P(string serialNumber, string user, string password, out int channelCount)
        => LoginP2P(serialNumber, 37777, user, password, out channelCount);

    /// <summary>
    /// Inicia visualização em tempo real.
    /// streamType: 0 = principal (main), 1 = extra (substream)
    /// </summary>
    public static IntPtr RealPlay(IntPtr login, int channel, IntPtr hwnd, int streamType = 0)
    {
        if (login == IntPtr.Zero) return IntPtr.Zero;

        // IMPORTANTE:
        // No NetSDK, o tipo de stream é escolhido via EM_RealPlayType:
        // Realplay_0 = principal, Realplay_1 = extra.
        var rType = streamType switch
        {
            1 => EM_RealPlayType.Realplay_1,
            2 => EM_RealPlayType.Realplay_2,
            _ => EM_RealPlayType.Realplay_0
        };

        var handle = CLIENT_RealPlayEx(login, channel, hwnd, rType);
        if (handle == IntPtr.Zero)
        {
            var err = CLIENT_GetLastError();
            Log.Warning("Falha ao iniciar play CH{Channel}, erro={Error} ({ErrorDesc})", channel, err, GetErrorDescription(err));
        }
        return handle;
    }

    public static void StopRealPlay(IntPtr playHandle)
    {
        if (playHandle != IntPtr.Zero)
            CLIENT_StopRealPlayEx(playHandle);
    }

    public static byte[]? SnapshotJpegBytesFromPlay(IntPtr playHandle, int quality = 70)
    {
        if (playHandle == IntPtr.Zero) return null;

        const int maxSize = 2_000_000;
        var buffer = Marshal.AllocHGlobal(maxSize);

        try
        {
            if (!CLIENT_GetPicJPEG(playHandle, buffer, (uint)maxSize, out var size, quality))
                return null;

            if (size == 0 || size > maxSize) return null;

            var data = new byte[size];
            Marshal.Copy(buffer, data, 0, (int)size);
            return data;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao capturar snapshot");
            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public static string GetErrorDescription(uint errorCode)
    {
        return errorCode switch
        {
            0 => "Sucesso",
            5 => "Usuário inválido ou não existe",
            6 => "Senha incorreta",
            7 => "Timeout - Dispositivo não respondeu",
            10 => "Conexão recusada",
            11 => "Dispositivo offline / não acessível",
            33 => "Serial inválido / não registrado no Cloud",
            _ => $"Erro {errorCode}"
        };
    }

    public static bool IsSdkAvailable()
    {
        try
        {
            _ = CLIENT_GetLastError();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Logout seguro: se o login veio de túnel P2P (libt2u), fecha o túnel também.
    /// </summary>
    public static void Logout(IntPtr loginHandle)
    {
        try
        {
            if (loginHandle != IntPtr.Zero && _p2pBindings.TryRemove(loginHandle, out var bind))
            {
                P2PCloudManager.Instance.ReleaseTunnel(bind.SerialNumber, bind.RemotePort, bind.LocalPort);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao liberar túnel P2P no logout");
        }
        finally
        {
            try
            {
                if (loginHandle != IntPtr.Zero)
                    CLIENT_Logout(loginHandle);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro no CLIENT_Logout");
            }
        }
    }

}
