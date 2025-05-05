namespace waterb.Graphs;

public readonly struct NodeDataPair<TNode, TData>(TNode node, TData data)
{
	public readonly TNode node = node;
	public readonly TData data = data;
}