namespace waterb.Graphs;

public interface IGraphInputCollector<TNode, TData>
{
	public void Collect((NodeDataPair<TNode, TData> from, NodeDataPair<TNode, TData> to, double? weight) data);
	public void ClearInput();
}