using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SadConsole.SerializedTypes;
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
        public static Vector stairSpot = new Vector(Program.gameWidth / 2, Program.gameHeight / 2);
        public static int patrolRouteCount;
        public List<Tile> towerTiles = new List<Tile>();
        public List<Tile> skyTiles = new List<Tile>();
        public Draw[,] backgroundImprint = new Draw[Program.gameWidth, Program.gameHeight];
        public float baseRadius = 45.5f;
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
            Vector center = new Vector(Program.gameWidth / 2, Program.gameHeight / 2);

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

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        CreateOpenAir(x, y);
                    }
                }
            }

            float radius = baseRadius - (Program.floor * 2);
            Vector center = new Vector(Program.gameWidth / 2, Program.gameHeight / 2);

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
                        towerTiles.Add(CreateWall(x, y));
                    }
                }
            }

            int roomsToGenerate = 50;
            int minRoomSize = 5;
            int maxRoomSize = 14;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Room lastRoom = CreateRoom(center.x, center.y, 8, 8);
            rooms.Add(lastRoom);

            int tryCount = 0;

            for (int i = 0; i < roomsToGenerate - 1; i++)
            {
                int xSP = seed.Next(0, Program.gameWidth);
                int ySP = seed.Next(0, Program.gameHeight);
                int rW = seed.Next(minRoomSize, maxRoomSize);
                int rH = seed.Next(minRoomSize, maxRoomSize);

                if (!CheckIfHasSpace(xSP, ySP, xSP + rW - 1, ySP + rH - 1))
                {
                    tryCount++;

                    if (tryCount >= 250)
                    {
                        break;
                    }

                    i--;
                    continue;
                }

                tryCount = 0;

                Room room = CreateRoom(xSP, ySP, rW, rH);
                rooms.Add(room);

                lastRoom = room;
            }

            stopwatch.Stop();

            SpawnEvents();

            CreateWindows();

            CreateOutsidePassages();

            CreatePassages();

            CreateDoors();

            CreateStaircase();

            CreatePatrolLocations();

            UpdateWalls();

            //PopulateFloor();

            CreateImprint();

            foreach (Tile tile in Program.tiles)
            {
                if (tile != null && tile.terrainType == 3)
                {
                    tile.GetComponent<Visibility>().explored = true;
                }
            }
        }
        public void CreateOutsidePassages()
        {
            List<Vector> checkList = new List<Vector>();
            List<Vector> checkedList = new List<Vector>();

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
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
                                    if (Program.tiles[x, node.position.y].terrainType == 3)
                                    {
                                        if (seed.Next(0, 101) > decayChance)
                                        {
                                            CreateFloor(x, node.position.y, true);
                                        }
                                    }
                                }
                                for (int y = node.position.y - 1; y <= node.position.y + 1; y++)
                                {
                                    if (Program.tiles[node.position.x, y].terrainType == 3)
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
                        double xDistance = Math.Distance(Program.gameWidth / 2, Program.gameWidth / 2, firstTile.x, firstTile.x);
                        double yDistance = Math.Distance(Program.gameHeight / 2, Program.gameHeight / 2, firstTile.y, firstTile.y);

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
                    if (Program.tiles[room.x, y].terrainType == 3)
                    {
                        tile = new Vector(room.x, y);
                        spotFound = true;
                        break;
                    }
                }

                for (int x = room.x - 10; x <= room.x + 10; x++)
                {
                    if (Program.tiles[x, room.y].terrainType == 3)
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
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
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
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
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

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
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
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameWidth; y++)
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
        public void PopulateFloor()
        {
            int totalActorsToSpawn = seed.Next(15, 30) + Program.floor;
            int totalItemsToSpawn = seed.Next(6, 10);
            int totalObstaclesToSpawn = seed.Next(3, 6);

            foreach (Room room in rooms)
            {
                int actorsToSpawn = 0;
                int itemsToSpawn = 0;
                int obstaclesToSpawn = 0;

                if (totalActorsToSpawn > 0)
                {
                    totalActorsToSpawn--;
                    actorsToSpawn = 1;
                }

                if (totalItemsToSpawn > 0)
                {
                    totalItemsToSpawn--;
                    itemsToSpawn = 1;
                }

                if (totalObstaclesToSpawn > 0)
                {
                    totalObstaclesToSpawn--;
                    obstaclesToSpawn = 1;
                }

                List<Tile> viableTiles = new List<Tile>();
                
                foreach (Tile tile in room.tiles)
                {
                    if (tile != null && tile.terrainType != 0)
                    {
                        viableTiles.Add(tile);
                    }
                }

                if (viableTiles.Count <= 0) { continue; }

                for (int i = 0; i < actorsToSpawn; i++)
                {
                    Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                    viableTiles.Remove(tile);
                    Vector vector = tile.GetComponent<Vector>();

                    Entity entity = JsonDataManager.ReturnEntity(RandomTableManager.RetrieveRandomEnemy(0, true));

                    entity.GetComponent<Vector>().x = vector.x;
                    entity.GetComponent<Vector>().y = vector.y;

                    tile.actor = entity;
                    TurnManager.AddActor(entity.GetComponent<TurnComponent>());
                    Math.SetTransitions(entity);

                    if (entity.GetComponent<SpawnDetails>() != null)
                    {
                        entity.GetComponent<SpawnDetails>().onSpawn?.Invoke(room);
                    }
                }
                for (int i = 0; i < itemsToSpawn; i++)
                {
                    Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                    viableTiles.Remove(tile);
                    Vector vector = tile.GetComponent<Vector>();

                    Entity entity = JsonDataManager.ReturnEntity(RandomTableManager.RetrieveRandomItem(0, true));

                    entity.GetComponent<Vector>().x = vector.x;
                    entity.GetComponent<Vector>().y = vector.y;

                    tile.item = entity;

                    if (entity.GetComponent<SpawnDetails>() != null)
                    {
                        entity.GetComponent<SpawnDetails>().onSpawn?.Invoke(room);
                    }
                }
                for (int i = 0; i < obstaclesToSpawn; i++)
                {
                    Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                    viableTiles.Remove(tile);
                    Vector vector = tile.GetComponent<Vector>();

                    Tile entity = JsonDataManager.ReturnTile(RandomTableManager.RetrieveRandomTile(0, true));

                    entity.GetComponent<Vector>().x = vector.x;
                    entity.GetComponent<Vector>().y = vector.y;
                    Program.tiles[vector.x, vector.y] = entity;

                    if (entity.GetComponent<SpawnDetails>() != null)
                    {
                        entity.GetComponent<SpawnDetails>().onSpawn?.Invoke(room);
                    }
                }
            }
        }
        public void ClearDungeon()
        {
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameWidth; y++)
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

            tile.AddComponent(new DoorComponent());
            tile.AddComponent(new Trap());

            tile.SetDelegates();

            Program.tiles[placement.x, placement.y] = tile;
        }
        public void CreatePassages()
        {
            bool creatingPassages = true;
            List<Room> checkList = rooms.ToList();
            HashSet<Room> completed = new HashSet<Room>();
            Room first = checkList.First();
            checkList.Remove(first);
            completed.Add(first);

            while (creatingPassages) 
            {
                List<Room> currentCheck = new List<Room>();

                Room current = null;
                Room secondCurrent = null;

                foreach (Room r in checkList)
                {
                    foreach (Room c in completed)
                    {
                        if (secondCurrent == null)
                        {
                            secondCurrent = c;
                        }
                        if (current == null)
                        {
                            current = r;
                        }
                        
                        if (Math.Distance(current.x, current.y, c.x, c.y) > Math.Distance(r.x, r.y, c.x, c.y))
                        {
                            current = r;
                        }

                        if (Math.Distance(r.x, r.y, secondCurrent.x, secondCurrent.y) > Math.Distance(r.x, r.y, c.x, c.y))
                        {
                            secondCurrent = c;
                        }
                    }
                }

                if (current != null)
                {
                    checkList.Remove(current);
                    completed.Add(current);
                    CreateCorridor(current.x, current.y, secondCurrent.x, secondCurrent.y, false);
                }

                if (checkList.Count == 0) 
                {
                    return;
                }
            }
            /*
            for (int i = 0; i < 1; i++)
            {
                Room firstRoom = null;
                Room secondRoom = null;

                foreach (Room room in rooms)
                {
                    if (firstRoom == null) 
                    {
                        firstRoom = room;
                    }
                    else if (room.connectionCount < firstRoom.connectionCount)
                    {
                        firstRoom = room;
                    }
                }

                foreach (Room room in rooms)
                {
                    if (secondRoom == null && room != firstRoom)
                    {
                        secondRoom = room;
                    }
                    else if (room != firstRoom && secondRoom != null)
                    {
                        if (room.connectionCount <= secondRoom.connectionCount && Math.Distance(firstRoom.x, firstRoom.y, room.x, room.y) <
                        Math.Distance(firstRoom.x, firstRoom.y, secondRoom.x, secondRoom.y))
                        {
                            secondRoom = room;
                        }
                    }
                }

                if (firstRoom == secondRoom)
                {
                    i--;
                    continue;
                }
                else
                {
                    if (!CreateCorridor(firstRoom.x, firstRoom.y, secondRoom.x, secondRoom.y))
                    {
                        continue;
                    }
                    else
                    {
                        firstRoom.connectionCount++;
                    }
                    //secondRoom.connectionCount++;
                }
            }
            */
        }
        public void CreateStaircase()
        {
            if (rooms.Count != 0)
            {
                List<Tile> viableTiles = new List<Tile>();

                foreach (Tile tile in rooms[rooms.Count - 1].tiles)
                {
                    if (tile != null && tile.terrainType == 1)
                    {
                        Vector vector = tile.GetComponent<Vector>();
                        int x = vector.x - rooms[rooms.Count - 1].corner.x;
                        int y = vector.y - rooms[rooms.Count - 1].corner.y;
                        if (x > 0 && y > 0 && x < rooms[rooms.Count - 1].width - 1 && y < rooms[rooms.Count - 1].height - 1)
                        {
                            viableTiles.Add(tile);
                        }
                    }
                }
                Tile chosenTile = viableTiles[seed.Next(viableTiles.Count)];
                stairSpot = chosenTile.GetComponent<Vector>();
            }

            if (Program.floor == 10)
            {
                Entity goblet = JsonDataManager.ReturnEntity("The Goblet of Eternity");
                Program.tiles[stairSpot.x, stairSpot.y].item = goblet;
            }
            else if (Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().character != '<')
            {
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().character = '<';
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().fColor = Color.White;
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Description>().name = "Staircase";
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Description>().description = "A long winding staircase into complete darkness.";
            }
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
                        if (Program.tiles[x, y].terrainType == 3 && !connectToSky) return false;
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
        public Room CreateRoom(int _x, int _y, int roomWidth, int roomHeight)
        {
            Room room = new Room(roomWidth, roomHeight, new Vector(_x, _y));

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
            return room;
        }
        public bool CheckIfHasSpace(int sX, int sY, int eX, int eY)
        {
            for (int y = sY - 1; y <= eY + 1; y++)
            {
                for (int x = sX - 1; x <= eX + 1; x++)
                {
                    if (x <= 2 || y <= 2 || x >= Program.gameWidth - 1 || y >= Program.gameHeight - 1) return false;
                    if (Program.tiles[x, y].terrainType != 0) return false;
                    
                    for (int y2 = y - 1; y2 < y + 1; y2++)
                    {
                        for (int x2 = x - 1; x2 < x + 1; x2++)
                        {
                            if (!Math.CheckBounds(x2, y2)) return false;
                            if (!towerTiles.Contains(Program.tiles[x, y]) || Program.tiles[x, y].terrainType != 0) return false;
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
                Program.tiles[Program.gameWidth / 2, Program.gameHeight / 2].actor = Program.player;
                Vector vector2 = Program.tiles[Program.gameWidth / 2, Program.gameHeight / 2].GetComponent<Vector>();

                Program.player.GetComponent<Vector>().x = vector2.x;
                Program.player.GetComponent<Vector>().y = vector2.y;

                ShadowcastFOV.ClearSight();
                ShadowcastFOV.Compute(Program.player.GetComponent<Vector>(), 10);
                Program.MoveCamera(new Vector(vector2.x, vector2.y));
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
                Vector vector = chosenTile.GetComponent<Vector>();

                Program.player.GetComponent<Vector>().x = vector.x;
                Program.player.GetComponent<Vector>().y = vector.y;
                Program.tiles[vector.x, vector.y].actor = Program.player;

                ShadowcastFOV.ClearSight();
                ShadowcastFOV.Compute(Program.player.GetComponent<Vector>(), 10);
                Program.MoveCamera(new Vector(vector.x, vector.y));
            }
        }
        public Tile CreateOpenAir(int x, int y)
        {
            Tile tile = null;

            int t = seed.Next(4);

            switch (t)
            {
                case 0:
                    {
                        tile = new Tile(x, y, Color.SkyBlue, Color.Black, '~', "Open Air.", "The open air.", 3, false);
                        break;
                    }
                case 1:
                    {
                        tile = new Tile(x, y, Color.SkyBlue, Color.Black, (char)247, "Open Air.", "The open air.", 3, false);
                        break;
                    }
                case 2:
                    {
                        tile = new Tile(x, y, Color.DarkBlue, Color.Black, '~', "Open Air.", "The open air.", 3, false);
                        break;
                    }
                case 3:
                    {
                        tile = new Tile(x, y, Color.DarkBlue, Color.Black, (char)247, "Open Air.", "The open air.", 3, false);
                        break;
                    }
            }

            Program.tiles[x, y] = tile;
            return tile;
        }
        public Tile CreateFloor(int x, int y, bool thick = false)
        {
            Draw draw = new Draw(Color.Brown, Color.Black, '.');
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
            Draw draw = new Draw(Color.LightGray, Color.Black, (char)177);
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
    }
    public class Room
    {
        public int width { get; set; }
        public int height { get; set; }
        public Vector corner { get; set; }
        public int x { get { return corner.x + (width / 2); } }
        public int y { get { return corner.y + (height / 2); } }
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
