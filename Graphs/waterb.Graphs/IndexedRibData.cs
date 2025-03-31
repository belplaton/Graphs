namespace waterb.Graphs;

public readonly struct IndexedRibData
{
	public readonly int fromNode;
	public readonly int toNode;
	public readonly double weight;

	public IndexedRibData(int fromNode, int toNode, double weight)
	{
		this.fromNode = fromNode;
		this.toNode = toNode;
		this.weight = weight;
	}

	public RibData<TNode> Convert<TNode, TData>(IGraph<TNode, TData> graph)
	{
		return new RibData<TNode>(graph.Nodes[fromNode], graph.Nodes[toNode], weight);
	}
}