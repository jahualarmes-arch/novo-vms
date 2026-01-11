using System;
using System.Runtime.InteropServices;

namespace VMS_AlarmesJahu.App.Views.Controls;

/// <summary>
/// Janela Win32 oculta para manter o stream do SDK ativo
/// O SDK precisa de um HWND para rodar o RealPlay, mas queremos mostrar
/// snapshots num Image WPF para poder desenhar overlays por cima
/// </summary>
public class HiddenNativeWindow : IDisposable
{
    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    private const int WS_DISABLED = 0x08000000;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName,
        int dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private IntPtr _hwnd;
    private bool _disposed;

    public IntPtr Handle => _hwnd;

    public HiddenNativeWindow()
    {
        var hInstance = GetModuleHandle(null);
        
        // Cria uma janela oculta (tamanho 1x1 fora da tela)
        _hwnd = CreateWindowEx(
            0,
            "STATIC",
            "HiddenVideoWindow",
            WS_CHILD | WS_DISABLED,
            -10, -10, 1, 1,
            GetDesktopWindow(),
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero
        );

        if (_hwnd == IntPtr.Zero)
        {
            // Fallback: criar como popup invisível
            _hwnd = CreateWindowEx(
                0,
                "STATIC",
                "HiddenVideoWindow",
                0, // Sem estilo visível
                0, 0, 1, 1,
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero
            );
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }

    ~HiddenNativeWindow()
    {
        Dispose();
    }
}
