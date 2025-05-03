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