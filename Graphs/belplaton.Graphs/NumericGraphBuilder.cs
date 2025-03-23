namespace belplaton.Graphs;

#nullable disable

public class NumericGraphBuilder<TNode> : IGraphBuilder<GraphMatrix<int, TNode>, int, TNode> where TNode : notnull
{
	public List<(int from, int to, double? weight)> payloadData = new();
	
	public GraphMatrix<int, TNode> CreateGraph()
	{
		var graph = new GraphMatrix<int, TNode>();
		for (var i = 0; i < payloadData.Count; i++)
		{
			graph.AddNode(payloadData[i].from, default);
			graph.AddNode(payloadData[i].to, default);
			graph.SetEdgeData(payloadData[i].from, payloadData[i].to, payloadData[i].weight);
		}

		return graph;
	}
}