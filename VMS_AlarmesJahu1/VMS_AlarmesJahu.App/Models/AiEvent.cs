using System;

namespace VMS_AlarmesJahu.App.Models;

public class AiEvent
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public string DeviceName { get; set; } = "";
    public int Channel { get; set; }
    public long? RuleId { get; set; }
    public string RuleName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public double Confidence { get; set; }
    public string EventType { get; set; } = ""; // LINE, AREA, MANUAL
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? SnapshotPath { get; set; }
    public string? ClipPath { get; set; }
    public bool Acknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? Notes { get; set; }

    // Computed
    public string ChannelDisplay => $"CH {Channel + 1}";
    public string TimeDisplay => Timestamp.ToString("dd/MM/yyyy HH:mm:ss");
    public string ConfidenceDisplay => $"{Confidence:P0}";
}
