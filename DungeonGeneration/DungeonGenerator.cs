using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        private List<Room> rooms = new List<Room>();
        public static Vector stairSpot = new Vector(Program.gameWidth / 2, Program.gameHeight / 2);
        public static int patrolRouteCount;
        public string dungeonType = "Dungeon";
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
        }
        public void GenerateDungeon() 
        {
            ClearDungeon();

            int roomsToGenerate = 20;
            int minRoomSize = 5;
            int maxRoomSize = 14;

            CreateBaseMap();

            dungeonType = "Dungeon";

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Room lastRoom = CreateRoom(stairSpot.x, stairSpot.y, 8, 8);
            rooms.Add(lastRoom);

            for (int i = 0; i < roomsToGenerate - 1; i++)
            {
                int tryCount = 0;

                int xSP = seed.Next(0, Program.gameWidth);
                int ySP = seed.Next(0, Program.gameHeight);
                int rW = seed.Next(minRoomSize, maxRoomSize);
                int rH = seed.Next(minRoomSize, maxRoomSize);

                if (!CheckIfHasSpace(xSP, ySP, xSP + rW - 1, ySP + rH - 1) || Math.Distance(lastRoom.x, lastRoom.y, xSP, ySP) > 25)
                {
                    tryCount++;

                    if (tryCount >= 100 || stopwatch.Elapsed.Seconds > 4)
                    {
                        break;
                    }

                    i--;
                    continue;
                }

                Room room = CreateRoom(xSP, ySP, rW, rH);
                rooms.Add(room);
                CreateCorridor(lastRoom.x, lastRoom.y, room.x, room.y);
                lastRoom = room;

                if (stopwatch.Elapsed.Seconds > 4)
                {
                    break;
                }
            }

            stopwatch.Stop();

            CreateDoors();

            CreateStaircase();

            CreatePatrolLocations();

            UpdateWalls();

            PopulateFloor();
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
            string tableReturn = $"Depth-{Program.depth}-{dungeonType}";

            int totalActorsToSpawn = seed.Next(8, 15) + Program.depth;
            int totalItemsToSpawn = seed.Next(6, 10);
            int totalObstaclesToSpawn = seed.Next(10, 20);

            foreach (Room room in rooms)
            {
                int actorsToSpawn;
                int itemsToSpawn;
                int obstaclesToSpawn;

                if (rooms.Count > totalActorsToSpawn)
                {
                    actorsToSpawn = seed.Next(0, 2);
                }
                else
                {
                    actorsToSpawn = (int)MathF.Ceiling(totalActorsToSpawn / rooms.Count);
                }

                if (rooms.Count > totalItemsToSpawn)
                {
                    itemsToSpawn = seed.Next(0, 2);
                }
                else
                {
                    itemsToSpawn = (int)MathF.Ceiling(totalItemsToSpawn / rooms.Count);

                }

                if (rooms.Count > totalObstaclesToSpawn)
                {
                    obstaclesToSpawn = seed.Next(0, 2);
                }
                else
                {
                    obstaclesToSpawn = (int)MathF.Ceiling(totalObstaclesToSpawn / rooms.Count);

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

                    Entity entity = JsonDataManager.ReturnEntity(RandomTableManager.RetrieveRandom(tableReturn, 1, true));

                    entity.GetComponent<Vector>().x = vector.x;
                    entity.GetComponent<Vector>().y = vector.y;

                    tile.actor = entity;
                    TurnManager.AddActor(entity.GetComponent<TurnComponent>());
                    Math.SetTransitions(entity);
                }
                for (int i = 0; i < itemsToSpawn; i++)
                {
                    Tile tile = viableTiles[seed.Next(0, viableTiles.Count - 1)];
                    viableTiles.Remove(tile);
                    Vector vector = tile.GetComponent<Vector>();

                    Entity entity = JsonDataManager.ReturnEntity(RandomTableManager.RetrieveRandom($"Items", 1, true));

                    entity.GetComponent<Vector>().x = vector.x;
                    entity.GetComponent<Vector>().y = vector.y;

                    tile.item = entity;
                }
                for (int i = 0; i < obstaclesToSpawn; i++)
                {
                    //Add later
                }
            }
        }
        public void CreateBaseMap()
        {
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        CreateWall(x, y);
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

            if (Program.depth == 6)
            {
                Entity goblet = JsonDataManager.ReturnEntity("The Goblet of Eternity");
                Program.tiles[stairSpot.x, stairSpot.y].item = goblet;
            }
            else if (Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().character != '>')
            {
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().character = '>';
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Draw>().fColor = Color.White;
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Description>().name = "Staircase";
                Program.tiles[stairSpot.x, stairSpot.y].GetComponent<Description>().description = "A long winding staircase into complete darkness.";
            }
        }
        public bool CreateCorridor(int startingX, int startingY, int finalX, int finalY)
        {
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

                    CreateFloor(x, startingY);

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

                    CreateFloor(finalX, y);

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

                    CreateFloor(startingX, y);

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

                    CreateFloor(x, finalY);

                    buildCount++;
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
                }
            }
            return true;
        }
        public void PlacePlayer()
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

            ShadowcastFOV.ClearSight();
            ShadowcastFOV.Compute(Program.player.GetComponent<Vector>(), 10);
            Program.MoveCamera(new Vector(vector.x, vector.y));
            //Program.DrawMap();
        }
        public Tile CreateFloor(int x, int y)
        {
            Draw draw = baseFloorDraw[seed.Next(baseFloorDraw.Count())];
            Description description = baseFloorDescription;
            Tile tile = new Tile(x, y, draw.fColor, draw.bColor, draw.character, description.name, description.description, 1, false);
            Program.tiles[x, y] = tile;
            return tile;
        }
        public Tile CreateWall(int x, int y)
        {
            Draw draw = baseWallDraw[seed.Next(baseWallDraw.Count())];
            Description description = baseWallDescription;

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
        public bool northUsed = false;
        public bool eastUsed = false;
        public bool southUsed = false;
        public bool westUsed = false;
        public bool connected = false;
        public Room(int width, int height, Vector corner)
        {
            this.width = width;
            this.height = height;
            this.corner = corner;

            tiles = new Tile[width, height];
        }
    }
}
