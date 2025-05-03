using System.Text;
using waterb.Graphs.GraphAlgorithms;
using waterb.Graphs.MapAlgorithms;

namespace waterb.Graphs;

public static partial class GraphTester
{
    public static string PrepareResults<TNumericGraphInputParser, TData>(string pathInput, GraphTestType mapTestType)
        where TNumericGraphInputParser : INumericGraphInputParser, new()
    {
        if (!File.Exists(pathInput))
        {
            Console.WriteLine($"file'{pathInput}' is not found!");
            return "No results.";
        }
		
        var lines = File.ReadAllLines(pathInput);
        var builder = new NumericGraphBuilder<TData>();
        builder.ParsePayloadInput<TNumericGraphInputParser>(lines);
        var graph = builder.CreateGraph();
        var sb = new StringBuilder();

        sb.AppendLine($"GraphMatrix: Size={graph.Size}, Settings={graph.Settings}\n");
        switch (mapTestType)
        {
            case GraphTestType.ConnectedComponents:
                if ((graph.Settings & GraphSettings.IsDirected) != 0)
                {
                    sb.AppendLine("Graph is directed\n");
                    
                    var r1Weak = graph.FindWeakConnectedComponents();
                    if (r1Weak is { Count: 1 })
                        sb.AppendLine("Digraph is weakly connected");
                    else sb.AppendLine("Digraph is not weakly connected");
                    sb.AppendLine("Weak Connected Components:");
                    if (r1Weak != null) sb.AppendLine(string.Join("\n", r1Weak));
                    sb.AppendLine();
                    
                    var r1Strong = graph.FindStrongConnectedComponents();
                    if (r1Strong is { Count: 1 })
                        sb.AppendLine("Digraph is strongly connected");
                    else sb.AppendLine("Digraph is not strongly connected");
                    sb.AppendLine("Strong Connected Components:");
                    if (r1Strong != null) sb.AppendLine(string.Join("\n", r1Strong));
                }
                else
                {
                    sb.AppendLine("Graph is not directed\n");
                    var r1 = graph.FindConnectedComponents();
                    sb.AppendLine("Connected Components:");
                    if (r1 != null) sb.AppendLine(string.Join("\n", r1));
                }
                
                break;
            case GraphTestType.BridgesAndJoints:
                var r2 = graph.FindJointsAndBridges();
                sb.AppendLine("Joints list:");
                if (r2 != null) sb.AppendLine(r2.Value.joints.ToString());
                sb.AppendLine("Bridges list:");
                if (r2 != null) sb.AppendLine(r2.Value.bridges.ToString());
                
                break;
            case GraphTestType.DFSSpanningTree:
                var r3 = graph.BuildSpanningTreeDFS();
                sb.AppendLine("Spanning Tree ribs:");
                if (r3 != null) sb.AppendLine(string.Join(", ", r3));   
                
                break;
            case GraphTestType.MinSpanningTree:
                var r3Prim = graph.BuildMinimumSpanningTreePrim();
                sb.AppendLine("Min Spanning Tree Prim:");
                if (r3Prim != null) sb.AppendLine(string.Join(", ", r3Prim));
                sb.AppendLine();
                
                var r3Kruskal = graph.BuildMinimumSpanningTreeKruskal();
                sb.AppendLine("Min Spanning Tree Kruskal:");
                if (r3Kruskal != null) sb.AppendLine(string.Join(", ", r3Kruskal));   
                sb.AppendLine();
                
                var r3Boruvka = graph.BuildMinimumSpanningTreeBoruvka();
                sb.AppendLine("Min Spanning Tree Boruvka:");
                if (r3Boruvka != null) sb.AppendLine(string.Join(", ", r3Boruvka));   
                
                break;
            case GraphTestType.FloydWarshall:
                var r4 = graph.ComputeFloydWarshallData();
                sb.AppendLine("Floyd Warshall Data:");
                if (r4 != null) sb.AppendLine(r4.Value.data.ToString());
                
                break;
            case GraphTestType.ComponentsFloydWarshall:
                var r5 = graph.ComputeComponentsFloydWarshallData();
                sb.AppendLine("Components Floyd Warshall Data:");
                if (r5 != null)
                {
                    var count = 0;
                    foreach (var pair in r5)
                    {
                        sb.AppendLine($"Component #{count++}");
                        sb.AppendLine(pair.Key.ToString());
                        sb.AppendLine(pair.Value.ToString());
                    }
                }
                
                break;
            case GraphTestType.FindGraphBipartite:
                var r6 = graph.TryGetBipartitePartition(out var r5PartA, out var r5PartB);
                sb.AppendLine("Bipartite First Set:");
                if (r6) sb.AppendLine($"[{string.Join(", ", r5PartA!)}]");
                sb.AppendLine("Bipartite Second Set:");
                if (r6) sb.AppendLine($"[{string.Join(", ", r5PartB!)}]");
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mapTestType), mapTestType, null);
        }

        return sb.ToString();
    }
    
    public static string PrepareResults(string pathInput, MapTestType mapTestType)
    {
        if (!File.Exists(pathInput))
        {
            Console.WriteLine($"file'{pathInput}' is not found!");
            return "No results.";
        }
		
        var lines = File.ReadAllLines(pathInput);
        if (GraphMapParser.TryCreateGraphMap(lines, out var map))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"GraphMap: Height={map!.Height}, Width={map.Width}\n");
            switch (mapTestType)
            {
                case MapTestType.FindPath:
                    Console.WriteLine("Path From (x, y) without braces: ");
                    if (!TryParseCoordinates(Console.ReadLine(), out var from1))
                    {
                        Console.WriteLine("Error with parsing coordinates");
                        return "No results.";
                    }
                    
                    Console.WriteLine("Path To (x, y) without braces: ");
                    if (!TryParseCoordinates(Console.ReadLine(), out var to1))
                    {
                        Console.WriteLine("Error with parsing coordinates");
                        return "No results.";
                    }
                    
                    var r1 = map.FindPathDijkstra(from1, to1);
                    if (r1 != null)
                    {
                        sb.AppendLine(map.PrepareMapInfoPath(r1.Value.path, r1.Value.length, true));
                    }
                
                    break;
                case MapTestType.FindPathAStar:
                    Console.WriteLine("Path From (x, y) without braces: ");
                    if (!TryParseCoordinates(Console.ReadLine(), out var from2))
                    {
                        Console.WriteLine("Error with parsing coordinates");
                        return "No results.";
                    }
                    
                    Console.WriteLine("Path To (x, y) without braces: ");
                    if (!TryParseCoordinates(Console.ReadLine(), out var to2))
                    {
                        Console.WriteLine("Error with parsing coordinates");
                        return "No results.";
                    }
                    
                    Console.WriteLine($"Enter distance metric [Metrics: {
                        string.Join(", ", Enum.GetNames<DistanceMetric>())}]: ");
                    var metricStr = Console.ReadLine();
                    if (!Enum.TryParse<DistanceMetric>(metricStr, out var metric))
                    {
                        Console.WriteLine("Error with parsing distance metric");
                        return "No results.";
                    }
                
                    var r2 = map.FindPathAStar(from2, to2, metric);
                    if (r2 != null)
                    {
                        sb.AppendLine($"Metrics: {metric}.");
                        sb.AppendLine(map.PrepareMapInfoPath(r2.Value.path, r2.Value.length, true));
                        
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapTestType), mapTestType, null);
            }
            
            return sb.ToString();
        }

        return "No results.";
    }

    private static bool TryParseCoordinates(string? input, out (int x, int y) result)
    {
        result = default;
        if (input == null) return false;
        var pair = input.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
        if (pair.Length < 2) return false;
        if (!int.TryParse(pair[0], out var first)) return false;
        if (!int.TryParse(pair[1], out var second)) return false;
        result = (first, second);
        return true;
    }
}