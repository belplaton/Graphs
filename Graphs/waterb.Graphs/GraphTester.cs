using System.Text;
using waterb.Graphs.GraphAlgorithms;

namespace waterb.Graphs;

public static partial class GraphTester
{
    public static string PrepareResults<TNumericGraphInputParser, TData>(
        string pathInput, string pathOutput, GraphTestType graphTestType)
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
        switch (graphTestType)
        {
            case GraphTestType.ConnectedComponents:
                if ((graph.Settings & GraphSettings.IsDirected) != 0)
                {
                    var r1Weak = graph.FindWeakConnectedComponents();
                    sb.AppendLine("Weak Connected Components:");
                    if (r1Weak != null) sb.AppendLine(string.Join("\n", r1Weak));
                    
                    var r1Strong = graph.FindStrongConnectedComponents();
                    sb.AppendLine("Strong Connected Components:");
                    if (r1Strong != null) sb.AppendLine(string.Join("\n", r1Strong));
                }
                else
                {
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
                
                var r3Kruskal = graph.BuildMinimumSpanningTreeKruskal();
                sb.AppendLine("Min Spanning Tree Prim:");
                if (r3Kruskal != null) sb.AppendLine(string.Join(", ", r3Kruskal));   
                
                var r3Boruvka = graph.BuildMinimumSpanningTreeBoruvka();
                sb.AppendLine("Min Spanning Tree Prim:");
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
        }

        return sb.ToString();
    }
}