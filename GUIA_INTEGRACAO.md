# 🔧 Guia de Integração - Modernização SIM Next

## Como Usar as Novas Views Modernizadas

Este guia explica como integrar as novas views modernizadas na sua aplicação VMS.

---

## Opção 1: Usar MainWindowModern.xaml como Janela Principal

### Passo 1: Atualizar App.xaml.cs

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // ... configuração existente (Serilog, SDK, DB) ...

        var serviceProvider = new ServiceCollection()
            // ... serviços existentes ...
            .AddSingleton<ConnectionManager>()
            .AddSingleton<DeviceRepository>()
            .AddSingleton<AiEventRepository>()
            .BuildServiceProvider();

        // Criar MainWindow moderna
        var mainWindow = new MainWindowModern();
        
        // DataContext será atribuído automaticamente nas Views
        // ou atribuir o ViewModel aqui
        var mainVM = new MainViewModel(serviceProvider.GetRequiredService<ConnectionManager>());
        mainWindow.DataContext = mainVM;
        
        mainWindow.Show();
    }
}
```

### Passo 2: Atualizar StartupUri em App.xaml

```xml
<Application x:Class="VMS_AlarmesJahu.App.App"
             StartupUri="MainWindowModern.xaml">
    <!-- ... -->
</Application>
```

---

## Opção 2: Mesclar Layout com MainWindow.xaml Existente

Se você quer manter a janela existente e só adicionar as novas views:

### Passo 1: Copiar Header do MainWindowModern

```xml
<!-- Em MainWindow.xaml, adicionar o header SIM Next -->
<Border Grid.Row="0" Background="{DynamicResource PrimaryBrush}" Padding="20,0">
    <!-- ... conteúdo do header ... -->
</Border>
```

### Passo 2: Adicionar Navegação

```xml
<StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Center">
    <Button x:Name="BtnDashboard" Content="📊 Dashboard" Click="OnDashboardClick"/>
    <Button x:Name="BtnDevices" Content="🖥️ Dispositivos" Click="OnDevicesClick"/>
    <Button x:Name="BtnLive" Content="📹 Ao Vivo" Click="OnLiveClick"/>
</StackPanel>
```

### Passo 3: No Code-Behind

```csharp
private void OnDashboardClick(object sender, RoutedEventArgs e)
{
    DashboardView.Visibility = Visibility.Visible;
    DevicesView.Visibility = Visibility.Collapsed;
    LiveViewContainer.Visibility = Visibility.Collapsed;
}

private void OnDevicesClick(object sender, RoutedEventArgs e)
{
    DashboardView.Visibility = Visibility.Collapsed;
    DevicesView.Visibility = Visibility.Visible;
    LiveViewContainer.Visibility = Visibility.Collapsed;
}

private void OnLiveClick(object sender, RoutedEventArgs e)
{
    DashboardView.Visibility = Visibility.Collapsed;
    DevicesView.Visibility = Visibility.Collapsed;
    LiveViewContainer.Visibility = Visibility.Visible;
}
```

---

## Configuração de ViewModels

### ModernDashboardViewModel

**Requisitos:**
- `ConnectionManager` - para monitorar status de dispositivos
- `DeviceRepository` - para carregar dados de dispositivos
- `AiEventRepository` - para eventos de IA

**Uso em XAML:**
```xml
<views:ModernDashboardView 
    DataContext="{Binding DashboardViewModel}"/>
```

**No Code-Behind (App.xaml.cs):**
```csharp
var dashboardVM = new ModernDashboardViewModel(
    serviceProvider.GetRequiredService<ConnectionManager>(),
    serviceProvider.GetRequiredService<DeviceRepository>(),
    serviceProvider.GetRequiredService<AiEventRepository>()
);
```

### ModernDevicesViewModel

**Requisitos:**
- `ConnectionManager` - para conectar/desconectar
- `DeviceRepository` - para carregar/salvar dispositivos

**Uso em XAML:**
```xml
<views:ModernDevicesView 
    DataContext="{Binding DevicesViewModel}"/>
```

**No Code-Behind:**
```csharp
var devicesVM = new ModernDevicesViewModel(
    serviceProvider.GetRequiredService<ConnectionManager>(),
    serviceProvider.GetRequiredService<DeviceRepository>()
);
```

---

## Customização da Paleta de Cores

### Alterar Cores SIM Next

Em `Themes/SimNext.xaml`, modificar as cores no início do arquivo:

```xml
<!-- Colors -->
<Color x:Key="SimNextPrimary">#004B94</Color>
<Color x:Key="SimNextAccent">#FF6F00</Color>
<Color x:Key="SimNextSuccess">#2E7D32</Color>
<!-- ... -->
```

### Usar Cores em Controles

```xml
<Button Background="{DynamicResource PrimaryBrush}"/>
<TextBlock Foreground="{DynamicResource SimNextErrorBrush}"/>
<Border BorderBrush="{DynamicResource SimNextBorderBrush}"/>
```

---

## Adicionar Novos Estilos

### Exemplo: Novo Estilo de Card

Em `Themes/SimNext.xaml`, adicionar:

```xml
<Style x:Key="ModernCard" TargetType="Border">
    <Setter Property="Background" Value="{StaticResource SimNextSurfaceBrush}"/>
    <Setter Property="BorderBrush" Value="{StaticResource SimNextBorderBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="CornerRadius" Value="12"/>
    <Setter Property="Padding" Value="16"/>
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect BlurRadius="8" ShadowDepth="2" Opacity="0.15"/>
        </Setter.Value>
    </Setter>
</Style>
```

### Usar o Novo Estilo

```xml
<Border Style="{StaticResource ModernCard}">
    <!-- Conteúdo do card -->
</Border>
```

---

## Integração com Gráficos

### LiveCharts2 - Configuração

Os gráficos no Dashboard já estão pré-configurados, mas você pode customizar:

```csharp
// Em ModernDashboardViewModel.cs
EventsChartSeries = new ISeries[]
{
    new LineSeries<int>
    {
        Values = eventValues,
        Fill = new SolidColorPaint(new SKColor(0, 75, 148, 150)),
        Stroke = new SolidColorPaint(new SKColor(0, 75, 148), 2),
        Name = "Eventos",
        IsVisible = true
    }
};

EventsChartXAxes = new Axis[]
{
    new DateTimeAxis(TimeSpan.FromDays(1), Formatter: x => new DateTime((long)x).ToString("dd/MM"))
};
```

### Adicionar Novo Gráfico

```xml
<!-- Em ModernDashboardView.xaml -->
<lvc:CartesianChart 
    Series="{Binding MyNewChartSeries}"
    XAxes="{Binding MyNewChartXAxes}"
    YAxes="{Binding MyNewChartYAxes}"
    Height="300"
    Margin="0,20,0,0"/>
```

---

## Comandos Disponíveis

### Dashboard Commands
```csharp
// Recarregar dados
[RelayCommand]
public void RefreshData() { /* ... */ }

// Exportar dados
[RelayCommand]
public void ExportData() { /* ... */ }
```

### Devices Commands
```csharp
// Criar novo dispositivo
[RelayCommand]
public void NewDevice() { /* ... */ }

// Conectar/desconectar
[RelayCommand]
public void Connect(Device device) { /* ... */ }

// Editar dispositivo
[RelayCommand]
public void Edit(Device device) { /* ... */ }

// Deletar dispositivo
[RelayCommand]
public void Delete(Device device) { /* ... */ }

// Atualizar lista
[RelayCommand]
public void Refresh() { /* ... */ }
```

---

## Verificação de Compilação

Após implementar, verificar:

✅ Todos os namespaces importados
✅ XAML sem erros de binding
✅ ViewModels atribuídos corretamente
✅ Repositórios injetados
✅ ConnectionManager ativo

### Comandos de Build

```bash
# Debug
dotnet build -c Debug

# Release
dotnet build -c Release

# Publicar
dotnet publish -c Release -r win-x64 --self-contained
```

---

## Troubleshooting

### Problema: "Não encontra SimNext.xaml"
**Solução:** Verificar se o arquivo existe em `Themes/SimNext.xaml`
```bash
ls -la VMS_AlarmesJahu.App/Themes/SimNext.xaml
```

### Problema: "MaterialDesignThemes não encontrado"
**Solução:** Restaurar pacotes
```bash
dotnet restore
```

### Problema: "Cores não aplicam"
**Solução:** Verificar se o recurso está incluído em App.xaml
```xml
<ResourceDictionary Source="Themes/SimNext.xaml"/>
```

### Problema: "Gráficos em branco"
**Solução:** Verificar se LiveCharts2.SkiaSharp está instalado
```bash
dotnet add package LiveCharts2.SkiaSharp
```

---

## Performance Tips

1. **Lazy Load Events**: Carregar eventos em paginação
2. **Virtual Devices Grid**: Usar VirtualizingStackPanel para muitos dispositivos
3. **Debounce Search**: Adicionar delay na busca
4. **Cache Status**: Cache de status com TTL

```csharp
// Exemplo: Debounce search
private DispatcherTimer _searchTimer;

partial void OnSearchTextChanged(string value)
{
    _searchTimer?.Stop();
    _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
    _searchTimer.Tick += (s, e) =>
    {
        _searchTimer.Stop();
        ApplyFilters();
    };
    _searchTimer.Start();
}
```

---

## Próximos Passos

1. Compilar em Windows
2. Testar ViewModels com dados reais
3. Ajustar cores conforme marca
4. Adicionar animações avançadas
5. Implementar dark mode
6. Criar versão responsiva (mobile)

---

**Dúvidas?** Consulte:
- [Copilot Instructions](.github/copilot-instructions.md)
- [P2P Testing Guide](VMS_AlarmesJahu1/P2P_TESTING_GUIDE.md)
- [Documentação Modernização](MODERNIZACAO_COMPLETADA.md)
