using System.Text;

namespace waterb.Graphs;

using System;

public sealed class GraphMap
{
    private readonly double[][] _mapMatrix;
    public int Height { get; }
    public int Width { get; }

    public GraphMap(int height, int width)
    {
        Height = height;
        Width = width;
        _mapMatrix = new double[height][];
        for (var i = 0; i < height; i++)
        {
            _mapMatrix[i] = new double[width];
        }
    }

    public double this[int x, int y]
    {
        get => _mapMatrix[y][x];
        set => _mapMatrix[y][x] = value;
    }
    
    public double GetDistance((int x, int y) from, (int x, int y) to)
    {
        var dx = Math.Abs(from.x - to.x);
        var dy = Math.Abs(from.y - to.y);
        var dh = Math.Abs(_mapMatrix[from.y][from.x] - _mapMatrix[to.y][to.x]);
        return dx + dy + dh;
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
}