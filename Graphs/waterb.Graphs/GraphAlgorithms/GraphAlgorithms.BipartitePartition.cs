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
    
    // non-oriented graph only (is not work correct, use Gamma check instead)
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

            var nodes = new List<int>(component.Nodes.Select(x => graph.GetIndex(x)!.Value));
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
                if (lowDegreeVertexIndex == -1) break;

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

            if (nodes.Count < 5) return true;
            var comb5 = new int[5];
            var coreIndices = new int[nodes.Count];
            for (var i = 0; i < nodes.Count; i++) coreIndices[i] = i;
            if (DFSK5(0, 0, comb5, coreIndices, adjacency, nodes.Count)) return false;
            
            if (nodes.Count < 6) return true;
            var leftComb3 = new int[3];
            var rightComb3 = new int[3];
            var rightInvertComb3 = new int[nodes.Count - 3];
            if (DFSK33Left(0, 0, leftComb3, rightComb3, rightInvertComb3, coreIndices, adjacency, nodes.Count)) return false;
            
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
            for (var i = 0; i < size; i++) if (Degree(adjacency, i, size) <= 2) return i;
            return -1;
        }

        static void RemoveVertex(bool[][] adjacency, List<int> nodes, int vertexIndex)
        {
            var size = nodes.Count;
            for (var i = vertexIndex; i < size - 1; i++)
                for (var j = 0; j < size; j++)
                    adjacency[i][j] = adjacency[i + 1][j];

            for (var j = vertexIndex; j < size - 1; j++)
                for (var i = 0; i < size - 1; i++)
                    adjacency[i][j] = adjacency[i][j + 1];

            nodes.RemoveAt(vertexIndex);
        }
        
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
    
    public readonly struct PartitionByDistanceData<TNode> where TNode : notnull
    {
        public double FromDistance { get; init; }
        public double ToDistance { get; init; }

        public List<TNode> Nodes { get; init; }
        public PartitionByDistanceData(double fromDistance, double toDistance)
        {
            FromDistance = fromDistance;
            ToDistance = toDistance;
            Nodes = [];
        }
    }
    
    public static List<PartitionByDistanceData<TNode>> PartitionByDistance<TNode, TData>(
        this IGraph<TNode,TData> graph, TNode start, int regionsCount) where TNode : notnull
    {
        var startIndex = graph.GetIndex(start) ??
            throw new ArgumentOutOfRangeException($"Node {start} is not found in graph!");
        if (regionsCount < 1) throw new ArgumentException("Can`t divide regions less than by 1 count.");
        
        var dist = new Dictionary<int, double>();
        var queue = new PriorityQueue<int, double>();
        var maxDist = double.NegativeInfinity;
        var minDist = double.PositiveInfinity;
        
        dist[startIndex] = 0;
        queue.Enqueue(startIndex, 0.0);
        while (queue.Count > 0)
        {
            var currentIndex = queue.Dequeue();
            var currentDistance = dist[currentIndex];
            
            for (var adjIndex = 0; adjIndex < graph.Size; adjIndex++)
            {
                if (!graph[currentIndex][adjIndex].HasValue || currentIndex == adjIndex) continue;
                
                var currDist = currentDistance + graph[currentIndex][adjIndex]!.Value;
                if (!dist.TryGetValue(adjIndex, out var adjDistance) || currDist < adjDistance)
                {
                    maxDist = Math.Max(currDist, maxDist);
                    minDist = Math.Min(currDist, minDist);
                    dist[adjIndex] = currDist;
                    queue.Enqueue(adjIndex, currDist);
                }
            }
        }

        var dataList = new List<PartitionByDistanceData<TNode>>();
        for (var i = 0; i < regionsCount; i++)
        {
            var prevBorder = (maxDist - minDist) * i / regionsCount;
            var currentBorder = (maxDist - minDist) * (i + 1) / regionsCount;
            var data = new PartitionByDistanceData<TNode>(prevBorder, currentBorder);
            dataList.Add(data);
        }

        for (var i = 0; i < graph.Size; i++)
        {
            if (!dist.TryGetValue(i, out var distance))
            {
                dataList[^1].Nodes.Add(graph.Nodes[i]);
            }
            else
            {
                for (var j = 0; j < regionsCount; j++)
                {
                    if (dataList[j].FromDistance <= distance && dataList[j].ToDistance >= distance)
                    {
                        dataList[j].Nodes.Add(graph.Nodes[i]);
                        break;
                    }
                }
            }
        }
        
        return dataList;
    }
}