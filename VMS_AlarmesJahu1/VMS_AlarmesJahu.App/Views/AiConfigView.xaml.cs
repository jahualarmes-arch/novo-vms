using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.Models;
using VMS_AlarmesJahu.App.Services;
using VMS_AlarmesJahu.App.Views.Controls;
using VMS_AlarmesJahu.App.Sdk;

namespace VMS_AlarmesJahu.App.Views;

public partial class AiConfigView : UserControl
{
    private DeviceRepository? _deviceRepo;
    private AiRuleRepository? _ruleRepo;
    private ConnectionManager? _connectionManager;
    
    // Substituindo HwndHost por Image + HiddenWindow
    private HiddenNativeWindow? _hiddenWindow;
    private Image? _previewImage;
    private IntPtr _playHandle;
    private DispatcherTimer? _snapshotTimer;
    
    private long _editingRuleId;

    public AiConfigView()
    {
        InitializeComponent();
        
        CbDevice.SelectionChanged += (s, e) => OnDeviceChanged();
        CbType.SelectionChanged += (s, e) => OnTypeChanged();
        CbColor.SelectionChanged += (s, e) => OnColorChanged();
        
        BtnSave.Click += (s, e) => SaveRule();
        BtnClear.Click += (s, e) => ClearPoints();
        BtnPreview.Click += (s, e) => TogglePreview();
        
        ZoneEditor.PointsChanged += () => UpdatePointsLabel();
        
        Unloaded += (s, e) => StopPreview();
    }

    public void Initialize(DeviceRepository deviceRepo, AiRuleRepository ruleRepo, ConnectionManager connMgr)
    {
        _deviceRepo = deviceRepo;
        _ruleRepo = ruleRepo;
        _connectionManager = connMgr;
        
        LoadDevices();
        LoadRules();
    }

    private void LoadDevices()
    {
        if (_deviceRepo == null) return;
        var devices = _deviceRepo.GetAll();
        CbDevice.ItemsSource = devices;
        if (devices.Count > 0)
            CbDevice.SelectedIndex = 0;
    }

    private void OnDeviceChanged()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null) return;

        CbChannel.Items.Clear();
        for (int i = 0; i < device.ChannelCount; i++)
            CbChannel.Items.Add($"Canal {i + 1}");
        
        if (CbChannel.Items.Count > 0)
            CbChannel.SelectedIndex = 0;

        LoadRules();
        StopPreview();
    }

    private void OnTypeChanged()
    {
        var isPolygon = CbType.SelectedIndex == 1;
        ZoneEditor.RuleType = isPolygon ? AiRuleType.Polygon : AiRuleType.Line;
        ZoneEditor.Clear();
    }

    private void OnColorChanged()
    {
        var item = CbColor.SelectedItem as ComboBoxItem;
        var color = item?.Tag?.ToString() ?? "#FF0000";
        ZoneEditor.Color = color;
    }

    private void LoadRules()
    {
        if (_ruleRepo == null || _deviceRepo == null) return;
        
        var device = CbDevice.SelectedItem as Device;
        if (device == null) return;

        var rules = _ruleRepo.GetByDevice(device.Id);
        LvRules.ItemsSource = rules;
    }

    private void SaveRule()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null)
        {
            MessageBox.Show("Selecione um dispositivo.");
            return;
        }

        var points = ZoneEditor.Points;
        var isPolygon = CbType.SelectedIndex == 1;

        if (isPolygon && points.Count < 3)
        {
            MessageBox.Show("Uma área precisa de pelo menos 3 pontos.");
            return;
        }
        if (!isPolygon && points.Count < 2)
        {
            MessageBox.Show("Uma linha precisa de 2 pontos.");
            return;
        }

        var colorItem = CbColor.SelectedItem as ComboBoxItem;
        var classes = TxtClasses.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var rule = new AiRule
        {
            Id = _editingRuleId,
            DeviceId = device.Id,
            Channel = CbChannel.SelectedIndex,
            Name = TxtName.Text,
            Type = isPolygon ? AiRuleType.Polygon : AiRuleType.Line,
            Points = points.ToList(),
            Classes = classes,
            Confidence = SliderConf.Value,
            CooldownSec = (int)SliderCooldown.Value,
            Enabled = ChkEnabled.IsChecked == true,
            Color = colorItem?.Tag?.ToString() ?? "#FF0000"
        };

        if (_editingRuleId > 0)
        {
            _ruleRepo?.Update(rule);
            MessageBox.Show("Regra atualizada!");
        }
        else
        {
            rule.Id = _ruleRepo?.Insert(rule) ?? 0;
            MessageBox.Show("Regra criada!");
        }

        _editingRuleId = 0;
        ClearPoints();
        LoadRules();
    }

    private void ClearPoints()
    {
        ZoneEditor.Clear();
        _editingRuleId = 0;
    }

    private void UpdatePointsLabel()
    {
        TxtPoints.Text = $"Pontos: {ZoneEditor.Points.Count}";
    }

    private void TogglePreview()
    {
        if (_playHandle != IntPtr.Zero)
        {
            StopPreview();
            BtnPreview.Content = "👁️ Abrir Preview";
        }
        else
        {
            OpenPreview();
            BtnPreview.Content = "⏹️ Fechar Preview";
        }
    }

    private void OpenPreview()
    {
        var device = CbDevice.SelectedItem as Device;
        if (device == null || _connectionManager == null) return;

        StopPreview();

        var login = _connectionManager.GetLogin(device.Id);
        if (login == IntPtr.Zero)
        {
            MessageBox.Show("Dispositivo não conectado.");
            return;
        }

        // Criar Image para exibir snapshots - UniformToFill igual ao mosaico
        _previewImage = new Image
        {
            Stretch = System.Windows.Media.Stretch.UniformToFill
        };
        VideoPreviewHost.Content = _previewImage;
        TxtPlaceholder.Visibility = Visibility.Collapsed;

        // Criar janela oculta para o stream
        _hiddenWindow = new HiddenNativeWindow();

        // Iniciar play na janela oculta
        _playHandle = IntelbrasSdk.RealPlay(login, CbChannel.SelectedIndex, _hiddenWindow.Handle, 0);

        if (_playHandle == IntPtr.Zero)
        {
            MessageBox.Show("Erro ao iniciar preview.");
            StopPreview();
            return;
        }

        // Iniciar timer de snapshots (8 FPS)
        _snapshotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(125) };
        _snapshotTimer.Tick += OnSnapshotTick;
        _snapshotTimer.Start();
    }

    private void OnSnapshotTick(object? sender, EventArgs e)
    {
        if (_playHandle == IntPtr.Zero || _previewImage == null) return;

        try
        {
            var jpegData = IntelbrasSdk.SnapshotJpegBytesFromPlay(_playHandle, 60);
            if (jpegData != null)
            {
                var bitmap = ImageUtil.BytesToBitmapSource(jpegData);
                if (bitmap != null)
                {
                    _previewImage.Source = bitmap;
                }
            }
        }
        catch
        {
            // Ignora erros de snapshot
        }
    }

    private void StopPreview()
    {
        if (_snapshotTimer != null)
        {
            _snapshotTimer.Stop();
            _snapshotTimer.Tick -= OnSnapshotTick;
            _snapshotTimer = null;
        }

        if (_playHandle != IntPtr.Zero)
        {
            IntelbrasSdk.StopRealPlay(_playHandle);
            _playHandle = IntPtr.Zero;
        }

        _hiddenWindow?.Dispose();
        _hiddenWindow = null;

        VideoPreviewHost.Content = null;
        _previewImage = null;
        TxtPlaceholder.Visibility = Visibility.Visible;
    }

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var ruleId = (long)(btn?.Tag ?? 0L);
        var rule = _ruleRepo?.GetById(ruleId);
        if (rule == null) return;

        _editingRuleId = rule.Id;
        TxtName.Text = rule.Name;
        CbType.SelectedIndex = rule.Type == AiRuleType.Polygon ? 1 : 0;
        TxtClasses.Text = string.Join(", ", rule.Classes);
        SliderConf.Value = rule.Confidence;
        SliderCooldown.Value = rule.CooldownSec;
        ChkEnabled.IsChecked = rule.Enabled;

        // Selecionar cor
        for (int i = 0; i < CbColor.Items.Count; i++)
        {
            var item = CbColor.Items[i] as ComboBoxItem;
            if (item?.Tag?.ToString() == rule.Color)
            {
                CbColor.SelectedIndex = i;
                break;
            }
        }

        ZoneEditor.RuleType = rule.Type;
        ZoneEditor.Color = rule.Color;
        ZoneEditor.SetPoints(rule.Points);
        UpdatePointsLabel();
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var ruleId = (long)(btn?.Tag ?? 0L);

        if (MessageBox.Show("Excluir esta regra?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            _ruleRepo?.Delete(ruleId);
            LoadRules();
        }
    }

    private void ChkRule_Changed(object sender, RoutedEventArgs e)
    {
        var chk = sender as CheckBox;
        var ruleId = (long)(chk?.Tag ?? 0L);
        var rule = _ruleRepo?.GetById(ruleId);
        if (rule != null)
        {
            rule.Enabled = chk?.IsChecked == true;
            _ruleRepo?.Update(rule);
        }
    }
}
