using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework.Media;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class DungeonGenerator
    {
        //The basic description and draw components for each floor tile.
        private Draw[] baseFloorDraw { get; set; }
        private Description baseFloorDescription { get; set; }
        //The basic description and draw components for each wall tile.
        private Draw[] baseWallDraw { get; set; }
        private Description baseWallDescription { get; set; }
        public Random seed { get; set; }
        public List<Room> rooms = new List<Room>();
        public static Vector stairSpot = new Vector(Program.mapWidth / 2, Program.mapHeight / 2);
        public static Vector keySpot = new Vector(Program.mapWidth / 2, Program.mapHeight / 2);
        public static int patrolRouteCount;
        public List<Vector> towerTiles = new List<Vector>();
        public List<Tile> skyTiles = new List<Tile>();
        public bool staircaseUnlocked = false;

        public Draw[,] backgroundImprint = new Draw[Program.mapWidth, Program.mapHeight];
        public float baseRadius = 25.5f;
        /// <summary>
        /// The chance for some parts of the structure to be decayed when created.
        /// </summary>
        public int decayChance = 5;
        public DungeonGenerator(Draw[] baseFloorDraw, Description baseFloorDescription, Draw[] baseWallDraw, Description baseWallDescription, int width, Random seed = null)
        {
            this.baseFloorDraw = baseFloorDraw;
            this.baseFloorDescription = baseFloorDescription;
            this.baseWallDraw = baseWallDraw;
            this.baseWallDescription = baseWallDescription;

            if (seed != null)
            {
                this.seed = seed;
            }
            else
            {
                this.seed = new Random();
            }

            float radius = baseRadius;
            Vector center = new Vector(Program.mapWidth / 2, Program.mapHeight / 2);

            int top = (int)MathF.Ceiling(center.y - radius),
                bottom = (int)MathF.Floor(center.y + radius);

            for (int y = top; y <= bottom; y++)
            {
                int dy = y - center.y;
                float dx = MathF.Sqrt(radius * radius - dy * dy);
                int left = (int)MathF.Ceiling(center.x - dx),
                    right = (int)MathF.Floor(center.x + dx);

                for (int x = left; x <= right; x++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        backgroundImprint[x, y] = new Draw(Color.LightGray, Color.Black, (char)177);
                    }
                }
            }
        }
        public void GenerateTowerFloor()
        {
            ClearDungeon();

            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        CreateOpenAir(x, y);
                    }
                }
            }

            float radius = baseRadius - Program.floor * .5f;
            Vector center = new Vector(Program.mapWidth / 2, Program.mapHeight / 2);

            int top = (int)MathF.Ceiling(center.y - radius),
                bottom = (int)MathF.Floor(center.y + radius);

            for (int y = top; y <= bottom; y++)
            {
                int dy = y - center.y;
                float dx = MathF.Sqrt(radius * radius - dy * dy);
                int left = (int)MathF.Ceiling(center.x - dx),
                    right = (int)MathF.Floor(center.x + dx);

                for (int x = left; x <= right; x++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        CreateWall(x, y);
                        towerTiles.Add(new Vector(x, y));
                    }
                }
            }

            Room lastRoom = CreateFirstRoom(center.x - 3, center.y - 3, 6, 6);
            rooms.Add(lastRoom);

            int roomsToGenerate = 30;
            int minRoomSize = 5;
            int maxRoomSize = 11;

            int tryCount = 0;

            for (int i = 0; i < roomsToGenerate; i++)
            {
                int xSP = seed.Next(0, Program.mapWidth);
                int ySP = seed.Next(0, Program.mapHeight);
                int rW = seed.Next(minRoomSize, maxRoomSize);
                int rH = seed.Next(minRoomSize, maxRoomSize);

                if (!towerTiles.Contains(new Vector(xSP, ySP)) || !towerTiles.Contains(new Vector(xSP + rW, ySP)) ||
                    !towerTiles.Contains(new Vector(xSP, ySP + rH)) || !towerTiles.Contains(new Vector(xSP + rW, ySP + rH)) ||
                    !CheckIfHasSpace(xSP, ySP, xSP + rW - 1, ySP + rH - 1))
                {
                    tryCount++;

                    if (tryCount >= 1000)
                    {
                        break;
                    }

                    i--;
                    continue;
                }

                tryCount = 0;

                Room room;
                room = CreateRoom(xSP, ySP, rW, rH);

                Program.dungeonGenerator.rooms.Add(room);
                rooms.Add(room);
            }

            CreatePassages(rooms);

            CreateWindows();

            CreateOutsidePassages();

            CreateDoors();

            UpdateWalls();

            SpawnAltars();

            CreateStaircaseAndKey();

            CreatePatrolLocations();

            CreateImprint();

            foreach (Tile tile in Program.tiles)
            {
                if (tile != null && tile.terrainType == 3)
                {
                    tile.GetComponent<Visibility>().explored = true;
                }
            }
        }
        public void SpawnAltars()
        {
            int terminalsToSpawn = seed.Next(2, 5);

            for (int i = 0; i < terminalsToSpawn; i++)
            {
                Room room = rooms[seed.Next(rooms.Count)];
                Vector selectedPosition = new Vector(room.x, room.y);

                string addOn = string.Empty;
                Color color = Color.White;

                int chance = seed.Next(4);

                switch (chance)
                {
                    case 0:
                        {
                            addOn = "This rudimentary shrine is made of the bones and shells of unknown creatures. " +
                                "Bespeckled with blood it was recently the site of a sacrifice.";
                            color = Color.Red;
                            break;
                        }
                    case 1:
                        {
                            addOn = "This rudimentary shrine is made of the bones and shells of unknown creatures. " +
                                "It was made quickly and looks to not have had any sacrifices yet.";
                            color = Color.White;
                            break;
                        }
                    case 2:
                        {
                            addOn = "This gleaming shrine is made from purest gold. It shines with a divine aura.";
                            color = Color.Gold;
                            break;
                        }
                    case 3:
                        {
                            addOn = "This gleaming shrine is made from purest silver. It shines with a divine aura.";
                            color = Color.Silver;
                            break;
                        }
                }


                Program.tiles[selectedPosition.x, selectedPosition.y].GetComponent<Draw>().fColor = color;
                Program.tiles[selectedPosition.x, selectedPosition.y].GetComponent<Draw>().bColor = Color.Black;
                Program.tiles[selectedPosition.x, selectedPosition.y].GetComponent<Draw>().character = '_';
                Program.tiles[selectedPosition.x, selectedPosition.y].GetComponent<Description>().name = "Divine Altar";
                Program.tiles[selectedPosition.x, selectedPosition.y].GetComponent<Description>().description = 
                    $"A shred of hope in light in this foul place. {addOn}";
                Program.tiles[selectedPosition.x, selectedPosition.y].AddComponent(new Altar());
                Program.tiles[selectedPosition.x, selectedPosition.y].terrainType = 0;
                Program.tiles[selectedPosition.x, selectedPosition.y].GetComponent<Visibility>().opaque = true;
                Program.tiles[selectedPosition.x, selectedPosition.y].SetDelegates();
            }
        }
        public void OvergrowRandomRooms(List<Room> rooms, List<Vector> dungeonSection)
        {
            int roomsToFlood = (int)(rooms.Count / 1.5f);

            List<Room> overgrown = new List<Room>();

            for (int i = 0; i < roomsToFlood; i++)
            {
                Room room = null;
                bool choosingRoom = true;

                while (choosingRoom)
                {
                    room = rooms[seed.Next(0, rooms.Count)];

                    if (overgrown.Contains(room)) { continue; }
                    else { choosingRoom = false; }
                }

                overgrown.Add(room);

                ConcurrentQueue<Vector> checkList = new ConcurrentQueue<Vector>();
                HashSet<Vector> tempList = new HashSet<Vector>();
                checkList.Enqueue(new Vector(room.x, room.y));
                tempList.Add(new Vector(room.x, room.y));

                int strength;

                if (seed.Next(1, 101) > 75) { strength = (room.width + room.height) / 2; }
                else { strength = (room.width + room.height) / 8; }

                List<Vector> overgrownVectors = new List<Vector>();

                foreach (Tile tile in room.tiles)
                {
                    if (tile.terrainType == 0)
                    {
                        if (seed.Next(1, 101) < 25)
                        {
                            Vector vector = tile.GetComponent<Vector>();

                            string name = RandomTableManager.RetrieveRandomSpecified($"Overgrown-Tiles", 0, true);

                            if (name == "None")
                            {
                                overgrownVectors.Add(vector);
                                continue;
                            }

                            Tile entity = JsonDataManager.ReturnTile(name);

                            entity.GetComponent<Vector>().x = vector.x;
                            entity.GetComponent<Vector>().y = vector.y;

                            Program.tiles[vector.x, vector.y] = entity;

                            entity.GetComponent<SpawnDetails>()?.onSpawn?.Invoke(room);

                            overgrownVectors.Add(vector);
                        }
                    }
                }

                DijkstraMap.CreateMap(overgrownVectors, "Overgrown", strength * 2);

                int[,] map = DijkstraMap.maps["Overgrown"];

                for (int y = 0; y <= Program.mapHeight; y++)
                {
                    for (int x = 0; x <= Program.mapWidth; x++)
                    {
                        if (Math.CheckBounds(x, y) && map[x, y] < 1000)
                        {
                            for (int y2 = y - 1; y2 <= y + 1; y2++)
                            {
                                for (int x2 = x - 1; x2 <= x + 1; x2++)
                                {
                                    if (Program.tiles[x2, y2].terrainType == 0)
                                    {
                                        Program.tiles[x2, y2].GetComponent<Draw>().fColor =
                                            new Color(Color.Brown.R + map[x, y] * 5, Color.Brown.G +
                                            map[x, y] * 5, Color.Brown.B + map[x, y] * 5);
                                    }
                                    else if (Program.tiles[x2, y2].GetComponent<Draw>().character == '.')
                                    {
                                        if (seed.Next(1, 101) < 10)
                                        {
                                            string name = RandomTableManager.RetrieveRandomSpecified($"Overgrown-Tiles", 0, true);

                                            if (name != "None")
                                            {
                                                Tile entity = JsonDataManager.ReturnTile(name);

                                                entity.GetComponent<Vector>().x = x;
                                                entity.GetComponent<Vector>().y = y;

                                                Program.tiles[x, y] = entity;

                                                entity.GetComponent<SpawnDetails>()?.onSpawn?.Invoke(room);

                                                continue;
                                            }
                                        }

                                        Program.tiles[x2, y2].GetComponent<Draw>().fColor =
                                            new Color(Color.DarkOliveGreen.R + map[x, y] * 5, Color.DarkOliveGreen.G +
                                            map[x, y] * 5, Color.DarkOliveGreen.B + map[x, y] * 5);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void InfestRandomRooms(List<Room> rooms, List<Vector> dungeonSection)
        {
            int roomsToFlood = (int)(rooms.Count / 1.5f);

            List<Room> infested = new List<Room>();

            for (int i = 0; i < roomsToFlood; i++)
            {
                Room room = null;
                bool choosingRoom = true;

                while (choosingRoom)
                {
                    room = rooms[seed.Next(0, rooms.Count)];

                    if (infested.Contains(room)) { continue; }
                    else { choosingRoom = false; }
                }

                infested.Add(room);

                ConcurrentQueue<Vector> checkList = new ConcurrentQueue<Vector>();
                HashSet<Vector> tempList = new HashSet<Vector>();
                checkList.Enqueue(new Vector(room.x, room.y));
                tempList.Add(new Vector(room.x, room.y));

                int strength;

                if (seed.Next(1, 101) > 75) { strength = (room.width + room.height) / 2; }
                else { strength = (room.width + room.height) / 8; }

                List<Tile> tiles = new List<Tile>();
                List<Vector> infestedVectors = new List<Vector>();

                foreach (Tile tile in room.tiles)
                {
                    if (tile.terrainType == 0)
                    {
                        if (seed.Next(1, 101) < 25)
                        {
                            Vector vector = tile.GetComponent<Vector>();

                            CreateFloor(vector.x, vector.y);
                            infestedVectors.Add(vector);
                        }
                    }
                }

                DijkstraMap.CreateMap(infestedVectors, "Infested", strength);

                int[,] map = DijkstraMap.maps["Infested"];

                for (int y = 0; y <= Program.mapHeight; y++)
                {
                    for (int x = 0; x <= Program.mapWidth; x++)
                    {
                        if (Math.CheckBounds(x, y) && map[x, y] < 1000)
                        {
                            for (int y2 = y - 1; y2 <= y + 1; y2++)
                            {
                                for (int x2 = x - 1; x2 <= x + 1; x2++)
                                {
                                    if (Program.tiles[x2, y2].terrainType == 0)
                                    {
                                        Program.tiles[x2, y2].GetComponent<Draw>().fColor =
                                            new Color(Color.SandyBrown.R + map[x, y] * 5, Color.SandyBrown.G +
                                            map[x, y] * 5, Color.SandyBrown.B + map[x, y] * 5);
                                    }
                                    else if (Program.tiles[x2, y2].GetComponent<Draw>().character == '.')
                                    {
                                        Program.tiles[x2, y2].GetComponent<Draw>().fColor =
                                            new Color(Color.SaddleBrown.R + map[x, y] * 5, Color.SaddleBrown.G +
                                            map[x, y] * 5, Color.SaddleBrown.B + map[x, y] * 5);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void FloodRandomRooms(List<Room> rooms, List<Vector> dungeonSection)
        {
            int roomsToFlood = (int)(rooms.Count / 1.5f);

            List<Room> flooded = new List<Room>();

            for (int i = 0; i < roomsToFlood; i++)
            {
                Room room = null;
                bool choosingRoom = true;

                while (choosingRoom)
                {
                    room = rooms[seed.Next(0, rooms.Count)];

                    if (flooded.Contains(room)) { continue; }
                    else { choosingRoom = false; }
                }

                flooded.Add(room);

                ConcurrentQueue<Vector> checkList = new ConcurrentQueue<Vector>();
                HashSet<Vector> tempList = new HashSet<Vector>();
                checkList.Enqueue(new Vector(room.x, room.y));
                tempList.Add(new Vector(room.x, room.y));
                List<Vector> floodTiles = new List<Vector>();

                int strength;

                if (seed.Next(1, 101) > 75) { strength = (room.width + room.height) / 2; }
                else { strength = (room.width + room.height) / 8; }

                for (int o = 0; o < strength; o++)
                {
                    for (int v = 0; v < checkList.Count; v++)
                    {
                        checkList.TryDequeue(out Vector vector);
                        CheckNeighbors(floodTiles, tempList, checkList, vector.x, vector.y);
                    }
                    tempList.Clear();
                }

                foreach (Vector vector in floodTiles)
                {
                    CreateWater(vector.x, vector.y);
                }

                foreach (Room roomRef in rooms)
                {
                    foreach (Tile tile in roomRef.tiles)
                    {
                        if (tile.terrainType == 0)
                        {
                            Vector vector = tile.GetComponent<Vector>();
                            int waterCount = 0;

                            for (int y2 = vector.y - 1; y2 <= vector.y + 1; y2++)
                            {
                                for (int x2 = vector.x - 1; x2 <= vector.x + 1; x2++)
                                {
                                    if (Program.tiles[x2, y2].terrainType == 2) { waterCount++; }
                                }
                            }

                            if (waterCount >= 4)
                            {
                                CreateWater(vector.x, vector.y);
                            }
                        }
                    }
                }

                DijkstraMap.CreateMap(floodTiles, "Flood", 10);

                int[,] map = DijkstraMap.maps["Flood"];

                for (int y = 0; y <= Program.mapHeight; y++)
                {
                    for (int x = 0; x <= Program.mapWidth; x++)
                    {
                        if (Math.CheckBounds(x, y) && map[x, y] < 1000)
                        {
                            for (int y2 = y - 1; y2 <= y + 1; y2++)
                            {
                                for (int x2 = x - 1; x2 <= x + 1; x2++)
                                {
                                    if (Program.tiles[x2, y2].terrainType == 0)
                                    {
                                        Program.tiles[x2, y2].GetComponent<Draw>().fColor =
                                            new Color(Color.LightSlateGray.R + map[x, y] * 5, Color.LightSlateGray.G +
                                            map[x, y] * 5, Color.LightSlateGray.B + map[x, y] * 5);
                                    }
                                    else if (Program.tiles[x2, y2].GetComponent<Draw>().character == '.')
                                    {
                                        Program.tiles[x2, y2].GetComponent<Draw>().fColor =
                                            new Color(Color.DarkSlateGray.R + map[x, y] * 5, Color.DarkSlateGray.G +
                                            map[x, y] * 5, Color.DarkSlateGray.B + map[x, y] * 5);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void CheckNeighbors(List<Vector> check, HashSet<Vector> tempList, ConcurrentQueue<Vector> checkList, int x, int y)
        {
            for (int y2 = y - 1; y2 <= y + 1; y2++)
            {
                if (CheckBoundsAndWalls(x, y2))
                {
                    Vector vector = new Vector(x, y2);
                    if (!tempList.Contains(vector))
                    {
                        check.Add(vector);
                        tempList.Add(vector);
                        checkList.Enqueue(vector);
                    }
                }
                else { continue; }
            }
            for (int x2 = x - 1; x2 <= x + 1; x2++)
            {
                if (CheckBoundsAndWalls(x2, y))
                {
                    Vector vector = new Vector(x2, y);
                    if (!tempList.Contains(vector))
                    {
                        check.Add(vector);
                        tempList.Add(vector);
                        checkList.Enqueue(vector);
                    }
                }
                else { continue; }
            }
        }
        private static bool CheckBoundsAndWalls(int x, int y)
        {
            return x >= 1 && x <= Program.mapWidth - 1 && y >= 1 && y <= Program.mapHeight - 1 && Program.tiles[x, y].terrainType != 0;
        }
        public void CreateOutsidePassages()
        {
            List<Vector> checkList = new List<Vector>();
            List<Vector> checkedList = new List<Vector>();

            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapHeight; y++)
                {
                    if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType == 3)
                    {
                        for (int x2 = x - 1; x2 <= x + 1; x2++)
                        {
                            for (int y2 = y - 1; y2 <= y + 1; y2++)
                            {
                                if (Math.CheckBounds(x2, y2) && Program.tiles[x2, y2].terrainType == 1)
                                {
                                    checkList.Add(new Vector(x2, y2));
                                }
                            }
                        }
                    }
                }
            }

            foreach (Vector position in checkList)
            {
                if (checkedList.Contains(position))
                {
                    continue;
                }
                else
                {
                    Vector firstTile = position;
                    checkedList.Add(firstTile);

                    Vector secondTile = null;

                    if (seed.Next(0, 101) > 50)
                    {
                        foreach (Vector check in checkList)
                        {
                            if (checkedList.Contains(check))
                            {
                                continue;
                            }
                            else if (secondTile == null && !checkedList.Contains(check) && check != firstTile)
                            {
                                secondTile = check;
                            }
                            else if (check != firstTile && !checkedList.Contains(check) && Math.Distance(firstTile.x, firstTile.y, check.x, check.y) < Math.Distance(firstTile.x, firstTile.y, secondTile.x, secondTile.y))
                            {
                                secondTile = check;
                            }
                        }
                    }

                    if (secondTile != null)
                    {
                        checkedList.Add(secondTile);

                        List<Node> nodes = AStar.ReturnPath(firstTile, secondTile);
                        if (nodes != null && nodes.Count > 0)
                        {
                            foreach (Node node in nodes)
                            {
                                for (int x = node.position.x - 1; x <= node.position.x + 1; x++)
                                {
                                    if (Program.tiles[x, node.position.y] != null && Program.tiles[x, node.position.y].terrainType == 3)
                                    {
                                        if (seed.Next(0, 101) > decayChance)
                                        {
                                            CreateFloor(x, node.position.y, true);
                                        }
                                    }
                                }
                                for (int y = node.position.y - 1; y <= node.position.y + 1; y++)
                                {
                                    if (Program.tiles[node.position.x, y] != null && Program.tiles[node.position.x, y].terrainType == 3)
                                    {
                                        if (seed.Next(0, 101) > decayChance)
                                        {
                                            CreateFloor(node.position.x, y, true);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        double xDistance = Math.Distance(Program.mapWidth / 2, Program.mapWidth / 2, firstTile.x, firstTile.x);
                        double yDistance = Math.Distance(Program.mapHeight / 2, Program.mapHeight / 2, firstTile.y, firstTile.y);

                        if (xDistance > yDistance)
                        {
                            for (int x = firstTile.x - 2; x <= firstTile.x + 2; x++)
                            {
                                for (int y = firstTile.y - 4; y <= firstTile.y + 4; y++)
                                {
                                    if (checkList.Contains(new Vector(x, y)))
                                    {
                                        checkedList.Add(new Vector(x, y));
                                    }

                                    if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType == 3)
                                    {
                                        if (seed.Next(0, 101) > decayChance)
                                        {
                                            CreateFloor(x, y, true);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int x = firstTile.x - 4; x <= firstTile.x + 4; x++)
                            {
                                for (int y = firstTile.y - 2; y <= firstTile.y + 2; y++)
                                {
                                    if (checkList.Contains(new Vector(x, y)))
                                    {
                                        checkedList.Add(new Vector(x, y));
                                    }

                                    if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType == 3)
                                    {
                                        if (seed.Next(0, 101) > decayChance)
                                        {
                                            CreateFloor(x, y, true);
                                        }
                                    }
                                }
                            }
                        }

                        continue;
                    }
                }
            }
        }
        public void CreateWindows()
        {
            foreach (Room room in rooms)
            {
                bool spotFound = false;
                Vector tile = null;

                for (int y = room.y - 10; y <= room.y + 10; y++)
                {
                    if (Math.CheckBounds(room.x, y) && Program.tiles[room.x, y].terrainType == 3)
                    {
                        tile = new Vector(room.x, y);
                        spotFound = true;
                        break;
                    }
                }

                for (int x = room.x - 10; x <= room.x + 10; x++)
                {
                    if (Math.CheckBounds(x, room.y) && Program.tiles[x, room.y].terrainType == 3)
                    {
                        if (tile != null)
                        {
                            if (Math.Distance(x, room.y, room.x, room.y) < Math.Distance(x, room.y, tile.x, tile.y))
                            {
                                tile = new Vector(x, room.y);
                                spotFound = true;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            tile = new Vector(x, room.y);
                            spotFound = true;
                            break;
                        }
                    }
                }

                if (spotFound)
                {
                    CreateCorridor(tile.x, tile.y, room.x, room.y, true);
                }
            }
        }
        public void SmoothMap()
        {
            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        int walls = WallCount(x, y);
                        if (walls > 4)
                        {
                            CreateWall(x, y);
                        }
                        else if (walls < 4)
                        {
                            CreateFloor(x, y);
                        }
                    }
                }
            }
        }
        public static int WallCount(int sX, int sY)
        {
            int walls = 0;

            for (int x = sX - 1; x <= sX + 1; x++)
            {
                for (int y = sY - 1; y <= sY + 1; y++)
                {
                    if (x != sX || y != sY) { if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType == 0) { walls++; } }
                }
            }

            return walls;
        }
        public void SpawnEvents()
        {
            int totalEventsToSpawn = seed.Next(3, 6);

            List<Room> temp = new List<Room>();

            foreach (Room room in rooms)
            {
                if (totalEventsToSpawn > 0)
                {
                    totalEventsToSpawn--;
                    temp.Add(room);
                }
                else
                {
                    break;
                }
            }

            foreach (Room room in temp)
            {
                Event entity = JsonDataManager.ReturnEvent(RandomTableManager.RetrieveRandomEvent(0, true));
                entity.GetComponent<SpawnDetails>().Invoke(room);
            }
        }
        public void CreateImprint()
        {
            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapHeight; y++)
                {
                    if (Math.CheckBounds(x, y) && backgroundImprint[x, y] != null)
                    {
                        if (backgroundImprint[x, y].character == (char)247 || backgroundImprint[x, y].character == '~')
                        {
                            float offSet = 10f;
                            backgroundImprint[x, y] = new Draw(
                                new Color((int)(backgroundImprint[x, y].fColor.R - offSet), (int)(backgroundImprint[x, y].fColor.G - offSet), (int)(backgroundImprint[x, y].fColor.B - offSet)),
                                new Color((int)(backgroundImprint[x, y].bColor.R - offSet), (int)(backgroundImprint[x, y].bColor.G - offSet), (int)(backgroundImprint[x, y].bColor.B - offSet)),
                                backgroundImprint[x, y].character);
                        }
                        else
                        {
                            float offSet = 30f;
                            backgroundImprint[x, y] = new Draw(
                                new Color((int)(backgroundImprint[x, y].fColor.R - offSet), (int)(backgroundImprint[x, y].fColor.G - offSet), (int)(backgroundImprint[x, y].fColor.B - offSet)),
                                new Color((int)(backgroundImprint[x, y].bColor.R - offSet), (int)(backgroundImprint[x, y].bColor.G - offSet), (int)(backgroundImprint[x, y].bColor.B - offSet)),
                                backgroundImprint[x, y].character);
                        }
                    }
                }
            }

            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        if (Program.tiles[x, y].terrainType == 0 || Program.tiles[x, y].terrainType == 1)
                        {
                            backgroundImprint[x, y] = new Draw(Program.tiles[x, y].GetComponent<Draw>());
                            backgroundImprint[x, y].fColor = Color.LightGray;

                            if (backgroundImprint[x, y].character == '.')
                            {
                                backgroundImprint[x, y].character = (char)176;
                            }
                        }
                        else if (Program.tiles[x, y].terrainType == 3 && backgroundImprint[x, y] == null)
                        {
                            backgroundImprint[x, y] = new Draw(Program.tiles[x, y].GetComponent<Draw>());
                        }
                        else if (backgroundImprint[x, y] != null && Program.tiles[x, y].terrainType == 3)
                        {
                            Program.tiles[x, y].GetComponent<Draw>().character = backgroundImprint[x, y].character;
                            Program.tiles[x, y].GetComponent<Draw>().fColor = backgroundImprint[x, y].fColor;
                            Program.tiles[x, y].GetComponent<Draw>().bColor = backgroundImprint[x, y].bColor;
                        }
                    }
                }
            }
        }
        public void UpdateWalls()
        {
            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapWidth; y++)
                {
                    if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType == 0 && Math.CheckBounds(x, y + 1))
                    {
                        if (Program.tiles[x, y + 1].terrainType != 0 && Program.tiles[x, y + 1].GetComponent<Draw>().character != '+')
                        {
                            Program.tiles[x, y].GetComponent<Draw>().character = (char)220;
                        }
                        else
                        {
                            Program.tiles[x, y].GetComponent<Draw>().character = (char)177;
                        }
                    }
                }
            }
        }
        public void PopulateDungeonSection(List<Room> rooms, string theme, int section)
        {
            int totalActorsToSpawn = seed.Next(10, 15) + Program.floor;
            int totalItemsToSpawn = seed.Next(6, 10);
            int totalObstaclesToSpawn = seed.Next(50, 100);

            for (int i = 0; i < totalObstaclesToSpawn; i++)
            {
                Room room = rooms[seed.Next(rooms.Count)];

                List<Tile> viableTiles = new List<Tile>();

                foreach (Tile reference in room.tiles)
                {
                    if (reference != null && reference.terrainType != 0)
                    {
                        viableTiles.Add(reference);
                    }
                }

                if (viableTiles.Count <= 0) { continue; }

                Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                viableTiles.Remove(tile);
                Vector vector = tile.GetComponent<Vector>();

                string name = RandomTableManager.RetrieveRandomSpecified($"{theme}-Tiles", 0, true);

                if (name == "None")
                {
                    continue;
                }

                Tile entity = JsonDataManager.ReturnTile(name);

                entity.GetComponent<Vector>().x = vector.x;
                entity.GetComponent<Vector>().y = vector.y;

                Program.tiles[vector.x, vector.y] = entity;

                entity.GetComponent<SpawnDetails>()?.onSpawn?.Invoke(room);
            }

            for (int i = 0; i < totalActorsToSpawn; i++)
            {
                int tryCount = 0;

                Room room = rooms[seed.Next(rooms.Count)];

                string name = RandomTableManager.RetrieveRandomSpecified($"{theme}-Enemies", 0, true);

                if (name == "None")
                {
                    continue;
                }

                Entity entity = JsonDataManager.ReturnEntity(name);


                Movement movement = entity.GetComponent<Movement>();

                List<Tile> viableTiles = new List<Tile>();

                if (movement.moveTypes.Count != 0)
                {
                    foreach (Tile reference in room.tiles)
                    {
                        Vector v = reference.GetComponent<Vector>();
                        if (reference != null && Program.tiles[v.x, v.y].actor == null && movement.moveTypes.Contains(Program.tiles[v.x, v.y].terrainType))
                        {
                            viableTiles.Add(reference);
                        }
                    }
                }
                else
                {
                    foreach (Tile reference in room.tiles)
                    {
                        Vector v = reference.GetComponent<Vector>();
                        if (reference != null && Program.tiles[v.x, v.y].actor == null && Program.tiles[v.x, v.y].terrainType == 1)
                        {
                            viableTiles.Add(reference);
                        }
                    }
                }


                if (viableTiles.Count <= 0)
                {
                    tryCount++;
                    i--;

                    if (tryCount > 50)
                    {
                        break;
                    }

                    continue;
                }

                Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                viableTiles.Remove(tile);
                Vector vector = tile.GetComponent<Vector>();

                entity.GetComponent<Vector>().x = vector.x;
                entity.GetComponent<Vector>().y = vector.y;

                Program.tiles[vector.x, vector.y].actor = entity;
                TurnManager.AddActor(entity.GetComponent<TurnComponent>());
                Math.SetTransitions(entity);

                Math.ReturnAIController(entity).dungeonSection = section;

                entity.GetComponent<SpawnDetails>()?.onSpawn?.Invoke(room);
            }

            for (int i = 0; i < totalItemsToSpawn; i++)
            {
                Room room = rooms[seed.Next(rooms.Count)];

                List<Tile> viableTiles = new List<Tile>();

                foreach (Tile reference in room.tiles)
                {
                    Vector v = reference.GetComponent<Vector>();
                    if (reference != null && Program.tiles[v.x, v.y].terrainType != 0)
                    {
                        viableTiles.Add(reference);
                    }
                }

                if (viableTiles.Count <= 0) { continue; }

                Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                viableTiles.Remove(tile);
                Vector vector = tile.GetComponent<Vector>();


                string name = RandomTableManager.RetrieveRandomSpecified($"{theme}-Items", 0, true);

                if (name == "None")
                {
                    continue;
                }

                Entity entity = JsonDataManager.ReturnEntity(name);

                entity.GetComponent<Vector>().x = vector.x;
                entity.GetComponent<Vector>().y = vector.y;

                Program.tiles[vector.x, vector.y].item = entity;

                entity.GetComponent<SpawnDetails>()?.onSpawn?.Invoke(room);
            }
        }
        public void ClearDungeon()
        {
            for (int x = 0; x < Program.mapWidth; x++)
            {
                for (int y = 0; y < Program.mapWidth; y++)
                {
                    if (Math.CheckBounds(x, y) && Program.tiles[x, y] != null)
                    {
                        if (Program.tiles[x, y].actor != null && Program.tiles[x, y].actor != Program.player)
                        {
                            Program.tiles[x, y].actor.GetComponent<TurnComponent>().isAlive = false;
                            TurnManager.RemoveActor(Program.tiles[x, y].actor.GetComponent<TurnComponent>());
                        }
                    }
                }
            }

            rooms.Clear();
            towerTiles.Clear();
            skyTiles.Clear();

            staircaseUnlocked = false;
        }
        public void CreatePatrolLocations()
        {
            patrolRouteCount = 0;

            foreach (Room room in rooms)
            {
                List<Tile> viableTiles = new List<Tile>();
                foreach (Tile tile in room.tiles)
                {
                    if (tile != null && tile.terrainType != 0)
                    {
                        viableTiles.Add(tile);
                    }
                }

                patrolRouteCount++;
                DijkstraMap.CreateMap(new List<Vector>() { viableTiles[seed.Next(viableTiles.Count - 1)].GetComponent<Vector>() }, $"Patrol-{patrolRouteCount}");
            }
        }
        public void CreateDoors()
        {
            foreach (Room room in rooms)
            {
                for (int x = 0; x < room.width; x++)
                {
                    for (int y = 0; y < room.height; y++)
                    {
                        int _X = room.corner.x + x;
                        int _Y = room.corner.y + y;

                        if (x == 0 || y == 0 || x == room.width - 1 || y == room.height - 1)
                        {
                            if (Program.tiles[_X, _Y].terrainType == 1)
                            {
                                int wallCount = 0;
                                int floorCount = 0;

                                for (int x2 = x - 1; x2 <= x + 1; x2++)
                                {
                                    _X = room.corner.x + x2;
                                    _Y = room.corner.y + y;

                                    if (x2 != x)
                                    {
                                        if (Program.tiles[_X, _Y].terrainType == 0)
                                        {
                                            wallCount++;
                                        }
                                        else if (Program.tiles[_X, _Y].terrainType == 1)
                                        {
                                            floorCount++;
                                        }
                                    }
                                }

                                if (wallCount != 2 && floorCount != 2)
                                {
                                    continue;
                                }

                                for (int y2 = y - 1; y2 <= y + 1; y2++)
                                {
                                    _X = room.corner.x + x;
                                    _Y = room.corner.y + y2;

                                    if (y2 != y)
                                    {
                                        if (Program.tiles[_X, _Y].terrainType == 0)
                                        {
                                            wallCount++;
                                        }
                                        else if (Program.tiles[_X, _Y].terrainType == 1)
                                        {
                                            floorCount++;
                                        }
                                    }
                                }

                                if (wallCount == 2 && floorCount == 2)
                                {
                                    _X = room.corner.x + x;
                                    _Y = room.corner.y + y;
                                    PlaceDoor(new Vector(_X, _Y));
                                }
                            }
                        }
                    }
                }

            }
        }
        public void PlaceDoor(Vector placement)
        {
            Tile tile = CreateWall(placement.x, placement.y);
            tile.terrainType = 1;

            Color newBackColor = tile.GetComponent<Draw>().fColor;
            tile.GetComponent<Draw>().character = '+';
            tile.GetComponent<Draw>().fColor = Color.White;
            tile.GetComponent<Description>().name = "Door";
            tile.GetComponent<Description>().description = "A sturdy door made from worked stone.";

            tile.AddComponent(new DoorComponent());
            tile.AddComponent(new Trap());

            tile.SetDelegates();

            Program.tiles[placement.x, placement.y] = tile;
        }
        public void CreatePassages(List<Room> rooms)
        {
            bool creatingPassages = true;
            List<Room> checkList = rooms.ToList();

            if (checkList.Count == 0) return;

            HashSet<Room> completed = new HashSet<Room>();
            Room first = checkList.First();
            checkList.Remove(first);
            completed.Add(first);


            while (creatingPassages)
            {
                List<Room> currentCheck = new List<Room>();

                Room currentRef = null;
                Room secondCurrentRef = null;


                foreach (Room c in completed)
                {
                    if (secondCurrentRef == null)
                    {
                        secondCurrentRef = c;
                    }
                    if (currentRef == null)
                    {
                        currentRef = checkList.First();
                    }



                    if (Math.Distance(currentRef.x, currentRef.y, secondCurrentRef.x, secondCurrentRef.y) > Math.Distance(currentRef.x, currentRef.y, c.x, c.y))
                    {
                        secondCurrentRef = c;
                    }
                }

                if (currentRef != null)
                {
                    checkList.Remove(currentRef);
                    completed.Add(currentRef);
                    CreateCorridor(currentRef.x, currentRef.y, secondCurrentRef.x, secondCurrentRef.y, false);
                }

                if (checkList.Count == 0)
                {
                    creatingPassages = false;
                }

            }

            Room current = null;
            Room secondCurrent = null;

            foreach (Room c in completed)
            {
                bool valid = true;

                foreach (Room c2 in completed)
                {
                    if (AStar.ReturnPath(new Vector(c.x, c.y), new Vector(c2.x, c2.y)) != null)
                    {
                        continue;
                    }
                    else
                    {
                        valid = false;
                    }

                    if (secondCurrent == null)
                    {
                        secondCurrent = c2;
                    }
                    if (current == null)
                    {
                        current = c;
                    }

                    if (Math.Distance(current.x, current.y, c2.x, c2.y) > Math.Distance(c.x, c.y, c2.x, c2.y))
                    {
                        current = c;
                    }

                    if (Math.Distance(c.x, c.y, secondCurrent.x, secondCurrent.y) > Math.Distance(c.x, c.y, c2.x, c2.y))
                    {
                        secondCurrent = c2;
                    }
                }

                if (!valid)
                {
                    CreateCorridor(current.x, current.y, secondCurrent.x, secondCurrent.y, false);
                }
            }


            foreach (Room c in completed)
            {
                if (current == null)
                {
                    current = c;
                }

                if (Math.Distance(current.x, current.y, Program.dungeonGenerator.rooms[0].x, Program.dungeonGenerator.rooms[0].y) >
                    Math.Distance(c.x, c.y, Program.dungeonGenerator.rooms[0].x, Program.dungeonGenerator.rooms[0].y))
                {
                    current = c;
                }
            }

            CreateCorridor(current.x, current.y, Program.dungeonGenerator.rooms[0].x, Program.dungeonGenerator.rooms[0].y, false);
        }
        public void CreateStaircaseAndKey()
        {
            List<Room> viableRooms = new List<Room>();

            foreach (Room room in rooms)
            {
                if (Math.Distance(room.x, room.y, rooms[0].x, rooms[0].y) > 5 )
                {
                    viableRooms.Add(room);
                }
            }

            Room viableRoom = viableRooms[seed.Next(viableRooms.Count)];
            List<Vector> viableTiles = new List<Vector>();

            foreach (Tile tile in viableRoom.tiles)
            {
                if (tile.terrainType == 1 || tile.terrainType == 2)
                {
                    viableTiles.Add(tile.GetComponent<Vector>());
                }
            }

            stairSpot = viableTiles[seed.Next(viableTiles.Count)];
            viableRooms.Remove(viableRoom);

            Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().character = '<';
            Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().fColor = Color.White;
            Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Description>().name = "Staircase";
            Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Description>().description = "A long winding staircase into complete darkness.";

            viableTiles.Clear();
            viableRoom = viableRooms[seed.Next(viableRooms.Count)];
            foreach (Tile tile in viableRoom.tiles)
            {
                if (tile.terrainType == 1 || tile.terrainType == 2)
                {
                    viableTiles.Add(tile.GetComponent<Vector>());
                }
            }

            keySpot = viableTiles[seed.Next(viableTiles.Count)];

            Entity item = JsonDataManager.ReturnEntity("Ichor Key");
            item.GetComponent<Vector>().x = keySpot.x;
            item.GetComponent<Vector>().y = keySpot.y;

            Program.tiles[keySpot.x, keySpot.y].item = item;
        }
        public bool CreateCorridor(int startingX, int startingY, int finalX, int finalY, bool connectToSky)
        {
            List<Vector> vectors = new List<Vector>();

            int startX = System.Math.Min(startingX, finalX);
            int startY = System.Math.Min(startingY, finalY);
            int endX = System.Math.Max(startingX, finalX);
            int endY = System.Math.Max(startingY, finalY);

            int buildCount = 0;

            if (seed.Next(1) == 0)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (buildCount != 0 && Program.tiles[x, startingY].terrainType == 0)
                    {
                        //CreateWall(x, startingY - 1);
                        //CreateWall(x, startingY + 1);
                    }

                    vectors.Add(new Vector(x, startingY));

                    //CreateFloor(x, startingY);

                    buildCount++;
                }

                buildCount = 0;

                for (int y = startY; y < endY + 1; y++)
                {
                    if (buildCount != 0 && Program.tiles[finalX, y].terrainType == 0)
                    {
                        //CreateWall(finalX - 1, y);
                        //CreateWall(finalX + 1, y);
                    }

                    vectors.Add(new Vector(finalX, y));

                    //CreateFloor(finalX, y);

                    buildCount++;
                }
            }
            else
            {
                for (int y = startY; y < endY + 1; y++)
                {
                    if (buildCount != 0 && Program.tiles[startingX, y].terrainType == 0)
                    {
                        //CreateWall(startingX - 1, y);
                        //CreateWall(startingX + 1, y);
                    }

                    vectors.Add(new Vector(startingX, y));

                    //CreateFloor(startingX, y);

                    buildCount++;
                }

                buildCount = 0;

                for (int x = startX; x < endX; x++)
                {
                    if (buildCount != 0 && Program.tiles[x, finalY].terrainType == 0)
                    {
                        //CreateWall(x, finalY - 1);
                        //CreateWall(x, finalY + 1);
                    }

                    vectors.Add(new Vector(x, finalY));

                    //CreateFloor(x, finalY);

                    buildCount++;
                }
            }

            foreach (Vector vector in vectors)
            {
                for (int y = vector.y - 1; y <= vector.y + 1; y++)
                {
                    for (int x = vector.x - 1; x <= vector.x + 1; x++)
                    {
                        if (Program.tiles[x, y] != null && Program.tiles[x, y].terrainType == 3 && !connectToSky) return false;
                    }
                }
            }

            foreach (Vector vector in vectors)
            {
                if (Program.tiles[vector.x, vector.y].terrainType == 0)
                {
                    CreateFloor(vector.x, vector.y);
                }
            }


            return true;
        }
        public Room CreateRoom(int _x, int _y, int roomWidth, int roomHeight, bool circle = false)
        {
            Room room = new Room(roomWidth, roomHeight, new Vector(_x, _y));

            if (circle)
            {
                float radius = (roomWidth + roomHeight) / 4;

                int top = (int)MathF.Ceiling(room.y - radius),
                    bottom = (int)MathF.Floor(room.y + radius);

                for (int y = top; y <= bottom; y++)
                {
                    int dy = y - room.y;
                    float dx = MathF.Sqrt(radius * radius - dy * dy);
                    int left = (int)MathF.Ceiling(room.x - dx),
                        right = (int)MathF.Floor(room.x + dx);

                    for (int x = left; x <= right; x++)
                    {
                        if (CheckIfHasSpace(x, y, roomWidth, roomHeight))
                        {
                            Program.dungeonGenerator.CreateFloor(x, y);
                        }
                    }
                }

                for (int x = 0; x < roomWidth; x++)
                {
                    for (int y = 0; y < roomHeight; y++)
                    {
                        int _X = room.corner.x + x;
                        int _Y = room.corner.y + y;

                        room.tiles[x, y] = Program.tiles[_X, _Y];
                    }
                }
            }
            else
            {
                for (int x = 0; x < roomWidth; x++)
                {
                    for (int y = 0; y < roomHeight; y++)
                    {
                        int _X = _x + x;
                        int _Y = _y + y;

                        if (x <= 0 || y <= 0 || x >= room.width - 1 || y >= room.height - 1)
                        {
                            room.tiles[x, y] = CreateWall(_X, _Y);
                        }
                        else
                        {
                            room.tiles[x, y] = CreateFloor(_X, _Y);
                        }
                    }
                }
            }

            return room;
        }
        public Room CreateFirstRoom(int _x, int _y, int roomWidth, int roomHeight)
        {
            Room room = new Room(roomWidth, roomHeight, new Vector(_x, _y));

            float radius = (roomWidth + roomHeight) / 4;

            int top = (int)MathF.Ceiling(room.y - radius),
                bottom = (int)MathF.Floor(room.y + radius);

            for (int y = top; y <= bottom; y++)
            {
                int dy = y - room.y;
                float dx = MathF.Sqrt(radius * radius - dy * dy);
                int left = (int)MathF.Ceiling(room.x - dx),
                    right = (int)MathF.Floor(room.x + dx);

                for (int x = left; x <= right; x++)
                {
                    Program.dungeonGenerator.CreateFloor(x, y);
                }
            }

            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    int _X = room.corner.x + x;
                    int _Y = room.corner.y + y;

                    room.tiles[x, y] = Program.tiles[_X, _Y];
                }
            }
            return room;
        }
        public bool CheckIfHasSpace(int sX, int sY, int eX, int eY)
        {
            for (int y = sY - 1; y <= eY + 1; y++)
            {
                for (int x = sX - 1; x <= eX + 1; x++)
                {
                    if (x <= 2 || y <= 2 || x >= Program.mapWidth - 1 || y >= Program.mapHeight - 1) return false;
                    if (Program.tiles[x, y].terrainType != 0) return false;

                    for (int y2 = y - 1; y2 < y + 1; y2++)
                    {
                        for (int x2 = x - 1; x2 < x + 1; x2++)
                        {
                            if (!Math.CheckBounds(x2, y2)) return false;
                            if (!towerTiles.Contains(Program.tiles[x, y].GetComponent<Vector>()) || Program.tiles[x, y].terrainType != 0) return false;
                        }
                    }
                }
            }

            return true;
        }
        public void PlacePlayer()
        {
            if (rooms.Count == 0)
            {
                Program.tiles[Program.mapWidth / 2, Program.mapHeight / 2].actor = Program.player;
                Vector vector2 = Program.tiles[Program.mapWidth / 2, Program.mapHeight / 2].GetComponent<Vector>();

                Program.player.GetComponent<Vector>().x = vector2.x;
                Program.player.GetComponent<Vector>().y = vector2.y;

                ShadowcastFOV.ClearSight();
                ShadowcastFOV.Compute(Program.player.GetComponent<Vector>(), 10);
                Program.MoveCamera(new Vector(vector2.x, vector2.y));
            }
            else
            {
                Vector vector;

                if (Program.tiles[rooms[0].x, rooms[0].y].actor == null)
                {
                    vector = new Vector(rooms[0].x, rooms[0].y);
                }
                else
                {
                    List<Tile> viableTiles = new List<Tile>();

                    foreach (Tile tile in rooms[0].tiles)
                    {
                        if (tile != null && tile.terrainType == 1)
                        {
                            viableTiles.Add(tile);
                        }
                    }

                    Tile chosenTile = viableTiles[seed.Next(viableTiles.Count)];
                    chosenTile.actor = Program.player;
                    vector = chosenTile.GetComponent<Vector>();
                }

                Program.player.GetComponent<Vector>().x = vector.x;
                Program.player.GetComponent<Vector>().y = vector.y;
                Program.tiles[vector.x, vector.y].actor = Program.player;

                ShadowcastFOV.ClearSight();
                ShadowcastFOV.Compute(Program.player.GetComponent<Vector>(), 10);
                Program.MoveCamera(new Vector(vector.x, vector.y));
                AttributeManager.UpdateAttributes(Program.player);
            }
        }
        public Tile CreateOpenAir(int x, int y)
        {
            Tile tile = null;

            int t = seed.Next(4);

            Description description;

            if (Program.floor == 1)
            {
                description = new Description("Roiling Sea", "The furious ocean, an unending force that erodes all.");
            }
            else
            {
                description = new Description("Open Air", "The open air, you can see straight to the ocean.");
            }

            switch (t)
            {
                case 0:
                    {
                        tile = new Tile(x, y, Color.SkyBlue, Color.Black, '~', description.name, description.description, 3, false);
                        break;
                    }
                case 1:
                    {
                        tile = new Tile(x, y, Color.SkyBlue, Color.Black, (char)247, description.name, description.description, 3, false);
                        break;
                    }
                case 2:
                    {
                        tile = new Tile(x, y, Color.DarkBlue, Color.Black, '~', description.name, description.description, 3, false);
                        break;
                    }
                case 3:
                    {
                        tile = new Tile(x, y, Color.DarkBlue, Color.Black, (char)247, description.name, description.description, 3, false);
                        break;
                    }
            }

            Program.tiles[x, y] = tile;
            return tile;
        }
        public Tile CreateFloor(int x, int y, bool thick = false)
        {
            Draw draw = new Draw(Color.DarkRed, Color.Black, '.');
            if (thick)
            {
                draw.character = (char)176;
            }

            if (seed.Next(0, 101) < decayChance)
            {
                draw.fColor = Color.Chocolate;
            }

            Description description = baseFloorDescription;
            Tile tile = new Tile(x, y, draw.fColor, draw.bColor, draw.character, description.name, description.description, 1, false);
            Program.tiles[x, y] = tile;
            return tile;
        }
        public Tile CreateWall(int x, int y)
        {
            Draw draw = new Draw(Color.LightGray, Color.AnsiBlackBright, (char)177);
            Description description = baseWallDescription;

            if (seed.Next(0, 101) < decayChance)
            {
                draw.fColor = Color.Gray;
            }

            Tile tile;
            if (Math.CheckBounds(x, y + 1) && Program.tiles[x, y + 1] != null && Program.tiles[x, y + 1].terrainType != 0)
            {
                tile = new Tile(x, y, draw.fColor, draw.bColor, (char)220, description.name, description.description, 0, true);
            }
            else
            {
                tile = new Tile(x, y, draw.fColor, draw.bColor, draw.character, description.name, description.description, 0, true);
            }
            Program.tiles[x, y] = tile;
            return tile;
        }
        public Tile CreateWater(int x, int y)
        {
            Draw draw = new Draw(Color.Blue, Color.Black, '÷');
            Description description = new Description("Water", "A deep pool.");

            Tile tile;
            if (seed.Next(1, 101) < 25)
            {
                tile = new Tile(x, y, draw.fColor, draw.bColor, '~', description.name, description.description, 2, false);
            }
            else if (seed.Next(1, 101) < 25)
            {
                tile = new Tile(x, y, Color.DarkBlue, draw.bColor, '~', description.name, description.description, 2, false);
            }
            else if (seed.Next(1, 101) < 25)
            {
                tile = new Tile(x, y, Color.DarkBlue, draw.bColor, draw.character, description.name, description.description, 2, false);
            }
            else
            {
                tile = new Tile(x, y, draw.fColor, draw.bColor, draw.character, description.name, description.description, 2, false);
            }
            Program.tiles[x, y] = tile;
            return tile;
        }
    }
    public class Room
    {
        public int width { get; set; }
        public int height { get; set; }
        public Vector corner { get; set; }
        public int x { get { return corner.x + width / 2; } }
        public int y { get { return corner.y + height / 2; } }
        public Tile[,] tiles { get; set; }
        public int connectionCount = 0;
        public Room(int width, int height, Vector corner)
        {
            this.width = width;
            this.height = height;
            this.corner = corner;

            tiles = new Tile[width, height];
        }
    }
}
