using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Servants_of_Arcana
{
    public class AStar
    {
        private readonly static List<Node> openSet = new List<Node>();
        private readonly static HashSet<Node> closedSet = new HashSet<Node>();
        private static Node[,] nodeSet { get; set; }
        public static List<Node> ReturnPath(Vector start, Vector end)
        {
            openSet.Clear();
            closedSet.Clear();

            Node[,] currentNodeSet = new Node[Program.gameWidth, Program.gameHeight];

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    currentNodeSet[x, y] = new Node(x, y);
                }
            }

            Node startNode = currentNodeSet[start.x, start.y];
            //startNode.gCost = 0;
            //startNode.hCost = 0;
            Node endNode = currentNodeSet[end.x, end.y];

            openSet.Add(startNode);

            while(openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) 
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode.position.x == endNode.position.x && currentNode.position.y == endNode.position.y)
                {
                    return RetracePath(startNode, endNode);
                }

                foreach (Node neighbor in ReturnNeighbors(currentNodeSet, currentNode))
                {
                    if (Program.tiles[neighbor.position.x, neighbor.position.y].terrainType == 0  || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    //Program.tiles[neighbor.position.x, neighbor.position.y].GetComponent<Draw>().fColor = new SadRogue.Primitives.Color(255 - GetDistance(startNode, neighbor), GetDistance(startNode, neighbor), 0);

                    int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbor);

                    if (newCostToNeighbour < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbour;
                        neighbor.hCost = GetDistance(neighbor, endNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            openSet.Remove(neighbor);
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }
        private static List<Node> ReturnNeighbors(Node[,] currentNodeSet, Node currentNode)
        {
            List<Node> neighbors = new List<Node>();
            for (int x = currentNode.position.x - 1; x <= currentNode.position.x + 1; x++)
            {
                for (int y = currentNode.position.y - 1; y <= currentNode.position.y + 1; y++)
                {
                    if (x == currentNode.position.x && y == currentNode.position.y)
                    {
                        continue;
                    } 
                    
                    if (x >= 0 && y >= 0 && x < Program.gameWidth && y < Program.gameHeight)
                    {
                        neighbors.Add(currentNodeSet[x, y]);
                    }
                }
            }

            return neighbors;
        }
        private static List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Add(currentNode);

            path.Reverse();

            return path;
        }
        private static int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = System.Math.Abs(nodeA.position.x - nodeB.position.x);
            int dstY = System.Math.Abs(nodeA.position.y - nodeB.position.y);

            if (dstX > dstY)
            {
                return 14 * dstY + 10 * (dstX - dstY);
            }
            else
            {
                return 14 * dstX + 10 * (dstY - dstX);
            }
        }
        public AStar()
        {
            nodeSet = new Node[Program.gameWidth, Program.gameHeight];

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    nodeSet[x, y] = new Node(x, y);
                }
            }
        }
    }
    public class Node
    {
        /// <summary>
        /// Distance from starting node
        /// </summary>
        public int gCost { get; set; } = 0;
        /// <summary>
        /// Distance from final node
        /// </summary>
        public int hCost { get; set; } = 0;
        /// <summary>
        /// gCost + hCost
        /// </summary>
        public int fCost { get { return gCost + hCost; } }
        public Node parent;
        public Vector position { get; set; }
        public Node(int x, int y) { position = new Vector(x, y); }
    }
}
