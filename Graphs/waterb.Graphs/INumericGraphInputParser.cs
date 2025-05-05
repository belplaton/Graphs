using System.Globalization;
using Exception = System.Exception;

namespace waterb.Graphs;

public interface INumericGraphInputCollector : IGraphInputCollector<int, int>
{

}

public interface INumericGraphInputParser : IGraphInputParser<int, int, INumericGraphInputCollector>
{

}

public sealed class RibsListNumericGraphInputParser : INumericGraphInputParser
{
	// Formatting input for numeric graph.
	// Example:
	// 10
	// 1 7 23
	// 51 85 4
	// 12 4 4
	//
	// There are first line - count of vertexes. Vertexes numbered from 1 to N 
	// Other any line is - 'from_num' 'to_num' 'weight'
	
	public bool TryParse(INumericGraphInputCollector destination, string[] input)
	{
		try
		{
			destination.ClearInput();
			if (input.Length == 0) return false;
			
			for (var i = 1; i < input.Length; i++)
			{
				var line = input[i].Trim();
				if (string.IsNullOrEmpty(line)) continue;
                    
				var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 2) continue; 
                    
				var from = int.Parse(parts[0], CultureInfo.InvariantCulture);
				var to = int.Parse(parts[1], CultureInfo.InvariantCulture);
				double? weight = parts.Length >= 3 ? double.Parse(parts[2], CultureInfo.InvariantCulture) : null;
                    
				destination.Collect(
					(new NodeDataPair<int, int>(from, from), new NodeDataPair<int, int>(to, to), weight));
			}
			
			return true;
		}
		catch
		{
			return false;
		}
	}
}

public sealed class AdjacencyListNumericGraphInputParser : INumericGraphInputParser
{
	// Formatting input for numeric graph.
	// Example:
	// 10
	// 2:6 5:8 4:7
	// 1:3 4:6
	// ...
	// 12:3 4:1 5:4
	//
	// There are first line - count of vertexes. Vertexes numbered from 1 to N (or 0 to N - 1)
	// Number of line is number of vertex
	// All lines is: 'to_number:weight'. So line#3 "4:3 9:6" is two edges: 3-4(weight=3), 3-9(weight=6)
	
	public bool TryParse(INumericGraphInputCollector destination, string[] input)
	{
		try
		{
			destination.ClearInput();
			if (input.Length == 0) return false;
                
			var vertexCount = int.Parse(input[0], CultureInfo.InvariantCulture);
			var isUseOneDigitOffset = true;
			for (var i = 1; i <= vertexCount && i < input.Length; i++)
			{
				var line = input[i].Trim();
				if (string.IsNullOrEmpty(line)) continue;
				
				var tokens = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
				foreach (var token in tokens)
				{
					double? weight = null;
					if (!int.TryParse(token, CultureInfo.InvariantCulture, out var to))
					{
						var parts = token.Split(':');
						if (parts.Length != 2) continue;
                        
						to = int.Parse(parts[0], CultureInfo.InvariantCulture);
						weight = double.Parse(parts[1], CultureInfo.InvariantCulture);
					}

					var from = i - (isUseOneDigitOffset ? 0 : 1);
					destination.Collect(
						(new NodeDataPair<int, int>(from, from), new NodeDataPair<int, int>(to, to), weight));
					if (to == 0 && isUseOneDigitOffset)
					{
						destination.ClearInput();
						isUseOneDigitOffset = false;
						i = 0;
						break;
					}
				}
			}
			
			return true;
		}
		catch
		{
			return false;
		}
	}
}

public sealed class AdjacencyMatrixNumericGraphInputParser : INumericGraphInputParser
{
	// Formatting input for numeric graph.
	// Example:
	// 5
	// 1 0 1 1 0
	// 1 0 1 0 1
	// 0 1 1 0 1
	// 1 1 1 1 0
	// 1 1 0 0 1 
	//
	// There are first line - count of vertexes. Vertexes numbered from 1 to N 
	// All other lines - is adjacency matrix where - 0 is no rib, 1 - is rib
	
	public bool TryParse(INumericGraphInputCollector destination, string[] input)
	{
		try
		{
			destination.ClearInput();
			if (input.Length == 0) return false;
                
			var vertexCount = int.Parse(input[0], CultureInfo.InvariantCulture);
			for (var i = 1; i <= vertexCount && i < input.Length; i++)
			{
				var line = input[i].Trim();
				if (string.IsNullOrEmpty(line))
					continue;
                    
				var tokens = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
				if (tokens.Length < vertexCount) throw new Exception($"In line {i} is {vertexCount} values is expected, but {tokens.Length} were obtained.");
                    
				for (var j = 0; j < vertexCount; j++)
				{
					var weight = double.Parse(tokens[j], CultureInfo.InvariantCulture);
					if (weight != 0)
					{
						destination.Collect((
							new NodeDataPair<int, int>(i, i), 
							new NodeDataPair<int, int>(j + 1, j + 1),
							Math.Abs(weight - 1) > double.Epsilon ? weight : null));
					}
				}
			}
			
			return true;
		}
		catch
		{
			return false;
		}
	}
}