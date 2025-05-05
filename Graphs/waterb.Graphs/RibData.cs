namespace waterb.Graphs;

public readonly struct RibData<TNode>(TNode fromNode, TNode toNode, double weight)
{
	public readonly TNode fromNode = fromNode;
	public readonly TNode toNode = toNode;
	public readonly double weight = weight;

	public override string ToString()
	{
		return $"({fromNode}, {toNode}): {weight}";
	}
}

public readonly struct RibData<TNode, TData>(NodeDataPair<TNode, TData> from, NodeDataPair<TNode, TData> to, double weight)
{
	public readonly NodeDataPair<TNode, TData> from = from;
	public readonly NodeDataPair<TNode, TData> to = to;
	public readonly double weight = weight;

	public override string ToString()
	{
		return $"({from.data}[{from.node}], {to.data}[{to.node}]): {weight}";
	}
}