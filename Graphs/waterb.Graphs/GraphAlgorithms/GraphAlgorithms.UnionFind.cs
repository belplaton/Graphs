namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public class DisjointSetUnion
	{
		private readonly int[] _parent;
		private readonly int[] _rank;

		public DisjointSetUnion(int size)
		{
			_parent = new int[size];
			_rank = new int[size];

			for (var i = 0; i < size; i++)
			{
				_parent[i] = i;
				_rank[i] = 0;
			}
		}
		
		public int Find(int x)
		{
			return _parent[x] == x ? x : _parent[x] = Find(_parent[x]);
		}
		
		public bool Union(int x, int y)
		{
			x = Find(x);
			y = Find(y);

			if (x == y) return false;
			if (_rank[x] > _rank[y])
			{
				(x, y) = (y, x);
			}

			_rank[y] = Math.Max(_rank[y], _rank[x] + 1);
			_parent[x] = y;
			return true;
		}
	}
}