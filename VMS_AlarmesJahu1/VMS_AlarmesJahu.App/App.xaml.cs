using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.IA;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Services.P2P;
using VMS_AlarmesJahu.App.Sdk;

namespace VMS_AlarmesJahu.App;

public partial class App : Application
{
    private IServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configurar Serilog
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VMS_AlarmesJahu", "Logs", "vms-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .CreateLogger();

        Log.Information("=== VMS Alarmes Jahu iniciando ===");

        try
        {
            // Inicializar SDK
            IntelbrasSdk.Initialize();

            // Inicializar P2P Tunnel Manager
            P2PTunnelManager.Instance.EnsureInitialized();

            // Inicializar banco de dados
            Database.Initialize();

            // Configurar DI
            var services = new ServiceCollection();
            ConfigureServices(services);
            _services = services.BuildServiceProvider();

            // Criar e exibir MainWindow
            var mainWindow = _services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Erro fatal ao iniciar aplicação");
            MessageBox.Show($"Erro ao iniciar aplicação:\n\n{ex.Message}", "Erro Fatal", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Repositories
        services.AddSingleton<DeviceRepository>();
        services.AddSingleton<AiRuleRepository>();
        services.AddSingleton<AiEventRepository>();

        // Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<StorageService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<ConnectionManager>();

        // AI Engine
        services.AddSingleton<AiEngine>();

        // Main Window
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("=== VMS Alarmes Jahu encerrando ===");
        
        // Fechar todos os túneis P2P
        try
        {
            P2PTunnelManager.Instance?.CloseAll();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao fechar túneis P2P");
        }
        
        if (_services is IDisposable disposable)
            disposable.Dispose();
        
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
