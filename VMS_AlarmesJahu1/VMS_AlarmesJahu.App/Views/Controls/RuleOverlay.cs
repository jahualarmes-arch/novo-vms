using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.Views.Controls;

/// <summary>
/// Overlay para desenhar regras (linhas e polígonos) sobre o vídeo
/// Este componente é um Canvas que fica em cima do Image de vídeo
/// </summary>
public class RuleOverlay : Canvas
{
    private readonly List<AiRule> _rules = new();

    public RuleOverlay()
    {
        Background = Brushes.Transparent;
        IsHitTestVisible = false; // Não intercepta cliques
        ClipToBounds = true;
    }

    /// <summary>
    /// Define as regras a serem desenhadas
    /// </summary>
    public void SetRules(IEnumerable<AiRule>? rules)
    {
        _rules.Clear();
        if (rules != null)
        {
            foreach (var r in rules)
            {
                if (r.Enabled)
                    _rules.Add(r);
            }
        }
        UpdateVisuals();
    }

    /// <summary>
    /// Adiciona uma regra ao overlay
    /// </summary>
    public void AddRule(AiRule rule)
    {
        if (rule.Enabled && !_rules.Exists(r => r.Id == rule.Id))
        {
            _rules.Add(rule);
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Remove uma regra do overlay
    /// </summary>
    public void RemoveRule(long ruleId)
    {
        _rules.RemoveAll(r => r.Id == ruleId);
        UpdateVisuals();
    }

    /// <summary>
    /// Limpa todas as regras
    /// </summary>
    public void Clear()
    {
        _rules.Clear();
        Children.Clear();
    }

    /// <summary>
    /// Atualiza os desenhos baseado no tamanho atual
    /// </summary>
    public void Refresh()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Children.Clear();

        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        foreach (var rule in _rules)
        {
            DrawRule(rule);
        }
    }

    private void DrawRule(AiRule rule)
    {
        if (rule.Points.Count < 2) return;

        try
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(rule.Color ?? "#FF0000"));
            var fillBrush = brush.Clone();
            fillBrush.Opacity = 0.2;

            if (rule.Type == AiRuleType.Polygon && rule.Points.Count >= 3)
            {
                // Desenha polígono
                var polygon = new Polygon
                {
                    Stroke = brush,
                    StrokeThickness = 2,
                    Fill = fillBrush
                };

                foreach (var p in rule.Points)
                {
                    polygon.Points.Add(new Point(p.X * ActualWidth, p.Y * ActualHeight));
                }

                Children.Add(polygon);

                // Label do polígono
                AddLabel(rule.Name, rule.Points[0], brush);
            }
            else
            {
                // Desenha linha
                var line = new Polyline
                {
                    Stroke = brush,
                    StrokeThickness = 3,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                foreach (var p in rule.Points)
                {
                    line.Points.Add(new Point(p.X * ActualWidth, p.Y * ActualHeight));
                }

                Children.Add(line);

                // Pontas da linha (setas ou círculos)
                foreach (var p in rule.Points)
                {
                    var dot = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = brush
                    };
                    SetLeft(dot, p.X * ActualWidth - 4);
                    SetTop(dot, p.Y * ActualHeight - 4);
                    Children.Add(dot);
                }

                // Label da linha
                if (rule.Points.Count >= 2)
                {
                    var midPoint = new AiPoint(
                        (rule.Points[0].X + rule.Points[1].X) / 2,
                        (rule.Points[0].Y + rule.Points[1].Y) / 2 - 0.03
                    );
                    AddLabel(rule.Name, midPoint, brush);
                }
            }
        }
        catch
        {
            // Ignora erros de cor inválida
        }
    }

    private void AddLabel(string? text, AiPoint position, Brush color)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var label = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(4, 2, 4, 2),
            Child = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            }
        };

        SetLeft(label, position.X * ActualWidth);
        SetTop(label, position.Y * ActualHeight);
        Children.Add(label);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateVisuals();
    }
}
