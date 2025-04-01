namespace waterb.Graphs;

public interface IGraphBuilder<out TGraph, TNode, TData>
	where TGraph : IGraph<TNode, TData>
	where TNode : notnull
{
	public TGraph CreateGraph();
}