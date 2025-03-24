using System.Globalization;

namespace belplaton.Graphs;

public interface INumericGraphInputParser
{
	public bool IsValidWeight(double weight);
	
	public bool TryParse(List<(int from, int to, double? weight)> destination, string[] input, bool clearDestination = true);
	public List<(int from, int to, double? weight)>? Parse(string[] input);
}

public class RibsListNumericGraphInputParser : INumericGraphInputParser
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
	
	public bool IsValidWeight(double weight) => true;
	
	public bool TryParse(List<(int from, int to, double? weight)> destination, string[] input, bool clearDestination = true)
	{
		try
		{
			if (clearDestination) destination.Clear();
			if (input.Length == 0) return false;
			
			var vertexCount = int.Parse(input[0], CultureInfo.InvariantCulture);
			for (var i = 1; i < input.Length; i++)
			{
				var line = input[i].Trim();
				if (string.IsNullOrEmpty(line)) continue;
                    
				var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length < 3) continue; 
                    
				var from = int.Parse(parts[0], CultureInfo.InvariantCulture);
				var to = int.Parse(parts[1], CultureInfo.InvariantCulture);
				double? weight = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    
				destination.Add((from, to, weight));
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public List<(int from, int to, double? weight)>? Parse(string[] input)
	{
		var result = new List<(int from, int to, double? weight)>();
		return TryParse(result, input) ? result : null;
	}
}

public class AdjacencyListNumericGraphInputParser : INumericGraphInputParser
{
	// Formatting input for numeric graph.
	// Example:
	// 10
	// 2:6 5:8 4:7
	// 1:3 4:6
	// ...
	// 12:3 4:1 5:4
	//
	// There are first line - count of vertexes. Vertexes numbered from 1 to N 
	// Number of line is number of vertex
	// All lines is: 'to_number:weight'. So line#3 "4:3 9:6" is two edges: 3-4(weight=3), 3-9(weight=6)

	public bool IsValidWeight(double weight) => true;
	
	public bool TryParse(List<(int from, int to, double? weight)> destination, string[] input, bool clearDestination = true)
	{
		try
		{
			if (clearDestination) destination.Clear();
			if (input.Length == 0) return false;
                
			var vertexCount = int.Parse(input[0], CultureInfo.InvariantCulture);
			for (var i = 1; i <= vertexCount && i < input.Length; i++)
			{
				var line = input[i].Trim();
				if (string.IsNullOrEmpty(line)) continue;
				
				var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var token in tokens)
				{
					var parts = token.Split(':');
					if (parts.Length != 2) continue;
                        
					var to = int.Parse(parts[0], CultureInfo.InvariantCulture);
					var weight = double.Parse(parts[1], CultureInfo.InvariantCulture);
					destination.Add((i, to, weight));
				}
			}
			
			return true;
		}
		catch
		{
			return false;
		}
	}

	public List<(int from, int to, double? weight)>? Parse(string[] input)
	{
		var result = new List<(int from, int to, double? weight)>();
		return TryParse(result, input) ? result : null;
	}
}

public class AdjacencyMatrixNumericGraphInputParser : INumericGraphInputParser
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
	
	public bool IsValidWeight(double weight) => Math.Abs(weight) > double.Epsilon;
	
	public bool TryParse(List<(int from, int to, double? weight)> destination, string[] input, bool clearDestination = true)
	{
		try
		{
			destination.Clear();
			if (input.Length == 0) return false;
                
			var vertexCount = int.Parse(input[0], CultureInfo.InvariantCulture);
			for (var i = 1; i <= vertexCount && i < input.Length; i++)
			{
				var line = input[i].Trim();
				if (string.IsNullOrEmpty(line))
					continue;
                    
				var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (tokens.Length < vertexCount) throw new Exception($"In line {i} is {vertexCount} values is expected, but {tokens.Length} were obtained.");
                    
				for (var j = 0; j < vertexCount; j++)
				{
					var value = double.Parse(tokens[j], CultureInfo.InvariantCulture);
					if (IsValidWeight(value))
					{
						destination.Add((i, j + 1, value));
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

	public List<(int from, int to, double? weight)>? Parse(string[] input)
	{
		var result = new List<(int from, int to, double? weight)>();
		return TryParse(result, input) ? result : null;
	}
}