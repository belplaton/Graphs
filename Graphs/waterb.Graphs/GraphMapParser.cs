using System.Globalization;

namespace waterb.Graphs;

public static class GraphMapParser
{
	public static bool TryCreateGraphMap(string[] input, out GraphMap? map)
	{
		map = null;
		try
		{
			if (input.Length == 0) return false;
			
			for (var y = 0; y < input.Length; y++)
			{
				var line = input[y].Trim();
				var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (map == null)
				{
					map = new GraphMap(input.Length, parts.Length);
				}
				else if (map.Width != parts.Length)
				{
					map = null;
					return false;
				}

				for (var x = 0; x < parts.Length; x++)
				{
					map[x, y] = double.Parse(parts[x], CultureInfo.InvariantCulture);
					if (map[x, y] == 0) map[x, y] = null;
				}
			}
			
			return map != null;
		}
		catch
		{
			return false;
		}
	}
}