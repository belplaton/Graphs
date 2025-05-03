namespace waterb.Graphs;

#nullable disable

public class NumericGraphBuilder<TData> : IGraphBuilder<GraphMatrix<int, TData>, int, TData>, INumericGraphInputCollector
{
	private readonly HashSet<(int from, int to, double? weight)> _ribs = [];
	private readonly HashSet<(int from, int to, double? weight)> _missingReverseRibs = [];
	private GraphSettings _desiredSettings;
	private int _maxVertex;
	private bool _isAnyWeighted;
	private bool _isRequireNodeOneOffset = true;

	private GraphSettings _settings => _desiredSettings |
		(_isAnyWeighted ? GraphSettings.IsWeighted : GraphSettings.None) |
		(_missingReverseRibs.Count != 0 ? GraphSettings.IsDirected : GraphSettings.None);

	public NumericGraphBuilder<TData> SetDesiredSettings(GraphSettings settings)
	{
		_desiredSettings = settings;
		return this;
	}
	
	public NumericGraphBuilder<TData> AddRib(int from, int to, double? weight)
	{
		if (weight != null) _isAnyWeighted = true;
		if (from == 0 || to == 0) _isRequireNodeOneOffset = false;

		var data = (from, to, weight);
		var reverseData = (to, from, weight);
		if (!_ribs.Contains(reverseData)) _missingReverseRibs.Add(reverseData);
		
		_missingReverseRibs.Remove(data);
		_ribs.Add((from, to, weight));
		
		_maxVertex = Math.Max(_maxVertex, Math.Max(to, from));
		return this;
	}
	
	public NumericGraphBuilder<TData> Clear()
	{
		_ribs.Clear();
		_missingReverseRibs.Clear();
		_desiredSettings = GraphSettings.None;
		_maxVertex = 0;
		_isAnyWeighted = false;
		_isRequireNodeOneOffset = true;
		return this;
	}

	public NumericGraphBuilder<TData> ParsePayloadInput<TParser>(string[] input)
		where TParser : INumericGraphInputParser, new()
	{
		var parser = new TParser();
		parser.TryParse(this, input);
		
		return this;
	}
	
	public void Collect((int from, int to, double? weight) data)
	{
		AddRib(data.from, data.to, data.weight);
	}

	public void ClearInput() => Clear();
	
	public GraphMatrix<int, TData> CreateGraph()
	{
		var graph = new GraphMatrix<int, TData>(_settings);

		var offset = _isRequireNodeOneOffset ? 1 : 0;
		for (var i = 0; i < _maxVertex; i++)
		{
			graph.AddNode(i + offset, default);
		}

		foreach (var rib in _ribs)
		{
			graph.SetData(rib.from, default);
			graph.SetData(rib.to, default);
			graph.SetEdge(rib.from, rib.to, rib.weight);
		}

		return graph;
	}
}