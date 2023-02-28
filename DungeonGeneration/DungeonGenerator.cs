using System;
using System.Collections.Generic;
using System.Linq;
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
        private Random seed { get; set; }
        private List<Room> rooms = new List<Room>();
        public DungeonGenerator(Draw[] baseFloorDraw, Description baseFloorDescription, Draw[] baseWallDraw, Description baseWallDescription, Random seed = null)
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
            int roomsToGenerate = 20;
            int minRoomSize = 7;
            int maxRoomSize = 14;

            SetWalls();

            Room lastRoom = CreateRoom(Program.gameWidth / 2, Program.gameHeight / 2, 8, 8);
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

                    if (tryCount >= 100)
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
            }

            //NOT CURRENTLY FUNCTIONAL
            CreateDoors();

            CreateStaircase();
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

            tile.AddComponent(new DoorComponent());

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
                chosenTile.GetComponent<Draw>().character = '>';
                chosenTile.GetComponent<Draw>().fColor = SadRogue.Primitives.Color.White;
                chosenTile.GetComponent<Description>().name = "Staircase";
                chosenTile.GetComponent<Description>().description = "A long winding staircase into complete darkness.";
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
        public void SetWalls()
        {
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameWidth; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        CreateWall(x, y);
                    }
                }
            }
        }
        public bool CheckIfHasSpace(int sX, int sY, int eX, int eY)
        {
            for (int y = sY - 1; y <= eY + 1; y++)
            {
                for (int x = sX - 1; x <= eX + 1; x++)
                {
                    if (x < 2 || y <= 2 || x >= Program.gameWidth - 1 || y >= Program.gameHeight - 1) return false;
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
            Program.DrawMap();
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
            Tile tile = new Tile(x, y, draw.fColor, draw.bColor, draw.character, description.name, description.description, 0, true);
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
