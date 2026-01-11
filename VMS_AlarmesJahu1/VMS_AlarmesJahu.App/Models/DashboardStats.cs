using System;
using System.Collections.Generic;

namespace VMS_AlarmesJahu.App.Models;

public class DashboardStats
{
    public int TotalEventsToday { get; set; }
    public int TotalEventsWeek { get; set; }
    public int TotalEventsMonth { get; set; }
    public int ConnectedDevices { get; set; }
    public int TotalDevices { get; set; }
    public int ActiveRules { get; set; }
    public List<HourlyCount> EventsByHour { get; set; } = new();
    public List<ChannelCount> TopChannels { get; set; } = new();
    public List<ClassCount> TopClasses { get; set; } = new();
    public DateTime? LastEventTime { get; set; }
    public TimeSpan Uptime { get; set; }
}

public class HourlyCount
{
    public int Hour { get; set; }
    public int Count { get; set; }
    public string Label => $"{Hour:D2}:00";
}

public class ChannelCount
{
    public string DeviceName { get; set; } = "";
    public int Channel { get; set; }
    public int Count { get; set; }
    public string Label => $"{DeviceName} CH{Channel + 1}";
}

public class ClassCount
{
    public string ClassName { get; set; } = "";
    public int Count { get; set; }
}
