namespace belplaton.Graphs;

#nullable disable

public class NumericGraphBuilder<TNode> : IGraphBuilder<GraphMatrix<int, TNode>, int, TNode> where TNode : notnull
{
	private GraphSettings _settings;
	private readonly List<(int from, int to, double? weight)> _ribsPayloadData = new();

	public NumericGraphBuilder<TNode> SetSettings(GraphSettings settings)
	{
		_settings = settings;
		return this;
	}
	
	public NumericGraphBuilder<TNode> AddRib(int from, int to, double? weight)
	{
		_ribsPayloadData.Add((from, to, weight));
		return this;
	}
	
	public NumericGraphBuilder<TNode> Clear()
	{
		_ribsPayloadData.Clear();
		return this;
	}

	public NumericGraphBuilder<TNode> ParsePayloadInput<TParser>(string[] input)
		where TParser : INumericGraphInputParser, new()
	{
		var parser = new TParser();
		parser.TryParse(_ribsPayloadData, input);
		
		return this;
	}
	
	public GraphMatrix<int, TNode> CreateGraph()
	{
		var graph = new GraphMatrix<int, TNode>(_settings);
		for (var i = 0; _ribsPayloadData != null && i < _ribsPayloadData.Count; i++)
		{
			graph.AddNode(_ribsPayloadData[i].from, default);
			graph.AddNode(_ribsPayloadData[i].to, default);
			graph.SetEdgeData(_ribsPayloadData[i].from, _ribsPayloadData[i].to, _ribsPayloadData[i].weight);
		}

		return graph;
	}
}