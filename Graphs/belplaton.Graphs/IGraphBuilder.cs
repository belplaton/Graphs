namespace belplaton.Graphs;

public interface IGraphBuilder<TGraph, TNode, TData>
	where TGraph : IGraph<TNode, TData>
	where TNode : notnull
{
	public TGraph CreateGraph();
}