using waterb.Graphs;

internal static class Program
{
	public static void Main(string[] args)
	{
		if (args.Length > 0 && args[0] == "--help")
		{
			Console.WriteLine($"Structures Available For Test: Graph, Map\n");

			Console.WriteLine($"Graph Test Types: {
				string.Join(", ", Enum.GetValues<GraphTester.GraphTestType>())}\n");

			Console.WriteLine($"Map Test Types: {
				string.Join(", ", Enum.GetValues<GraphTester.MapTestType>())}\n");
		}

		if (args.Length < 3)
		{
			Console.WriteLine("Usage: <StructureType> <TestType> <InputPath> <OutputPath>(optional)\n");
			
			Console.WriteLine("Example: Graph ConnectedComponents input.txt output.txt\n");
			
			Console.WriteLine("--help for more info\n");
			return;
		}

		var inputPath = args[2];
		var outputPath = args.ElementAtOrDefault(3);
		if (!File.Exists(inputPath))
		{
			Console.WriteLine($"file'{inputPath}' is not found!");
			return;
		}
		
		try
		{
			switch (args[0])
			{
				case "Graph":
				{
					if (!Enum.TryParse<GraphTester.GraphTestType>(args[1], true, out var graphTestType))
					{
						Console.WriteLine($"Unknown test type: {args[1]}");
						Console.WriteLine($"Graph Test Types: {
							string.Join(", ", Enum.GetValues<GraphTester.GraphTestType>())}");
						return;
					}

					Console.WriteLine("Available graph input parsers: RibList, AdjacencyList, AdjacencyMatrix");
					Console.WriteLine("Enter you`r input parser: ");
					var inputParserStr = Console.ReadLine();

					var result1 = inputParserStr switch
					{
						"RibList" => GraphTester.PrepareResults<RibsListNumericGraphInputParser, int>(
							inputPath, graphTestType),
						"AdjacencyList" => GraphTester.PrepareResults<AdjacencyListNumericGraphInputParser, int>(
							inputPath, graphTestType),
						"AdjacencyMatrix" => GraphTester.PrepareResults<AdjacencyMatrixNumericGraphInputParser, int>(
							inputPath, graphTestType),
						_ => "No result."
					};

					Console.WriteLine(result1);
					if (outputPath != null)
					{
						File.WriteAllText(outputPath, result1);
						Console.WriteLine($"Result written to {outputPath}");
					}

					break;
				}
				case "Map":
					if (!Enum.TryParse<GraphTester.MapTestType>(args[1], true, out var mapTestType))
					{
						Console.WriteLine($"Unknown test type: {args[1]}");
						Console.WriteLine($"Map Test Types: {
							string.Join(", ", Enum.GetValues<GraphTester.MapTestType>())}");
						return;
					}

					var result2 = GraphTester.PrepareResults(inputPath, mapTestType);
					Console.WriteLine(result2);
					if (outputPath != null)
					{
						File.WriteAllText(outputPath, result2);
						Console.WriteLine($"Result written to {outputPath}");
					}

					break;
				default:
					Console.WriteLine($"Unknown structure type: {args[0]}");
					Console.WriteLine("Available types: Graph, Map");
					break;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error during {args[0]} processing:");
			Console.WriteLine(ex.Message);
		}
	}
}
