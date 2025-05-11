using System.Text;
using waterb.Graphs.GraphAlgorithms;
using waterb.Graphs.MapAlgorithms;

namespace waterb.Graphs;

public static partial class GraphTester
{
    public static string PrepareResults<TNumericGraphInputParser>(string pathInput, GraphTestType mapTestType)
        where TNumericGraphInputParser : INumericGraphInputParser, new()
    {
        if (!File.Exists(pathInput))
        {
            Console.WriteLine($"file'{pathInput}' is not found!");
            return "No results.";
        }
		
        var lines = File.ReadAllLines(pathInput);
        var builder = new NumericGraphBuilder();
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
            case GraphTestType.FindGraphMaxBipartiteMatching:
                var r7 = graph.FindMaxBipartiteMatching();
                if (r7 != null)
                {
                    sb.AppendLine($"Maximum matching number: {r7.Count}");
                    sb.AppendLine("Matching maximum size:");
                    sb.AppendLine($"[{string.Join(", ", r7)}]");
                }
                else
                {
                    sb.AppendLine("Graph is not bipartite");
                }
                
                break;
            case GraphTestType.FordBellmanShortestPath:
                Console.WriteLine("Enter start node name (integer): ");
                var t8Input = Console.ReadLine();
                if (!int.TryParse(t8Input, out var t8StartNode))
                {
                    Console.WriteLine("Error with parsing start node");
                    return "No results.";
                }
                
                var r8 = graph.FindShortestPathsFordBellman(t8StartNode);
                if (r8 != null)
                {
                    sb.AppendLine($"Shortest paths lengths from node {t8StartNode}: ");
                    sb.AppendLine($"[{string.Join(", ", r8.Select(x => $"{x.Key}: {x.Value}"))}]");
                }
                else
                {
                    sb.AppendLine("Graph includes a negative cycle.");
                }
                
                break;
            case GraphTestType.FordFulkersonMaxFlow:
                Console.WriteLine("Enter flow node name (integer): ");
                var t9Input1 = Console.ReadLine();
                if (!int.TryParse(t9Input1, out var t9FlowNode))
                {
                    Console.WriteLine("Error with parsing start node");
                    return "No results.";
                }
                
                Console.WriteLine("Enter sink node name (integer): ");
                var t9Input2 = Console.ReadLine();
                if (!int.TryParse(t9Input2, out var t9SinkNode))
                {
                    Console.WriteLine("Error with parsing start node");
                    return "No results.";
                }
                
                var r9 = graph.FindMaxFlowFordFulkerson(t9FlowNode, t9SinkNode);
                sb.AppendLine($"Maximum flow value: {r9?.maxFlow ?? 0}");
                if (r9.HasValue)
                {
                    sb.AppendLine($"Flow list: [{string.Join(", ", r9.Value.flow)}]");
                }
                
                break;
            case GraphTestType.HamiltonianCycleAntColony:
                var r10 = graph.FindHamiltonianCycleAntColony();
                sb.AppendLine($"Length of shortest traveling salesman path is: {r10?.pathLength ?? 0}");
                if (r10.HasValue)
                {
                    sb.AppendLine($"Traveling Path: [{string.Join(", ", r10.Value.path)}]");
                }
                
                break;
            case GraphTestType.HamiltonianCycleBnB:
                var r11 = graph.FindHamiltonianCycleBnB();
                sb.AppendLine($"Length of shortest traveling salesman path is: {r11?.pathLength ?? 0}");
                if (r11.HasValue)
                {
                    sb.AppendLine($"Traveling Path: [{string.Join(", ", r11.Value.path)}]");
                }
                
                break;
            case GraphTestType.PlanarIdentification:
                var r12 = graph.CheckPlanarNaive();
                sb.AppendLine($"Graph {(r12 ? "is" : "is not")} planar.");
                break;
            case GraphTestType.PlanarGammaIdentification:
                var r13 = graph.CheckPlanarGamma();
                sb.AppendLine($"Graph {(r13 ? "is" : "is not")} planar.");
                break;
            case GraphTestType.PartitionFourByDistance:
                Console.WriteLine("Enter start node name (integer): ");
                var t14Input1 = Console.ReadLine();
                if (!int.TryParse(t14Input1, out var t14StartNode))
                {
                    Console.WriteLine("Error with parsing start node");
                    return "No results.";
                }
                
                var r14 = graph.PartitionByDistance(t14StartNode, 4);
                sb.AppendLine($"Distance from {t14StartNode} vertex:");
                for (var i = 0; i < r14.Count; i++)
                {
                    sb.AppendLine($"{r14[i].FromDistance} <= d < {r14[i].ToDistance}");
                    sb.AppendLine($"\t[{string.Join(", ", r14[i].Nodes)}]");
                }

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