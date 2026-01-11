using System;
using System.Collections.Generic;
using System.Linq;
using VMS_AlarmesJahu.App.Models;

namespace VMS_AlarmesJahu.App.IA;

public static class RuleEvaluator
{
    private const double LineTolerance = 0.025; // 2.5% do frame como tolerância

    public static bool Match(AiRule rule, IaDetection detection, int frameWidth, int frameHeight)
    {
        if (!rule.Enabled) return false;
        if (detection.conf < rule.Confidence) return false;
        
        // Verificar classe
        if (rule.Classes.Count > 0 && !rule.Classes.Contains(detection.class_name, StringComparer.OrdinalIgnoreCase))
            return false;

        // Normalizar centro da detecção para 0..1
        var cx = detection.Cx / frameWidth;
        var cy = detection.Cy / frameHeight;

        return rule.Type switch
        {
            AiRuleType.Polygon => PointInPolygon(cx, cy, rule.Points),
            AiRuleType.Line => NearLine(cx, cy, rule.Points, LineTolerance),
            AiRuleType.TripWire => CrossesLine(detection, rule.Points, frameWidth, frameHeight),
            _ => false
        };
    }

    public static bool PointInPolygon(double x, double y, List<AiPoint> polygon)
    {
        if (polygon.Count < 3) return false;

        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            var xi = polygon[i].X;
            var yi = polygon[i].Y;
            var xj = polygon[j].X;
            var yj = polygon[j].Y;

            var intersect = ((yi > y) != (yj > y)) &&
                            (x < (xj - xi) * (y - yi) / (yj - yi + 1e-12) + xi);
            if (intersect) inside = !inside;
        }
        return inside;
    }

    public static bool NearLine(double x, double y, List<AiPoint> points, double tolerance)
    {
        if (points.Count < 2) return false;
        
        var a = points[0];
        var b = points[1];
        var distance = DistancePointToSegment(x, y, a.X, a.Y, b.X, b.Y);
        return distance <= tolerance;
    }

    public static bool CrossesLine(IaDetection detection, List<AiPoint> points, int w, int h)
    {
        // Para tripwire, precisaríamos de tracking entre frames
        // Por enquanto, usa a mesma lógica de NearLine
        if (points.Count < 2) return false;
        
        var cx = detection.Cx / w;
        var cy = detection.Cy / h;
        return NearLine(cx, cy, points, LineTolerance);
    }

    public static double DistancePointToSegment(double px, double py, double ax, double ay, double bx, double by)
    {
        var vx = bx - ax;
        var vy = by - ay;
        var wx = px - ax;
        var wy = py - ay;

        var c1 = vx * wx + vy * wy;
        if (c1 <= 0)
            return Math.Sqrt((px - ax) * (px - ax) + (py - ay) * (py - ay));

        var c2 = vx * vx + vy * vy;
        if (c2 <= c1)
            return Math.Sqrt((px - bx) * (px - bx) + (py - by) * (py - by));

        var t = c1 / c2;
        var projX = ax + t * vx;
        var projY = ay + t * vy;
        return Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
    }

    public static (double X, double Y) GetPolygonCenter(List<AiPoint> points)
    {
        if (points.Count == 0) return (0.5, 0.5);
        var x = points.Average(p => p.X);
        var y = points.Average(p => p.Y);
        return (x, y);
    }
}
