using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VMS_AlarmesJahu.App.Models;

public class AiRule
{
    public long Id { get; set; }
    public long DeviceId { get; set; }
    public int Channel { get; set; }
    public string Name { get; set; } = "Regra";
    public AiRuleType Type { get; set; } = AiRuleType.Line;
    public List<AiPoint> Points { get; set; } = new();
    public List<string> Classes { get; set; } = new() { "person" };
    public AiDirection Direction { get; set; } = AiDirection.Both;
    public double Confidence { get; set; } = 0.35;
    public int CooldownSec { get; set; } = 10;
    public bool Enabled { get; set; } = true;
    public string Color { get; set; } = "#FF0000";
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string PointsJson
    {
        get => JsonSerializer.Serialize(Points);
        set => Points = string.IsNullOrEmpty(value) ? new() : JsonSerializer.Deserialize<List<AiPoint>>(value) ?? new();
    }

    public string ClassesJson
    {
        get => JsonSerializer.Serialize(Classes);
        set => Classes = string.IsNullOrEmpty(value) ? new() : JsonSerializer.Deserialize<List<string>>(value) ?? new();
    }
}

public class AiPoint
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }

    public AiPoint() { }
    public AiPoint(double x, double y) { X = x; Y = y; }
}

public enum AiRuleType
{
    Line,
    Polygon,
    TripWire
}

public enum AiDirection
{
    Both,
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}
