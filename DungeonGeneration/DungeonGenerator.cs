using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                bool generating = true;
                while (generating)
                {
                    int xSP = seed.Next(0, Program.gameWidth);
                    int ySP = seed.Next(0, Program.gameHeight);
                    int rW = seed.Next(minRoomSize, maxRoomSize);
                    int rH = seed.Next(minRoomSize, maxRoomSize);

                    if (!CheckIfHasSpace(xSP, ySP, xSP + rW - 1, ySP + rH - 1) || Math.Distance(lastRoom.x, lastRoom.y, xSP, ySP) > 25) { continue; }

                    Room room = CreateRoom(xSP, ySP, rW, rH);
                    rooms.Add(room);
                    CreateCorridor(lastRoom.x, lastRoom.y, room.x, room.y);
                    lastRoom = room;

                    generating = false;
                }
            }

            CreateDoors();
        }
        public void CreateDoors()
        {
            foreach (Room room in rooms)
            {
                for (int y = 0; y < room.height; y++)
                {
                    for (int x = 0; x < room.width; x++)
                    {
                        if (x == 0 || y == 0 || x == room.width - 1 || y == room.height - 1)
                        {
                            if (room.tiles[x, y].terrainType == 1)
                            {
                                int wallCount = 0;
                                for (int x2 = x - 1; x2 < x + 1; x2++)
                                {
                                    for (int y2 = y - 1; y2 < y + 1; y2++)
                                    {
                                        if (x > 0 && y > 0 && x < room.width && y < room.height && room.tiles[x, y].terrainType == 0)
                                        {
                                            wallCount++;
                                        }
                                    }
                                }

                                if (wallCount == 2)
                                {
                                    //Placeholder until I make doors again.
                                    room.tiles[x, y].GetComponent<Draw>().character = '+';
                                }
                            }
                        }
                    }
                }
            }
        }
        public void CreateCorridor(int startingX, int startingY, int finalX, int finalY)
        {
            int startX = System.Math.Min(startingX, finalX);
            int startY = System.Math.Min(startingY, finalY);
            int endX = System.Math.Max(startingX, finalX);
            int endY = System.Math.Max(startingY, finalY);

            if (seed.Next(1) == 0)
            {
                for (int x = startX; x < endX; x++)
                {
                    CreateFloor(x, startingY);
                }
                for (int y = startY; y < endY + 1; y++)
                {
                    CreateFloor(finalX, y);
                }
            }
            else
            {
                for (int y = startY; y < endY + 1; y++)
                {
                    CreateFloor(startingX, y);
                }
                for (int x = startX; x < endX; x++)
                {
                    CreateFloor(x, finalY);

                }
            }
        }
        public Room CreateRoom(int _x, int _y, int roomWidth, int roomHeight)
        {
            Room room = new Room(roomWidth, roomHeight, new Vector(_x, _y), new Vector(_x, _y + roomHeight), new Vector(_x + roomWidth, _y), new Vector(_x + roomWidth, _y + roomHeight));

            for (int y = 0; y < roomHeight; y++)
            {
                for (int x = 0; x < roomWidth; x++)
                {
                    int _X = _x + x;
                    int _Y = _y + y;

                    if (x == 0 || y == 0 || x == roomWidth - 1 || y == roomHeight - 1)
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

            foreach (Tile tile in Program.tiles)
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
        public Vector bottomLeft { get; set; }
        public Vector topLeft { get; set; }
        public Vector bottomRight { get; set; }
        public Vector topRight { get; set; }
        public int x { get { return bottomLeft.x + (width / 2); } }
        public int y { get { return bottomLeft.y + (height / 2); } }
        public Tile[,] tiles { get; set; }
        public bool northUsed = false;
        public bool eastUsed = false;
        public bool southUsed = false;
        public bool westUsed = false;
        public bool connected = false;
        public Room(int width, int height, Vector bottomLeft, Vector topLeft, Vector bottomRight, Vector topRight)
        {
            this.width = width;
            this.height = height;
            this.bottomLeft = bottomLeft;
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
            this.topRight = topRight;

            tiles = new Tile[width, height];
        }
    }
}
