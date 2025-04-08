using waterb.Graphs;

internal static class Program
{
	public static void Main(string[] args)
	{
		if (args.Length > 0 && args[0] == "--help")
		{
			Console.WriteLine($"Structures Available For Test: Graph, Map");

			Console.WriteLine($"Graph Test Types: {
				string.Join(", ", Enum.GetValues<GraphTester.GraphTestType>())}");

			Console.WriteLine($"Map Test Types: {
				string.Join(", ", Enum.GetValues<GraphTester.MapTestType>())}");
		}

		if (args.Length < 4)
		{
			Console.WriteLine("Usage: dotnet run <StructureType> <TestType> <InputPath> <OutputPath>");
			Console.WriteLine("Example: dotnet run ConnectedComponents input.txt output.txt");
			Console.WriteLine("--help for more info");
			return;
		}

		var inputPath = args[2];
		var outputPath = args[3];
		try
		{
			switch (args[0])
			{
				case "Graph":
				{
					if (!Enum.TryParse<GraphTester.GraphTestType>(args[1], true, out var testType))
					{
						Console.WriteLine($"Unknown test type: {args[1]}");
						Console.WriteLine($"Graph Test Types: {
							string.Join(", ", Enum.GetValues<GraphTester.GraphTestType>())}");
						return;
					}

					Console.WriteLine("Available graph input parsers: RibList, AdjacencyList, AdjacencyMatrix");
					Console.WriteLine("Enter you`r input parser: ");
					var inputParserStr = Console.ReadLine();

					var result = inputParserStr switch
					{
						"RibList" => GraphTester.PrepareResults<RibsListNumericGraphInputParser, int>(inputPath,
							outputPath, testType),
						"AdjacencyList" => GraphTester.PrepareResults<AdjacencyListNumericGraphInputParser, int>(
							inputPath, outputPath, testType),
						"AdjacencyMatrix" => GraphTester.PrepareResults<AdjacencyMatrixNumericGraphInputParser, int>(
							inputPath, outputPath, testType),
						_ => "No result."
					};

					File.WriteAllText(outputPath, result);
					Console.WriteLine($"Result written to {outputPath}");

					break;
				}
				case "Map":
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


/*

		if (GraphMapParser.TryCreateGraphMap(lines, out var map))
		{
			var path = map!.FindPathAStar((0, 0), (14, 14), DistanceMetric.Manhattan);
			if (path != null)
			{
				Console.WriteLine($"Metrics: {DistanceMetric.Manhattan}.");
				Console.Write(map?.PrepareMapInfoWithRoute(path));
				Console.WriteLine();
			}
		}
		
		*/