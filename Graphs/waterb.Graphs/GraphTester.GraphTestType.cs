namespace waterb.Graphs;

public static partial class GraphTester
{
    [Flags]
    public enum GraphTestType
    {
        None = 0,
        ConnectedComponents = 1 << 0,
        BridgesAndJoints = 1 << 1,
        SpanningTree = 1 << 2,
        FloydWarshall = 1 << 3,
        FindGraphDuality = 1 << 4,
        FindPath = 1 << 5,
        
        Graph = ConnectedComponents | BridgesAndJoints | SpanningTree | FloydWarshall | FindGraphDuality,
        Map = FindPath
    }
}