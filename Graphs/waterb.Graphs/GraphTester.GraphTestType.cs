namespace waterb.Graphs;

public static partial class GraphTester
{
    public enum GraphTestType
    {
        ConnectedComponents,
        BridgesAndJoints,
        DFSSpanningTree,
        MinSpanningTree,
        FloydWarshall,
        ComponentsFloydWarshall,
        FindGraphBipartite,
        FindGraphMaxBipartiteMatching,
        FordBellmanShortestPath,
        FordFulkersonMaxFlow,
        HamiltonianCycleAntColony,
        HamiltonianCycleBnB
    }
}