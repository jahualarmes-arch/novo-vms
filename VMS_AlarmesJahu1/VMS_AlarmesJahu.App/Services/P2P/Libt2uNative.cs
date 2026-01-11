using System;
using System.Runtime.InteropServices;

namespace VMS_AlarmesJahu.App.Services.P2P;

/// <summary>
/// P/Invoke mínimo da libt2u.dll (P2P Intelbras Cloud).
/// OBS: esta DLL é proprietária (vem do SIM Next/cliente oficial). Coloque libt2u.dll junto do executável.
/// </summary>
internal static class Libt2uNative
{
    private const string DllName = "libt2u.dll";

    // int t2u_init(const char* svraddr, int svrport, const char* svrkey);
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern int t2u_init(string svraddr, int svrport, string svrkey);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int t2u_exit();

    // int t2u_query(const char* sn);
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern int t2u_query(string sn);

    // int t2u_add_port(const char* sn, int remote_port, int local_port);
    // retorna a porta local alocada (>0) ou negativo para erro.
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal static extern int t2u_add_port(string sn, int remote_port, int local_port);

    // int t2u_del_port(int local_port);
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int t2u_del_port(int local_port);
}
