using waterb.Utility;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    public static bool TryGetBipartitePartition<TNode, TData>(this IGraph<TNode, TData> graph,
        out List<TNode>? partA, out List<TNode>? partB) where TNode : notnull
    {
        HashSet<TNode>? visited = [];
        Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack = [];
        return graph.TryGetBipartitePartition(ref visited, ref stack, out partA, out partB);
    }

    public static bool TryGetBipartitePartition<TNode, TData>(this IGraph<TNode, TData> graph,
        ref HashSet<TNode>? visited, ref Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack,
        out List<TNode>? partA, out List<TNode>? partB) where TNode : notnull
    {
        if (graph.Size == 0)
        {
            partA = new List<TNode>();
            partB = new List<TNode>();
            return true;
        }
        
        (visited ??= new HashSet<TNode>()).Clear();
        (stack ??= new Stack<DFSEnumerator<TNode, TData>.DFSNode>()).Clear();
        var color = new bool?[graph.Size];
        var isValid = true;
        
        for (var i = 0; i < graph.Size; i++)
        {
            var node = graph.Nodes[i];
            if (!color[i].HasValue)
            {
                using var enumerator = new DFSEnumerator<TNode, TData>(graph, node, visited, stack, 
                    (current, g, s, v) => OnPrepareStackChanges(current, g, s, v, color, ref isValid));
                while (enumerator.MoveNext())
                {
                    if (!isValid)
                    {
                        partA = null;
                        partB = null;
                        return false;
                    }
                    
                    color[graph.GetIndex(enumerator.Current.Node)!.Value] = enumerator.Current.Depth % 2 == 0;
                }
            }
        }

        partA = new List<TNode>();
        partB = new List<TNode>();
        for (var i = 0; i < graph.Size; i++)
        {
            if (color[i]!.Value)
                partA.Add(graph.Nodes[i]);
            else
                partB.Add(graph.Nodes[i]);
        }

        return true;
        static void OnPrepareStackChanges(DFSEnumerator<TNode, TData>.DFSNode current, IGraph<TNode, TData> graph,
            Stack<DFSEnumerator<TNode, TData>.DFSNode> stack, HashSet<TNode> visited, bool?[] color, ref bool isValid)
        {
            var currentIndex = graph.GetIndex(current.Node)!.Value;
            for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
            {
                if (graph[adjIndex][currentIndex].HasValue)
                {
                    if (!visited.Contains(graph.Nodes[adjIndex]))
                    {
                        stack.Push(new DFSEnumerator<TNode, TData>.DFSNode(graph.Nodes[adjIndex], current.Depth + 1));
                    }
                    else if (color[adjIndex].HasValue && color[adjIndex] != ((current.Depth + 1) % 2 == 0))
                    {
                        isValid = false;
                    }
                }
            }
        }
    }

    public static List<RibData<TNode>>? FindMaxBipartiteMatching<TNode, TData>(this IGraph<TNode, TData> graph)
        where TNode : notnull
    {
        if (!graph.TryGetBipartitePartition(out var partA, out var partB)) return null;

        var lessPart = partA!.Count < partB!.Count ? partA : partB;
        var result = new List<RibData<TNode>>();

        var matching = new Dictionary<TNode, TNode>();
        var visited = new HashSet<TNode>();
        for (var i = 0; i < lessPart.Count; i++)
        {
            visited.Clear();
            TryFindAugmentingPath(graph, lessPart[i], visited, matching);
        }

        for (var i = 0; i < graph.Size; i++)
        {
            var node = graph.Nodes[i];
            if (matching.TryGetValue(node, out var matchedNode))
            {
                result.Add(new RibData<TNode>(node, matchedNode, graph.GetWeight(node, matchedNode)!.Value));
            }
        }
        
        return result.Count > 0 ? result : null;
    }
    
    private static bool TryFindAugmentingPath<TNode, TData>(IGraph<TNode, TData> graph, 
        TNode startNode, HashSet<TNode> visited, Dictionary<TNode, TNode> matching) where TNode : notnull
    {
        if (!visited.Add(startNode)) return false;
        for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
        {
            var adjNode = graph.Nodes[adjIndex];
            if (graph[startNode][adjIndex].HasValue)
            {
                if (!matching.ContainsKey(adjNode) ||
                    TryFindAugmentingPath(graph, matching[adjNode], visited, matching))
                {
                    matching[adjNode] = startNode;
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // non-oriented graph only
    public static bool CheckPlanarNaive<TNode, TData>(this IGraph<TNode, TData> graph) where TNode : notnull
    {
        if (graph.Size < 5) return true;
        var components = graph.FindConnectedComponents();
        if (components == null) return true;
        for (var i = 0; i < components.Count; i++)
        {
            if (!IsComponentPlanar(graph, components[i])) return false;
        }

        return true;

        static bool IsComponentPlanar(IGraph<TNode, TData> graph, ConnectedComponent<TNode> component)
        {
            if (component.Count <= 2) return true;

            var nodes = new IndexedSet<TNode>(component.Nodes);
            var adjacency = new bool[component.Count][];
            for (var i = 0; i < component.Count; i++) adjacency[i] = new bool[graph.Size];
            for (var i = 0; i < component.Count; i++)
            {
                for (var j = i + 1; j < component.Count; j++)
                {
                    adjacency[i][j] = adjacency[j][i] =
                        graph[component[i]][graph.GetIndex(component[j])!.Value].HasValue ||
                        graph[component[j]][graph.GetIndex(component[i])!.Value].HasValue;
                }
            }

            while (nodes.Count > 3)
            {
                var lowDegreeVertexIndex = FindLowDegreeVertex(adjacency, nodes.Count);
                if (lowDegreeVertexIndex == -1) return false;

                var degree = Degree(adjacency, lowDegreeVertexIndex, nodes.Count);
                if (degree > 1)
                {
                    int a = -1, b = -1;
                    for (var j = 0; j < nodes.Count; j++)
                    {
                        if (adjacency[lowDegreeVertexIndex][j])
                        {
                            if (a == -1) a = j;
                            else { b = j; break; }
                        }
                    }
                    
                    if (a != -1 && b != -1 && !adjacency[a][b])
                    {
                        adjacency[a][b] = adjacency[b][a] = true;
                    }
                }

                RemoveVertex(adjacency, nodes, lowDegreeVertexIndex);
            }
            
            return true;
        }
        
        static int Degree(bool[][] adjacency, int vertexIndex, int size)
        {
            var degree = 0;
            for (var j = 0; j < size; j++) if (adjacency[vertexIndex][j]) degree++;
            return degree;
        }

        static int FindLowDegreeVertex(bool[][] adjacency, int size)
        {
            for (var i = 0; i < size; i++) 
                if (Degree(adjacency, i, size) <= 2) return i;
            return -1;
        }

        static void RemoveVertex(bool[][] adjacency, IndexedSet<TNode> nodes, int vertexIndex)
        {
            var size = nodes.Count;
            for (var i = vertexIndex; i < size - 1; i++)
                for (var j = 0; j < size; j++)
                    adjacency[i][j] = adjacency[i + 1][j];

            for (var j = vertexIndex; j < size - 1; j++)
                for (var i = 0; i < size - 1; i++)
                    adjacency[i][j] = adjacency[i][j + 1];

            nodes.RemoveByIndex(vertexIndex);
        }
    }
    
    // non-oriented graph only
    public static bool CheckPlanarGamma<TNode, TData>(this IGraph<TNode, TData> graph) where TNode : notnull
    {
        if (graph.Size < 5) return true;

        var adjacency = new bool[graph.Size][];
        for (var i = 0; i < graph.Size; i++) adjacency[i] = new bool[graph.Size];
        for (var i = 0; i < graph.Size; i++)
        {
            for (var j = i + 1; j < graph.Size; j++)
            {
                adjacency[i][j] = adjacency[j][i] = graph[i][j].HasValue || graph[j][i].HasValue;
            }
        }

        var visited = new bool[graph.Size];
        var visitedCount = 0;
        
        bool changed;
        do
        {
            changed = false;
            for (var currentIndex = 0; currentIndex < graph.Size; currentIndex++)
            {
                if (visited[currentIndex]) continue;
                
                var degree = 0;
                for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
                {
                    if (!visited[adjIndex] && adjacency[currentIndex][adjIndex]) degree++;
                }

                switch (degree)
                {
                    case <= 1:
                    {
                        for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
                        {
                            adjacency[currentIndex][adjIndex] = adjacency[adjIndex][currentIndex] = false;
                        }
                    
                        visited[currentIndex] = true;
                        visitedCount++;
                        changed = true;
                        break;
                    }
                    case 2:
                        int a = -1, b = -1;
                        for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
                        {
                            if (!visited[adjIndex] && adjacency[currentIndex][adjIndex])
                            {
                                if (a == -1) a = adjIndex;
                                else b = adjIndex;
                            }
                        }
                        
                        if (a != -1 && b != -1)
                        {
                            adjacency[a][currentIndex] = adjacency[currentIndex][a] = false;
                            adjacency[b][currentIndex] = adjacency[currentIndex][b] = false;
                            adjacency[a][b] = adjacency[b][a] = true;
                        }
                        
                        adjacency[currentIndex][currentIndex] = false;
                        visited[currentIndex] = true;
                        visitedCount++;
                        changed = true;
                        break;
                }
            }
        } while (changed);

        var remainSize = graph.Size - visitedCount;
        if (remainSize < 5) return true;

        var comb5 = new int[5];
        var coreIndices = new int[remainSize];
        for (int i = 0, j = 0; i < graph.Size; i++) if (!visited[i]) coreIndices[j++] = i;
        
        if (DFSK5(0, 0, comb5, coreIndices, adjacency, remainSize)) return false;
        if (remainSize < 6) return true;
        
        var leftComb3 = new int[3];
        var rightComb3 = new int[3];
        var rightInvertComb3 = new int[remainSize - 3];
        if (DFSK33Left(0, 0, leftComb3, rightComb3, rightInvertComb3, coreIndices, adjacency, remainSize)) return false;
        
        return true;
        static bool DFSK5(int depth, int currentIndex,
            int[] comb5, int[] coreIndices, bool[][] adjacency, int remainSize)
        {
            if (depth == 5)
            {
                for (var i = 0; i < 5; i++)
                {
                    for (var j = i + 1; j < 5; j++)
                    {
                        if (!adjacency[comb5[i]][comb5[j]]) return false;
                    }
                }
                
                return true;
            }
            
            for (var i = currentIndex; i <= remainSize - (5 - depth); i++)
            {
                comb5[depth] = coreIndices[i];
                if (DFSK5(depth + 1, i + 1, comb5, coreIndices, adjacency, remainSize)) return true;
            }
            
            return false;
        }
        
        static bool DFSK33Left(int depth, int currentIndex, int[] leftComb3, int[] rightComb3,
            int[] rightInvertComb3, int[] coreIndices, bool[][] adjacency, int remainSize)
        {
            if (depth == 3)
            {
                var rightInvertCombCount = 0;
                for (var i = 0; i < remainSize; i++)
                {
                    var inLeftComb = false;
                    for (var j = 0; j < 3; j++) if (coreIndices[i] == leftComb3[j]) { inLeftComb = true; break; }
                    if (!inLeftComb) rightInvertComb3[rightInvertCombCount++] = coreIndices[i];
                }
                
                if (rightInvertCombCount >= 3)
                {
                    if (ChooseRight(0, 0, rightInvertCombCount, leftComb3, rightComb3,
                        rightInvertComb3, adjacency)) return true;
                    static bool ChooseRight(int depth, int currentIndex, int rightInvertCombCount,
                        int[] leftComb3, int[] rightComb3, int[] rightInvertComb3, bool[][] adjacency)
                    {
                        if (depth == 3) return IsK33(leftComb3, rightComb3, adjacency);
                        for (var i = currentIndex; i <= rightInvertCombCount - (3 - depth); i++)
                        {
                            rightComb3[depth] = rightInvertComb3[i];
                            if (ChooseRight(depth + 1, i + 1, rightInvertCombCount, leftComb3, rightComb3,
                                rightInvertComb3, adjacency)) return true;
                        }
                        
                        return false;
                    }
                }
                return false;
            }
            
            for (var i = currentIndex; i <= remainSize - (3 - depth); i++)
            {
                leftComb3[depth] = coreIndices[i];
                if (DFSK33Left(depth + 1, i + 1, leftComb3, rightComb3, rightInvertComb3, coreIndices, adjacency, remainSize)) 
                    return true;
            }
            
            return false;
            static bool IsK33(int[] leftComb3, int[] rightComb3, bool[][] adjacency)
            {
                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {   
                        if (j > i)
                        {
                            if (adjacency[leftComb3[i]][leftComb3[j]] || 
                                adjacency[rightComb3[i]][rightComb3[j]]) return false;
                        }

                        if (!adjacency[leftComb3[i]][rightComb3[j]]) return false;
                    }
                }
            
                return true;
            }
        }
    }
}