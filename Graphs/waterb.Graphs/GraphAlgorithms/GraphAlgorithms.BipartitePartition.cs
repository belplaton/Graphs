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
        public List<int> Nodes { get; init; }
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
            var adjacencySource = new bool[component.Count][];
            for (var i = 0; i < component.Count; i++) adjacencySource[i] = new bool[component.Count];
            for (var i = 0; i < component.Count; i++)
            {
                for (var j = i + 1; j < component.Count; j++)
                {
                    adjacencySource[i][j] = adjacencySource[j][i] =
                        graph[component[i]][graph.GetIndex(component[j])!.Value].HasValue ||
                        graph[component[j]][graph.GetIndex(component[i])!.Value].HasValue;
                }
            }

            var cycles = FindCycles(adjacencySource, component.Count);
            for (var cycleIndex = 0; cycleIndex < cycles.Count; cycleIndex++)
            {
                var cycle = cycles[cycleIndex];
                if (cycle.Count < 3) continue;

                var adjacencyCopy = new bool[component.Count][];
                for (var i = 0; i < component.Count; i++)
                {
                    adjacencyCopy[i] = new bool[component.Count];
                    Array.Copy(adjacencySource[i], adjacencyCopy[i], component.Count);
                }
                
                if (!IsComponentPlanar(adjacencyCopy, component.Count, cycle)) return false;
            }
        }
        
        return true;
        static List<List<int>> FindCycles(bool[][] adjacency, int componentSize)
        {
            var parent = new int[componentSize];
            var visited = new bool[componentSize];
            var cycles  = new List<List<int>>();

            Array.Fill(parent, -1);
            for (var startIndex = 0; startIndex < componentSize; startIndex++)
                if (!visited[startIndex]) DFSCycles(startIndex, adjacency, componentSize, parent, visited, cycles);

            return cycles;
            static void DFSCycles(int startIndex, bool[][] adjacency, int componentSize,
                int[] parent, bool[] visited, List<List<int>> cycles)
            {
                visited[startIndex] = true;
                for (var adjIndex = 0; adjIndex < componentSize; adjIndex++)
                {
                    if (!adjacency[startIndex][adjIndex]) continue;
                    if (!visited[adjIndex])
                    {
                        parent[adjIndex] = startIndex;
                        DFSCycles(adjIndex, adjacency, componentSize, parent, visited, cycles);
                    }
                    else if (adjIndex != parent[startIndex])
                    {
                        var cycle = new List<int> { adjIndex };
                        var x = startIndex;
                        while (x != -1)
                        {
                            if (x != adjIndex) cycle.Add(x);
                            else if (cycle.Count > 2)
                            {
                                cycles.Add(cycle);
                                break;
                            }
                            
                            x = parent[x];
                        }
                    }
                }
            }
        }
        
        static bool IsComponentPlanar(bool[][] adjacency, int componentSize, IReadOnlyList<int> simpleCycle)
        {
            if (componentSize < 3 || simpleCycle.Count < 3) return true;
            
            for (var i = 0; i < simpleCycle.Count; i++)
            {
                var a = simpleCycle[i];
                var b = simpleCycle[(i + 1) % simpleCycle.Count];
                adjacency[a][b] = adjacency[b][a] = false;
            }
            
            var onCycle  = new HashSet<int>(simpleCycle);
            var visited = new bool[componentSize];
            var segments = new List<Segment>();
            var segmentStack = new Stack<int>();
            var parent   = new int[componentSize];
            var usedSegmentEdge = new bool[componentSize][];
            for (var i = 0; i < componentSize; i++) usedSegmentEdge[i] = new bool[componentSize];
            for (var startIndex = 0; startIndex < componentSize; startIndex++)
            {
                if (!onCycle.Contains(startIndex)) continue;
                for (var uIndex = 0; uIndex < componentSize; uIndex++)
                {
                    if (!adjacency[startIndex][uIndex] || usedSegmentEdge[startIndex][uIndex]) continue;
                    if (onCycle.Contains(uIndex))
                    {
                        segments.Add(new Segment
                        {
                            Nodes    = [startIndex, uIndex],
                            Contacts = [startIndex, uIndex]
                        });
                        
                        usedSegmentEdge[startIndex][uIndex] = usedSegmentEdge[uIndex][startIndex] = true;
                        continue;
                    }
                    
                    segmentStack.Clear();
                    segmentStack.Push(uIndex);
                    Array.Fill(parent,-1);
                    Array.Fill(visited, false);
                    visited[uIndex] = true;
                    
                    var finishIndex = -1;
                    while (segmentStack.Count > 0 && finishIndex == -1)
                    {
                        var vIndex = segmentStack.Pop();
                        for (var wIndex = 0; wIndex < componentSize; wIndex++)
                        {
                            if (!adjacency[vIndex][wIndex] ||
                                usedSegmentEdge[vIndex][wIndex] ||
                                visited[wIndex]) continue;

                            parent[wIndex] = vIndex;
                            visited[wIndex] = true;
                            
                            if (onCycle.Contains(wIndex))
                            {
                                finishIndex = wIndex;
                                break;
                            }
                            
                            segmentStack.Push(wIndex);
                        }
                    }
                    
                    if (finishIndex == -1) continue;
                    var nodes = new List<int> { startIndex };
                    for (var x = parent[finishIndex]; x != -1; x = parent[x])
                    {
                        nodes.Add(x);
                        if (x == startIndex) break;
                    }
                    
                    nodes.Add(finishIndex);
                    var contacts = new IndexedSet<int> { startIndex, finishIndex };
                    segments.Add(new Segment
                    {
                        Nodes = nodes,
                        Contacts = contacts
                    });

                    var prevIndex = startIndex;
                    for (var i = 0; i < nodes.Count; i++)
                    {
                        usedSegmentEdge[prevIndex][nodes[i]] = usedSegmentEdge[nodes[i]][prevIndex] = true;
                        prevIndex = nodes[i];
                    }
                }
            }

            if (segments.Count == 0) return true;
            var conflict = new List<int>[segments.Count];
            for (var i = 0; i < segments.Count; i++) conflict[i] = [];
            for (var i = 0; i < segments.Count; i++)
            {
                for (var j = i + 1; j < segments.Count; j++)
                {
                    if (IsConflict(segments[i], segments[j], simpleCycle))
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
                var subAdjSrc = new bool[segment.Nodes.Count][];
                for (var i = 0; i < segment.Nodes.Count; i++) subAdjSrc[i] = new bool[segment.Nodes.Count];
                for (var i = 0; i < segment.Nodes.Count; i++)
                {
                    for (var j = i + 1; j < segment.Nodes.Count; j++)
                    {
                        subAdjSrc[i][j] = subAdjSrc[j][i] = adjacency[segment.Nodes[i]][segment.Nodes[j]];
                    }
                }
                
                var cycles = FindCycles(subAdjSrc, segment.Nodes.Count);
                for (var cycleIndex = 0; cycleIndex < cycles.Count; cycleIndex++)
                {
                    var cycle = cycles[cycleIndex];
                    if (cycle.Count < 3) continue;

                    var subAjdCopy = new bool[segment.Nodes.Count][];
                    for (var i = 0; i < segment.Nodes.Count; i++)
                    {
                        subAjdCopy[i] = new bool[segment.Nodes.Count];
                        Array.Copy(subAdjSrc[i], subAjdCopy[i], segment.Nodes.Count);
                    }

                    if (!IsComponentPlanar(subAjdCopy, segment.Nodes.Count, cycle)) return false;
                }
            }

            return true;
            static bool IsConflict(Segment segmentA, Segment segmentB, IReadOnlyList<int> cycle)
            {
                int a1 = -1, a2 = -1, b1 = -1, b2 = -1, pos = 0;
                for (var i = 0; i < cycle.Count; i++)
                {
                    var currentIndex = cycle[i];
                    if (segmentA.Contacts.Contains(currentIndex))
                    {
                        if (a1 == -1) a1 = pos;
                        else a2 = pos;
                    }
                    else if (segmentB.Contacts.Contains(currentIndex))
                    {
                        if (b1 == -1) b1 = pos;
                        else b2 = pos;
                    }

                    pos++;
                }
                
                if (a1 > a2) (a1, a2) = (a2, a1);
                if (b1 > b2) (b1, b2) = (b2, b1);

                if (a1 == -1 || b1 == -1) return false;
                var isConflict = (a1 < b1 && b1 < a2 && a2 < b2) || (b1 < a1 && a1 < b2 && b2 < a2);
                return isConflict;
            }
        }
    }
}