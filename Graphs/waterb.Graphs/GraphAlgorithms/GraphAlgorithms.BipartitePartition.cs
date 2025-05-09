using waterb.Utility;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
    public static bool TryGetBipartitePartition<TNode, TData>(this IGraph<TNode, TData> graph) where TNode : notnull
    {
        HashSet<TNode>? visited = [];
        Stack<DFSEnumerator<TNode, TData>.DFSNode>? stack = [];
        return graph.TryGetBipartitePartition(ref visited, ref stack, out _, out _);
    }
    
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
            partA = [];
            partB = [];
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

        partA = [];
        partB = [];
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
            for (var i = 0; i < component.Count; i++) adjacency[i] = new bool[component.Count];
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
    
    private readonly struct Segment
    {
        public IndexedSet<int> Nodes { get; init; }
        public IndexedSet<int> Contacts { get; init; }
    }
    
    // non-oriented graph only
    public static bool CheckPlanarGamma<TNode, TData>(this IGraph<TNode, TData> graph) where TNode : notnull
    {
        if (graph.Size < 5) return true;
        var components = graph.FindConnectedComponents();
        if (components == null) return true;
        for (var componentIndex = 0; componentIndex < components.Count; componentIndex++)
        {
            var component = components[componentIndex];
            var adjacency = new bool[component.Count][];
            for (var i = 0; i < component.Count; i++) adjacency[i] = new bool[component.Count];
            for (var i = 0; i < component.Count; i++)
            {
                for (var j = i + 1; j < component.Count; j++)
                {
                    adjacency[i][j] = adjacency[j][i] =
                        graph[component[i]][graph.GetIndex(component[j])!.Value].HasValue ||
                        graph[component[j]][graph.GetIndex(component[i])!.Value].HasValue;
                }
            }
            
            if (!IsComponentPlanar(adjacency, component.Count)) return false;
        }

        return true;
        
        static bool IsComponentPlanar(bool[][] adjacency, int componentSize)
        {
            if (componentSize <= 2) return true;

            var parent = new int[componentSize];
            var visited = new bool[componentSize];
            IndexedSet<int>? reverseSimpleCycle = null;

            parent[0] = -1;
            if (!FindReverseCycle(0, -1, componentSize, adjacency, parent, visited, ref reverseSimpleCycle) ||
                reverseSimpleCycle == null || reverseSimpleCycle.Count < 3)
            {
                return true;
            }
            
            for (var i = 0; i < reverseSimpleCycle.Count; i++)
            {
                var a = reverseSimpleCycle[reverseSimpleCycle.Count - i - 1];
                var b = reverseSimpleCycle[(reverseSimpleCycle.Count - i) % reverseSimpleCycle.Count];
                adjacency[a][b] = adjacency[b][a] = false;
            }
            
            Array.Fill(visited, false);
            var segments = new List<Segment>();
            var segmentStack = new Stack<int>();
            
            for (var i = 0; i < componentSize; i++)
            {
                if (visited[i]) continue;
                var segmentComp = new IndexedSet<int>();
                var contacts = new IndexedSet<int>();
                segmentStack.Clear();
                segmentStack.Push(i);
                
                visited[i] = true;
                while (segmentStack.Count > 0)
                {
                    var currentIndex = segmentStack.Pop();
                    segmentComp.Add(currentIndex);
                    for (var adjIndex = 0; adjIndex < componentSize; adjIndex++)
                    {
                        if (!visited[adjIndex] && adjacency[currentIndex][adjIndex])
                        {
                            visited[adjIndex] = true;
                            segmentStack.Push(adjIndex);
                        }
                    }
                }

                for (var j = 0; j < segmentComp.Count; j++)
                {
                    var currentIndex = segmentComp[j];
                    if (reverseSimpleCycle.Contains(currentIndex)) contacts.Add(currentIndex);
                }
                
                if (contacts.Count > 1) segments.Add(new Segment
                {
                    Nodes = segmentComp,
                    Contacts = contacts
                });
            }

            if (segments.Count == 0) return true;
            var conflict = new List<int>[segments.Count];
            for (var i = 0; i < segments.Count; i++) conflict[i] = [];
            for (var i = 0; i < segments.Count; i++)
            {
                for (var j = i + 1; j < segments.Count; j++)
                {
                    if (IsConflict(segments[i], segments[j], reverseSimpleCycle))
                    {
                        conflict[i].Add(j);
                        conflict[j].Add(i);
                    }
                }
            }

            var color = new int[segments.Count];
            Array.Fill(color, -1);
            var queue = new Queue<int>();
            for (var i = 0; i < segments.Count; i++)
            {
                if (color[i] != -1) continue;
                
                color[i] = 0;
                queue.Enqueue(i);
                while (queue.Count > 0)
                {
                    var currentIndex = queue.Dequeue();
                    for (var j = 0; j < conflict[currentIndex].Count; j++)
                    {
                        var adjIndex = conflict[currentIndex][j];
                        if (color[adjIndex] == -1)
                        {
                            color[adjIndex] = 1 - color[currentIndex];
                            queue.Enqueue(adjIndex);
                        }
                        else if (color[adjIndex] == color[currentIndex]) return false;
                    }
                }
            }

            for (var segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                var segment = segments[segmentIndex];
                var subAdj = new bool[segment.Nodes.Count][];
                for (var i = 0; i < segment.Nodes.Count; i++) subAdj[i] = new bool[segment.Nodes.Count];
                for (var i = 0; i < segment.Nodes.Count; i++)
                {
                    for (var j = i + 1; j < segment.Nodes.Count; j++)
                    {
                        subAdj[i][j] = subAdj[j][i] = adjacency[segment.Nodes[i]][segment.Nodes[j]];
                    }
                }

                if (!IsComponentPlanar(subAdj, segment.Nodes.Count)) return false;
            }

            return true;
            static bool FindReverseCycle(int currentIndex, int parentIndex, int componentSize,
                bool[][] adjacency, int[] parent, bool[] visited, ref IndexedSet<int>? reverseSimpleCycle)
            {
                if (reverseSimpleCycle != null) return true;
                visited[currentIndex] = true;
                for (var adjIndex = 0; adjIndex < componentSize; adjIndex++)
                {
                    if (!adjacency[currentIndex][adjIndex]) continue;
                    if (!visited[adjIndex])
                    {
                        parent[adjIndex] = currentIndex;
                        if (FindReverseCycle(adjIndex, currentIndex, componentSize,
                            adjacency, parent, visited, ref reverseSimpleCycle)) return true;
                    }
                    else if (adjIndex != parentIndex && reverseSimpleCycle == null)
                    {
                        var temp = currentIndex;
                        reverseSimpleCycle = [ adjIndex ];
                        while (temp != adjIndex)
                        {
                            reverseSimpleCycle.Add(temp);
                            temp = parent[temp];
                        }
                        
                        return true;
                    }
                }

                return false;
            }

            static bool IsConflict(Segment segmentA, Segment segmentB, IndexedSet<int> reverseSimpleCycle)
            {
                var comp = 0;
                var compLength = 0;
                for (var i = 0; i < reverseSimpleCycle.Count && compLength < 4; i++)
                {
                    var currentIndex = reverseSimpleCycle[i];
                    var currentBit = GetBit(comp, compLength);
                    if (segmentA.Contacts.Contains(currentIndex) && (currentBit != 0 || compLength == 0))
                    {
                        comp &= ~(1 << compLength++);
                    }
                    else if (segmentB.Contacts.Contains(currentIndex) && (currentBit != 1 || compLength == 0))
                    {
                        comp |= 1 << compLength++;
                    }
                }
                
                for (var i = 0; i + 3 < compLength; i++)
                {
                    if (GetBit(comp, i) != GetBit(comp, i + 1) &&
                        GetBit(comp, i) == GetBit(comp, i + 2) &&
                        GetBit(comp, i) != GetBit(comp, i + 3))
                        return true;

                }
                return false;
                static int GetBit(int value, int shift) => (value >> shift) & 1;
            }
        }
    }
}