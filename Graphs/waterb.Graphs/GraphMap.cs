using System.Text;

namespace waterb.Graphs;

using System;

public sealed class GraphMap
{
    private readonly double?[][] _mapMatrix;
    public int Height { get; }
    public int Width { get; }

    public GraphMap(int height, int width)
    {
        Height = height;
        Width = width;
        _mapMatrix = new double?[height][];
        for (var i = 0; i < height; i++)
        {
            _mapMatrix[i] = new double?[width];
        }
    }

    public double? this[int x, int y]
    {
        get => _mapMatrix[y][x];
        set => _mapMatrix[y][x] = value;
    }
    
    public bool TryGetDistance((int x, int y) from, (int x, int y) to, out double distance)
    {
        distance = 0;
        if (!this[from.x, from.y].HasValue || !this[to.x, to.y].HasValue) return false;
        distance = Math.Abs(this[from.x, from.y]!.Value - this[to.x, to.y]!.Value);
        return true;
    }
    
    public static double GetHeuristicsDistance((int x, int y) from, (int x, int y) to, DistanceMetric metric)
    {
        var dx = Math.Abs(from.x - to.x);
        var dy = Math.Abs(from.y - to.y);

        return metric switch
        {
            DistanceMetric.Euclid =>
                // sqrt((r1 - r2)^2 + (c1 - c2)^2)
                Math.Sqrt(dx * dx + dy * dy),
            DistanceMetric.Manhattan =>
                // |r1 - r2| + |c1 - c2|
                dx + dy,
            DistanceMetric.Chebyshev =>
                // max(|r1 - r2|, |c1 - c2|)
                Math.Max(dx, dy),
            _ => throw new NotImplementedException($"{metric}")
        };
    }
    
    public List<(int x, int y)> GetNeighbors(int x, int y)
    {
        var result = new List<(int x, int y)>();
        if (x > 0 && x - 1 < Width) result.Add((x - 1, y));
        if (x + 1 < Width) result.Add((x + 1, y));
        if (y > 0 && y - 1 < Height) result.Add((x, y -1));
        if (y + 1 < Height) result.Add((x, y + 1));

        return result;
    }
    
    public string PrepareMapInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"GraphMap: Height={Height}, Width={Width}");
        
        sb.AppendLine();
        sb.AppendLine("Map Matrix:");
        sb.Append("     ");
        for (var i = 0; i < Width; i++)
        {
            sb.Append($"{i,8:0.##}");
        }
			
        sb.AppendLine();
        for (var y = 0; y < Height; y++)
        {
            sb.Append($"{y,4}:");
            for (var x = 0; x < Width; x++)
            {
                sb.Append($"{_mapMatrix[y][x],8:0.##}");
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    public string PrepareMapInfoRoute(List<(int x, int y)> route, bool printRouteOnMap)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"GraphMap: Height={Height}, Width={Width}\n");
        sb.AppendLine("Map Matrix with Route:\n");
        
        sb.AppendLine("Route List: [");
        var routeSet = new HashSet<(int x, int y)>();
        for (var i = 0; i < route.Count; i++)
        {
            routeSet.Add(route[i]);
            sb.Append($"({route[i].x}, {route[i].y}), ");
        }

        sb.Append("]\n");
    
        const string RedStart = "\e[31m";
        const string ResetColor = "\e[0m";

        double count = 0;
        double prev = 0;

        if (printRouteOnMap)
        {
            sb.AppendLine();
            for (var x = 0; x < Width; x++)
            {
                sb.Append($"{x,8}");
            }

            sb.AppendLine();
        }

        for (var y = 0; y < Height; y++)
        {
            if (printRouteOnMap) sb.Append($"{y,4}:");
            for (var x = 0; x < Width; x++)
            {
                if (routeSet.Contains((x, y)))
                {
                    if (printRouteOnMap) sb.Append($"{RedStart}{_mapMatrix[y][x],8}{ResetColor}");
                    count += Math.Abs(_mapMatrix[y][x]!.Value - prev);
                    prev = _mapMatrix[y][x]!.Value;
                }
                else if (printRouteOnMap)
                {
                    sb.Append(_mapMatrix[y][x].HasValue ? $"{_mapMatrix[y][x],8:0.##}" : $"{"-",8}");
                }
            }
            
            if (printRouteOnMap) sb.AppendLine();
        }
    
        
        sb.AppendLine();
        sb.AppendLine($"GraphMap: Length={count}");
        return sb.ToString();
    }
}