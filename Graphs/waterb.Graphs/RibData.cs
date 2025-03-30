namespace waterb.Graphs;

public readonly struct RibData<TNode>
{
	public readonly TNode fromNode;
	public readonly TNode toNode;
	public readonly double weight;

	public RibData(TNode fromNode, TNode toNode, double weight)
	{
		this.fromNode = fromNode;
		this.toNode = toNode;
		this.weight = weight;
	}
}