using System.Collections.Generic;
using System;
using TripleS.Physics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TripleS.Scripting {
    public static class NodegraphBuiler {
        public static List<NavNode> Nodes { get; private set; }

        public static void AddNode(Vector2 pos)
        {
            Nodes ??= new List<NavNode>();
            Nodes.Add(new NavNode(pos));
        }

        //yikes
        /// <summary>
        /// Builds Nodegraph by making node connections based on rays to find colliders.
        /// </summary>
        public static void CalculateGraph(Collider[] levelColliders)
        {
            if (Nodes != null && Nodes.Count != 0)
            {
                for (int mi = 0; mi < Nodes.Count; mi++)
                {
                    var mainNode = Nodes[mi];

                    for (int si = 0; si < Nodes.Count; si++)
                    {
                        if (si == mi)
                            continue;

                        var subNode = Nodes[si];
                        var dir = Vector2.Normalize(subNode.Location - mainNode.Location);
                        var dist = Vector2.Distance(mainNode.Location, subNode.Location);

                        if (dist > 16 * 32)
                            continue;

                        bool hit = false;
                        Ray2D ray = new Ray2D(mainNode.Location, dir);
                        foreach (Collider col in levelColliders)
                        {
                            if (col.Type == ColliderType.Face)
                            {
                                var inter = ray.Intersects(col.GetRectangle());
                                if (inter.HasValue)
                                {
                                    if (inter.Value < dist)
                                    {
                                        hit = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!hit)
                            mainNode.Connections.Add(si);
                    }
                }
            }
        }

        public static void ResetGraph()
        {
            Nodes = new List<NavNode>();
        }

        public static int GetClosestNode(Vector2 pos)
        {
            float bestDist = float.PositiveInfinity;
            int bestOfAll = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                float dis = Vector2.Distance(Nodes[i].Location, pos);
                if (dis < bestDist)
                {
                    bestDist = dis;
                    bestOfAll = i;
                }
            }
            return bestOfAll;
        }
    }

    public struct NavNode {
        public Vector2 Location { get; }
        public List<int> Connections { get; set; }

        public NavNode(Vector2 loc)
        {
            Location = loc;
            Connections = new List<int>();
        }
    }

    public class NodeController {
        public float NodeDistance { get; set; }
        private int NextNodeIndex { get; set; }
        public int NextNode { get; private set; }
        public bool ReachedLastNode { get; private set; }
        public bool StopRedos { get; set; }
        public Vector2 NextNodePos { get; private set; }
        public int[] nextNodes;
        private int oldTarget = -1;
        private float redoCounter;

        /// <summary>
        /// Finds node path by looping through connections and comparing distance to end point (closest node to player).
        /// </summary>
        public void UpdateConnections(Transform host, Vector2 playerCentre, float delta, bool small)
        {
            int playerNode = NodegraphBuiler.GetClosestNode(playerCentre);
            var pos = host.Centre;
            if (oldTarget != playerNode && MathF.Abs(host.Velocity.Y) < 0.1)
            {
                oldTarget = playerNode;
                CalculatePath(pos);
            }

            if (nextNodes != null && nextNodes.Length > 0)
            {
                if (NextNodeIndex < nextNodes.Length)
                {
                    NextNode = nextNodes[NextNodeIndex];
                    NextNodePos = NodegraphBuiler.Nodes[NextNode].Location;

                    var yp = NodegraphBuiler.Nodes[nextNodes[0]].Location.Y;
                    bool first = host.Centre.Y <= yp && nextNodes.Length > 1 && yp <= NodegraphBuiler.Nodes[nextNodes[1]].Location.Y;

                    float dis = Vector2.Distance(pos, NextNodePos);
                    bool yPass = !small || host.Position.Y > NextNodePos.Y - host.Height && pos.Y <= NextNodePos.Y;
                    if ((dis < NodeDistance && yPass) || (first && NextNodeIndex == 0))
                    {
                        NextNodeIndex++;
                        ReachedLastNode = NextNodeIndex == nextNodes.Length;
                    }

                    if (!StopRedos)
                    {
                        if (redoCounter > 0 && MathF.Abs(host.Velocity.X) <= 0.2f)
                            redoCounter -= delta;
                        else
                            redoCounter = 3;

                        if (redoCounter <= 0)
                            CalculatePath(pos);
                    }
                }
            }
        }

        private List<int> nodeBlacklist;

        /// <summary>
        /// Finds path to target (pos) using A* pathfinding. Finds a path to target, 
        /// and if unable to reach target (reach dead end or use too many nodes), try a different path.
        /// </summary>
        public void CalculatePath(Vector2 pos)
        {
            nodeBlacklist ??= new List<int>();

            int closest = NodegraphBuiler.GetClosestNode(pos);
            bool redo = false;

            ReachedLastNode = false;
            redoCounter = 0;
            NextNodeIndex = 0;
            NextNode = 0;
            var list = new List<int>();
            var lastNode = 0;
            for (int i = 0; i < NodegraphBuiler.Nodes.Count; i++)
            {
                if (i == 0)
                {
                    lastNode = closest;
                    list.Add(lastNode);
                    if (lastNode == oldTarget)
                    {
                        nodeBlacklist = new List<int>();
                        break;
                    }
                    continue;
                }

                float bestF = float.PositiveInfinity;
                foreach (int connect in NodegraphBuiler.Nodes[lastNode].Connections)
                {
                    if (list.Where(x => x == connect).Any() || nodeBlacklist.Where(x => x == connect).Any())
                        continue;

                    float h = Vector2.Distance(NodegraphBuiler.Nodes[oldTarget].Location, NodegraphBuiler.Nodes[connect].Location);
                    float g = Vector2.Distance(NodegraphBuiler.Nodes[closest].Location, NodegraphBuiler.Nodes[connect].Location);
                    float f = g + h;
                    if (f < bestF)
                    {
                        bestF = f;
                        lastNode = connect;
                    }
                }

                if (bestF == float.PositiveInfinity || list.Count > 60)
                {
                    nodeBlacklist.Add(lastNode);
                    redo = true;
                    break;
                }
                else
                {
                    list.Add(lastNode);
                    if (lastNode == oldTarget)
                    {
                        nodeBlacklist = new List<int>();
                        break;
                    }
                }
            }
            if (redo)
                CalculatePath(pos);
            else
                nextNodes = list.ToArray();
        }
    }
}