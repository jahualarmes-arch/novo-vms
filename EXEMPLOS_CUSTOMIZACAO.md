# 🎨 Exemplos de Customização - SIM Next

## Guia com Código de Exemplo para Extensões

Este documento fornece snippets de código prontos para estender a modernização.

---

## 1. Dark Mode

### Criar Tema Dark

```xml
<!-- Themes/SimNextDark.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    
    <Color x:Key="SimNextPrimary">#0D47A1</Color>
    <Color x:Key="SimNextBackground">#1E1E1E</Color>
    <Color x:Key="SimNextSurface">#2D2D2D</Color>
    <Color x:Key="SimNextTextPrimary">#FFFFFF</Color>
    <Color x:Key="SimNextTextSecondary">#BDBDBD</Color>
    <Color x:Key="SimNextBorder">#424242</Color>
    
    <!-- ... resto dos recursos ... -->
</ResourceDictionary>
```

### ViewModel para Dark Mode

```csharp
public partial class ThemeViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isDarkMode = false;

    [RelayCommand]
    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var app = Application.Current;
        var dict = app.Resources.MergedDictionaries[0];
        
        if (IsDarkMode)
        {
            app.Resources.MergedDictionaries[0] = 
                new ResourceDictionary { Source = new Uri("Themes/SimNextDark.xaml", UriKind.Relative) };
        }
        else
        {
            app.Resources.MergedDictionaries[0] = 
                new ResourceDictionary { Source = new Uri("Themes/SimNext.xaml", UriKind.Relative) };
        }
        
        Log.Information("Tema alterado para: {Mode}", IsDarkMode ? "Escuro" : "Claro");
    }
}
```

---

## 2. Dialogos Modernizados

### Dialog para Novo Dispositivo

```xaml
<Window x:Class="VMS_AlarmesJahu.App.Views.NewDeviceDialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Novo Dispositivo"
        Width="500" Height="600"
        Background="{DynamicResource Bg}"
        WindowStartupLocation="CenterOwner"
        ShowInTaskbar="False"
        WindowStyle="None"
        AllowsTransparency="True">

    <Border CornerRadius="12" 
            Background="{DynamicResource Card}"
            BorderBrush="{DynamicResource Border}"
            BorderThickness="1"
            Padding="24">
        
        <StackPanel Spacing="16">
            <!-- Título -->
            <TextBlock Text="Adicionar Novo Dispositivo"
                       Style="{DynamicResource Heading1}"/>
            
            <!-- Nome -->
            <StackPanel>
                <TextBlock Text="Nome do Dispositivo" 
                           Margin="0,0,0,8"
                           FontWeight="SemiBold"/>
                <TextBox x:Name="TxtName" 
                         Padding="12,10"
                         BorderThickness="1"
                         BorderBrush="{DynamicResource Border}"
                         CornerRadius="6"/>
            </StackPanel>
            
            <!-- Tipo de Conexão -->
            <StackPanel>
                <TextBlock Text="Tipo de Conexão" 
                           Margin="0,0,0,8"
                           FontWeight="SemiBold"/>
                <ComboBox x:Name="CmbConnectionType"
                          Padding="12,10">
                    <ComboBoxItem>IP Direto</ComboBoxItem>
                    <ComboBoxItem>P2P Cloud</ComboBoxItem>
                </ComboBox>
            </StackPanel>
            
            <!-- IP/Host (IP Direto) -->
            <StackPanel x:Name="DirectIPPanel">
                <TextBlock Text="Endereço IP" 
                           Margin="0,0,0,8"
                           FontWeight="SemiBold"/>
                <TextBox x:Name="TxtHost" 
                         Padding="12,10"
                         Placeholder="192.168.1.100"/>
            </StackPanel>
            
            <!-- Serial (P2P Cloud) -->
            <StackPanel x:Name="P2PPanel" Visibility="Collapsed">
                <TextBlock Text="Número de Série" 
                           Margin="0,0,0,8"
                           FontWeight="SemiBold"/>
                <TextBox x:Name="TxtSerial" 
                         Padding="12,10"
                         Placeholder="ABC123456789"/>
            </StackPanel>
            
            <!-- Porta -->
            <StackPanel>
                <TextBlock Text="Porta" 
                           Margin="0,0,0,8"
                           FontWeight="SemiBold"/>
                <TextBox x:Name="TxtPort" 
                         Padding="12,10"
                         Text="37777"/>
            </StackPanel>
            
            <!-- Usuário e Senha -->
            <Grid ColumnSpacing="12">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <StackPanel Grid.Column="0">
                    <TextBlock Text="Usuário" Margin="0,0,0,8"/>
                    <TextBox x:Name="TxtUsername" Padding="12,10"/>
                </StackPanel>
                
                <StackPanel Grid.Column="1">
                    <TextBlock Text="Senha" Margin="0,0,0,8"/>
                    <PasswordBox x:Name="PwdPassword" Padding="12,10"/>
                </StackPanel>
            </Grid>
            
            <!-- Botões -->
            <StackPanel Orientation="Horizontal" 
                        HorizontalAlignment="Right" 
                        Spacing="10" 
                        Margin="0,20,0,0">
                <Button Content="Cancelar" 
                        Click="OnCancel"
                        Style="{DynamicResource OutlineButton}"
                        Width="100"/>
                <Button Content="Criar" 
                        Click="OnCreate"
                        Style="{DynamicResource AccentButton}"
                        Width="100"/>
            </StackPanel>
        </StackPanel>
    </Border>
</Window>
```

### Code-Behind do Dialog

```csharp
public partial class NewDeviceDialog : Window
{
    public Device CreatedDevice { get; private set; }

    public NewDeviceDialog()
    {
        InitializeComponent();
        CmbConnectionType.SelectionChanged += (s, e) => UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool isDirect = CmbConnectionType.SelectedIndex == 0;
        DirectIPPanel.Visibility = isDirect ? Visibility.Visible : Visibility.Collapsed;
        P2PPanel.Visibility = isDirect ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs()) return;

        CreatedDevice = new Device
        {
            Name = TxtName.Text,
            Host = TxtHost.Text,
            Port = int.Parse(TxtPort.Text),
            SerialNumber = TxtSerial.Text,
            ConnectionType = CmbConnectionType.SelectedIndex == 0 ? ConnectionType.Direct : ConnectionType.P2PCloud,
            Username = TxtUsername.Text,
            ChannelCount = 4
        };

        CreatedDevice.SetPassword(PwdPassword.Password);

        DialogResult = true;
        Close();
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("Nome é obrigatório", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(TxtPort.Text, out int port) || port <= 0)
        {
            MessageBox.Show("Porta inválida", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }
}
```

---

## 3. Toast Notifications

### Serviço de Notificações Moderno

```csharp
public class ToastNotificationService
{
    private readonly List<ToastWindow> _activeToasts = new();

    public void ShowSuccess(string title, string message, int duration = 3000)
    {
        ShowToast(title, message, ToastLevel.Success, duration);
    }

    public void ShowWarning(string title, string message, int duration = 5000)
    {
        ShowToast(title, message, ToastLevel.Warning, duration);
    }

    public void ShowError(string title, string message, int duration = 5000)
    {
        ShowToast(title, message, ToastLevel.Error, duration);
    }

    public void ShowInfo(string title, string message, int duration = 3000)
    {
        ShowToast(title, message, ToastLevel.Info, duration);
    }

    private void ShowToast(string title, string message, ToastLevel level, int duration)
    {
        var toast = new ToastWindow(title, message, level);
        _activeToasts.Add(toast);
        
        toast.Show();
        
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(duration) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            toast.Close();
            _activeToasts.Remove(toast);
        };
        timer.Start();
        
        Log.Information("Toast: {Level} - {Title}: {Message}", level, title, message);
    }
}

public enum ToastLevel
{
    Success,
    Warning,
    Error,
    Info
}
```

### Toast Window UI

```xaml
<Window x:Class="VMS_AlarmesJahu.App.Views.ToastWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Width="350" Height="100"
        Background="Transparent"
        AllowsTransparency="True"
        WindowStyle="None"
        ShowInTaskbar="False"
        Topmost="True">

    <Border CornerRadius="8" 
            Background="{DynamicResource Card}"
            BorderBrush="{DynamicResource Border}"
            BorderThickness="2"
            Padding="16"
            x:Name="MainBorder">
        
        <StackPanel Spacing="8">
            <TextBlock x:Name="TitleBlock" 
                       FontWeight="Bold" 
                       FontSize="14"/>
            <TextBlock x:Name="MessageBlock" 
                       FontSize="12" 
                       Opacity="0.8" 
                       TextWrapping="Wrap"/>
        </StackPanel>
    </Border>
</Window>
```

---

## 4. Filtros Avançados

### Modal de Filtros

```xaml
<Window x:Class="VMS_AlarmesJahu.App.Views.AdvancedFiltersWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Filtros Avançados"
        Width="450" Height="550"
        WindowStartupLocation="CenterOwner">

    <Grid Padding="20">
        <StackPanel Spacing="16">
            <!-- Status -->
            <GroupBox Header="Status" Padding="12">
                <StackPanel Spacing="8">
                    <CheckBox Content="Conectado"/>
                    <CheckBox Content="Desconectado"/>
                    <CheckBox Content="Erro"/>
                    <CheckBox Content="Offline"/>
                </StackPanel>
            </GroupBox>

            <!-- Tipo de Conexão -->
            <GroupBox Header="Tipo de Conexão" Padding="12">
                <StackPanel Spacing="8">
                    <CheckBox Content="IP Direto"/>
                    <CheckBox Content="P2P Cloud"/>
                </StackPanel>
            </GroupBox>

            <!-- Canais -->
            <GroupBox Header="Canais" Padding="12">
                <StackPanel Spacing="8">
                    <CheckBox Content="4 Canais"/>
                    <CheckBox Content="8 Canais"/>
                    <CheckBox Content="16 Canais"/>
                </StackPanel>
            </GroupBox>

            <!-- Data de Conexão -->
            <GroupBox Header="Última Conexão" Padding="12">
                <StackPanel Spacing="8">
                    <TextBlock Text="De:" FontWeight="SemiBold" Margin="0,0,0,4"/>
                    <DatePicker/>
                    <TextBlock Text="Até:" FontWeight="SemiBold" Margin="0,8,0,4"/>
                    <DatePicker/>
                </StackPanel>
            </GroupBox>

            <!-- Botões -->
            <StackPanel Orientation="Horizontal" 
                        HorizontalAlignment="Right" 
                        Spacing="10" 
                        Margin="0,20,0,0">
                <Button Content="Limpar" Width="100"/>
                <Button Content="Aplicar" Width="100"/>
            </StackPanel>
        </StackPanel>
    </Grid>
</Window>
```

---

## 5. Indicador de Progresso Customizado

### Progress Circle

```xaml
<UserControl x:Class="VMS_AlarmesJahu.App.Controls.CircleProgressControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid>
        <!-- Background Circle -->
        <Ellipse Fill="{DynamicResource SimNextSurfaceBrush}"
                 Stroke="{DynamicResource SimNextBorderBrush}"
                 StrokeThickness="2"/>

        <!-- Progress Arc (usar Transform) -->
        <Path x:Name="ProgressArc"
              Stroke="{DynamicResource SimNextAccentBrush}"
              StrokeThickness="4"
              StrokeLineCap="Round"/>

        <!-- Percentage Text -->
        <TextBlock x:Name="PercentageText"
                   Text="0%"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"
                   FontSize="24"
                   FontWeight="Bold"
                   Foreground="{DynamicResource SimNextTextBrush}"/>
    </Grid>
</UserControl>
```

### Code-Behind

```csharp
public partial class CircleProgressControl : UserControl
{
    private double _progress = 0;

    public double Progress
    {
        get => _progress;
        set
        {
            if (_progress == value) return;
            _progress = Math.Clamp(value, 0, 100);
            UpdateProgress();
        }
    }

    public CircleProgressControl()
    {
        InitializeComponent();
    }

    private void UpdateProgress()
    {
        PercentageText.Text = $"{_progress:F0}%";
        // Atualizar Arc Path baseado em Progress
    }
}
```

---

## 6. Search com Auto-Complete

### Búsqueda com Sugestões

```xaml
<Grid>
    <StackPanel Spacing="8">
        <TextBox x:Name="SearchBox"
                 Padding="12,10"
                 TextChanged="OnSearchTextChanged"
                 Placeholder="Buscar dispositivos..."/>
        
        <ListBox x:Name="SuggestionsList"
                 ItemsSource="{Binding Suggestions}"
                 MaxHeight="200"
                 BorderThickness="1"
                 BorderBrush="{DynamicResource Border}">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Padding="12,8" 
                            Background="Transparent"
                            MouseUp="OnSuggestionSelected">
                        <StackPanel>
                            <TextBlock Text="{Binding Name}" 
                                       FontWeight="SemiBold"/>
                            <TextBlock Text="{Binding Info}" 
                                       Opacity="0.6" 
                                       FontSize="11"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </StackPanel>
</Grid>
```

### ViewModel

```csharp
public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DeviceSuggestion> suggestions = new();

    [ObservableProperty]
    private string searchText = "";

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Suggestions.Clear();
            return;
        }

        var filtered = _devices
            .Where(d => d.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
            .Select(d => new DeviceSuggestion
            {
                Name = d.Name,
                Info = d.ConnectionType == ConnectionType.Direct 
                    ? $"📡 {d.Host}:{d.Port}"
                    : $"☁️ {d.SerialNumber}",
                Device = d
            })
            .Take(5);

        Suggestions.Clear();
        foreach (var suggestion in filtered)
            Suggestions.Add(suggestion);
    }
}

public class DeviceSuggestion
{
    public string Name { get; set; }
    public string Info { get; set; }
    public Device Device { get; set; }
}
```

---

## 7. Exportar Dashboard para PDF

### Serviço de Exportação

```csharp
public class ExportService
{
    public void ExportDashboardToPdf(ModernDashboardViewModel vm, string filePath)
    {
        var doc = new PdfDocument();
        var page = doc.AddPage();
        var xgfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Segoe UI", 12);
        var titleFont = new XFont("Segoe UI", 20, XFontStyle.Bold);

        // Título
        xgfx.DrawString("Relatório do Dashboard", titleFont, XBrushes.Black, 
            new XPoint(50, 50));

        // KPIs
        int y = 100;
        xgfx.DrawString($"Dispositivos Conectados: {vm.ConnectedDevices}", font, XBrushes.Black, 
            new XPoint(50, y)); y += 30;
        xgfx.DrawString($"Canais Ativos: {vm.ActiveChannels}", font, XBrushes.Black, 
            new XPoint(50, y)); y += 30;
        xgfx.DrawString($"Eventos Hoje: {vm.EventsToday}", font, XBrushes.Black, 
            new XPoint(50, y)); y += 30;
        xgfx.DrawString($"Uptime: {vm.SystemUptime:F1}%", font, XBrushes.Black, 
            new XPoint(50, y));

        doc.Save(filePath);
        Log.Information("Dashboard exportado para: {Path}", filePath);
    }
}
```

---

## 8. Hotkeys/Atalhos de Teclado

### Input Bindings

```xaml
<Window.InputBindings>
    <KeyBinding Key="N" Modifiers="Ctrl" Command="{Binding NewDeviceCommand}"/>
    <KeyBinding Key="R" Modifiers="Ctrl" Command="{Binding RefreshCommand}"/>
    <KeyBinding Key="F" Modifiers="Ctrl" Command="{Binding FocusSearchCommand}"/>
    <KeyBinding Key="Escape" Command="{Binding CloseDialogCommand}"/>
</Window.InputBindings>
```

---

## 9. Preferências do Usuário

### Settings Window

```csharp
public partial class PreferencesViewModel : ObservableObject
{
    [ObservableProperty]
    private bool enableNotifications = true;

    [ObservableProperty]
    private int refreshInterval = 30; // segundos

    [ObservableProperty]
    private bool darkMode = false;

    [RelayCommand]
    public void Save()
    {
        var settings = new AppSettings
        {
            EnableNotifications = EnableNotifications,
            RefreshInterval = RefreshInterval,
            DarkMode = DarkMode
        };

        _settingsService.Save(settings);
        Log.Information("Preferências salvas");
    }

    [RelayCommand]
    public void RestoreDefaults()
    {
        EnableNotifications = true;
        RefreshInterval = 30;
        DarkMode = false;
    }
}
```

---

## Conclusão

Todos os exemplos acima podem ser integrados à modernização SIM Next. 

**Próximos passos:**
1. Escolher quais customizações implementar
2. Testar em Windows
3. Ajustar cores conforme necessário
4. Publicar versão final

---

**Suporte:** Consulte a documentação principal para mais detalhes.
