using System;
using System.Collections.Generic;
using System.Windows;
using VMS_AlarmesJahu.App.Data;
using VMS_AlarmesJahu.App.IA;
using VMS_AlarmesJahu.App.Services;

namespace VMS_AlarmesJahu.App.Views;

public partial class MosaicWindow : Window
{
    public MosaicWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Inicializa a janela de mosaico com as mesmas dependências do mosaico principal.
    /// </summary>
    public void Initialize(ConnectionManager? connMgr = null,
                           DeviceRepository? deviceRepo = null,
                           AiRuleRepository? ruleRepo = null,
                           AiEngine? aiEngine = null)
    {
        Mosaic.Initialize(connMgr, deviceRepo, ruleRepo, aiEngine);
    }

    /// <summary>
    /// Retorna lista de câmeras ativas desta janela (para IA/gerenciamento).
    /// </summary>
    public List<(long DeviceId, int Channel, IntPtr PlayHandle)> GetActivePlays()
    {
        return Mosaic?.GetActivePlays() ?? new List<(long, int, IntPtr)>();
    }
}
