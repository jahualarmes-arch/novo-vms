using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Views.Controls;

public class ZoneEditor : Canvas
{
    private readonly List<AiPoint> _points = new();
    private readonly List<Ellipse> _handles = new();
    private Polyline? _previewLine;
    private Polygon? _previewPolygon;
    
    private AiRuleType _ruleType = AiRuleType.Line;
    private int? _draggingIndex;
    private bool _isEditing;

    public List<AiPoint> Points => _points;
    public AiRuleType RuleType
    {
        get => _ruleType;
        set
        {
            _ruleType = value;
            UpdateVisuals();
        }
    }

    public string Color { get; set; } = "#FF0000";
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            Cursor = value ? Cursors.Cross : Cursors.Arrow;
        }
    }

    public event Action? PointsChanged;

    public ZoneEditor()
    {
        Background = Brushes.Transparent;
        ClipToBounds = true;
        
        MouseLeftButtonDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseUp;
        MouseRightButtonDown += OnRightClick;
    }

    public void SetPoints(List<AiPoint> points)
    {
        _points.Clear();
        _points.AddRange(points);
        UpdateVisuals();
    }

    public void Clear()
    {
        _points.Clear();
        UpdateVisuals();
        PointsChanged?.Invoke();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isEditing) return;

        var pos = e.GetPosition(this);
        var x = pos.X / ActualWidth;
        var y = pos.Y / ActualHeight;

        // Check if clicking on existing handle
        for (int i = 0; i < _handles.Count; i++)
        {
            var handle = _handles[i];
            var hx = Canvas.GetLeft(handle) + handle.Width / 2;
            var hy = Canvas.GetTop(handle) + handle.Height / 2;
            if (Math.Abs(pos.X - hx) < 10 && Math.Abs(pos.Y - hy) < 10)
            {
                _draggingIndex = i;
                handle.CaptureMouse();
                return;
            }
        }

        // Add new point
        if (_ruleType == AiRuleType.Line && _points.Count >= 2)
            return;

        _points.Add(new AiPoint(x, y));
        UpdateVisuals();
        PointsChanged?.Invoke();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_draggingIndex.HasValue) return;

        var pos = e.GetPosition(this);
        var x = Math.Clamp(pos.X / ActualWidth, 0, 1);
        var y = Math.Clamp(pos.Y / ActualHeight, 0, 1);

        _points[_draggingIndex.Value] = new AiPoint(x, y);
        UpdateVisuals();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggingIndex.HasValue)
        {
            _handles[_draggingIndex.Value].ReleaseMouseCapture();
            _draggingIndex = null;
            PointsChanged?.Invoke();
        }
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
        if (!_isEditing || _points.Count == 0) return;

        var pos = e.GetPosition(this);
        
        // Remove closest point
        double minDist = double.MaxValue;
        int minIdx = -1;
        for (int i = 0; i < _points.Count; i++)
        {
            var px = _points[i].X * ActualWidth;
            var py = _points[i].Y * ActualHeight;
            var dist = Math.Sqrt(Math.Pow(pos.X - px, 2) + Math.Pow(pos.Y - py, 2));
            if (dist < minDist && dist < 20)
            {
                minDist = dist;
                minIdx = i;
            }
        }

        if (minIdx >= 0)
        {
            _points.RemoveAt(minIdx);
            UpdateVisuals();
            PointsChanged?.Invoke();
        }
    }

    private void UpdateVisuals()
    {
        Children.Clear();
        _handles.Clear();

        if (_points.Count == 0) return;

        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
        var fillBrush = brush.Clone();
        fillBrush.Opacity = 0.3;

        // Draw shape
        if (_ruleType == AiRuleType.Polygon && _points.Count >= 3)
        {
            var polygon = new Polygon
            {
                Stroke = brush,
                StrokeThickness = 2,
                Fill = fillBrush
            };
            foreach (var p in _points)
                polygon.Points.Add(new Point(p.X * ActualWidth, p.Y * ActualHeight));
            Children.Add(polygon);
        }
        else if (_points.Count >= 2)
        {
            var line = new Polyline
            {
                Stroke = brush,
                StrokeThickness = 3
            };
            foreach (var p in _points)
                line.Points.Add(new Point(p.X * ActualWidth, p.Y * ActualHeight));
            Children.Add(line);
        }

        // Draw handles
        foreach (var p in _points)
        {
            var handle = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = brush,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Cursor = Cursors.Hand
            };
            Canvas.SetLeft(handle, p.X * ActualWidth - 7);
            Canvas.SetTop(handle, p.Y * ActualHeight - 7);
            Children.Add(handle);
            _handles.Add(handle);
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateVisuals();
    }
}
