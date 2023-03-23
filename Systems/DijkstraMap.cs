using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class DijkstraMap
    {
        public DijkstraMap()
        {
            baseIntArray = new int[Program.gameWidth, Program.gameHeight];

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    baseIntArray[x, y] = 1000;
                }
            }
        }
        private static int[,] baseIntArray { get; set; }
        public static Dictionary<string, int[,]> maps = new Dictionary<string, int[,]>();
        public static void CreateMap(List<Vector> coordinates, string name, int strength = 1000)
        {
            ConcurrentQueue<Vector> checkList = new ConcurrentQueue<Vector>();
            HashSet<Vector> tempList = new HashSet<Vector>();
            int[,] intArray = (int[,])baseIntArray.Clone();
            for (int o = 0; o < coordinates.Count; o++)
            {
                Vector vector = coordinates[o];
                intArray[vector.x, vector.y] = 0;
                checkList.Enqueue(vector);
                tempList.Add(vector);
            }

            for (int o = 0; o < strength; o++)
            {
                for (int i = 0; i < checkList.Count; i++)
                {
                    checkList.TryDequeue(out Vector vector);
                    CheckNeighbors(intArray, tempList, checkList, vector.x, vector.y);
                }
                tempList.Clear();
            }

            AddMap(intArray, name);
        }
        private static void CheckNeighbors(int[,] intArray, HashSet<Vector> tempList, ConcurrentQueue<Vector> checkList, int x, int y)
        {
            int current = intArray[x, y];
            for (int y2 = y - 1; y2 <= y + 1; y2++)
            {
                if (CheckBoundsAndWalls(x, y2) && intArray[x, y2] > current)
                {
                    Vector vector = new Vector(x, y2);
                    if (!tempList.Contains(vector))
                    {
                        intArray[x, y2] = current + 1;
                        tempList.Add(vector);
                        checkList.Enqueue(vector);
                    }
                }
                else { continue; }
            }
            for (int x2 = x - 1; x2 <= x + 1; x2++)
            {
                if (CheckBoundsAndWalls(x2, y) && intArray[x2, y] > current)
                {
                    Vector vector = new Vector(x2, y);
                    if (!tempList.Contains(vector))
                    {
                        intArray[x2, y] = current + 1;
                        tempList.Add(vector);
                        checkList.Enqueue(vector);
                    }
                }
                else { continue; }
            }
        }
        private static bool CheckBoundsAndWalls(int x, int y)
        {
            return x >= 1 && x <= Program.gameWidth - 1 && y >= 1 && y <= Program.gameHeight - 1 && Program.tiles[x, y].terrainType != 0;
        }
        private static void AddMap(int[,] map, string name)
        {
            if (maps.ContainsKey(name))
            {
                maps[name] = map;
            }
            else
            {
                maps.Add(name, map);
            }
        }
        public static void DiscardAll()
        {
            maps.Clear();
        }
        public static Vector PathFromMap(Entity entity, string mapName)
        {
            Vector start = entity.GetComponent<Vector>();
            int[,] map;
            if (maps.ContainsKey(mapName))
            {
                map = (int[,])maps[mapName].Clone();
            }
            else { return null; }

            Vector target = start;
            float v = map[start.x, start.y];

            for (int y = start.y - 1; y <= start.y + 1; y++)
            {
                for (int x = start.x - 1; x <= start.x + 1; x++)
                {
                    if (CheckBoundsAndWalls(x, y))
                    {
                        if (map[x, y] == 0 && Program.tiles[x, y].actor == null)
                        {
                            target = new Vector(x, y);
                            return target;
                        }
                        else
                        {
                            if (entity.GetComponent<Movement>().moveTypes.Contains(Program.tiles[x, y].terrainType) && Program.tiles[x, y].actor == null && map[x, y] < v)
                            {
                                target = new Vector(x, y);
                                v = map[x, y];
                            }
                            else { continue; }
                        }
                    }
                }
            }
            return target;
        }
    }
}
