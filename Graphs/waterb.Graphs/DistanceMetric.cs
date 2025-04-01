namespace waterb.Graphs;

/// <summary>
/// - Euclid: (sqrt((x1 - x2)^2 + (y1 - y2)^2))
/// - Manhattan: |x1 - x2| + |y1 - y2|
/// - Chebyshev: max(|x1 - x2|, |y1 - y2|)
/// </summary>
public enum DistanceMetric
{
	Euclid = 0,
	Manhattan = 1,
	Chebyshev = 2
}