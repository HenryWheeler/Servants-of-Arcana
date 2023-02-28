using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static System.Collections.Specialized.BitVector32;
using Console = SadConsole.Console;

namespace Servants_of_Arcana
{
    public class Program
    {
        public static RootConsole rootConsole { get; set; }
        public static TitleConsole logConsole { get; set; }
        public static TitleConsole mapConsole { get; set; }
        public static TitleConsole playerConsole { get; set; }
        public static TitleConsole inventoryConsole { get; set; }
        public static TitleConsole lookConsole { get; set; }
        public static TitleConsole targetConsole { get; set; }
        public static TitleConsole interactionConsole { get; set; }
        public static Console titleConsole { get; set; }
        public static Entity player { get; set; }
        public static bool isGameActive { get; set; } = false;

        //The Size of the root console
        public static int screenWidth = 120;
        public static int screenHeight = 60;

        //The size of the map console
        public static int mapWidth = 80;
        public static int mapHeight = 45;

        public static int interactionWidth = 80;
        public static int interactionHeight = 45;

        //The size of the ingame map
        public static int gameWidth = 100;
        public static int gameHeight = 100;

        private static int messageWidth = 120;
        private static int messageHeight = 15;

        private static int rogueHeight = 45;
        private static int rogueWidth = 40;

        private static int inventoryWidth = 40;
        private static int inventoryHeight = 45;

        private static int targetWidth = 40;
        private static int targetHeight = 45;

        private static int lookWidth = 40;
        private static int lookHeight = 45;
        public static int offSetX { get; set; }
        public static int offSetY { get; set; }
        public static int minX { get; set; }
        public static int maxX { get; set; }
        public static int minY { get; set; }
        public static int maxY { get; set; }
        public static int depth { get; set; } = 0;

        public static Tile[,] tiles = new Tile[gameWidth, gameHeight];
        public static Entity[,] sfx = new Entity[gameWidth, gameHeight];
        public static Random random = new Random();
        private static void Main(string[] args)
        {
            Settings.WindowTitle = "Servants of Arcana";

            Game.Create(screenWidth, screenHeight, "fonts/ascii_6x6.font.json");
            Game.Instance.DefaultFontSize = IFont.Sizes.Two;
            Game.Instance.OnStart = Init;
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void Init()
        {
            Settings.ResizeMode = Settings.WindowResizeOptions.Scale;
            //Settings.f

            rootConsole = new RootConsole(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);

            mapConsole = new TitleConsole("< Map >", mapWidth, mapHeight) { Position = new Point(0, 0) };
            logConsole = new TitleConsole("< Message Log >", messageWidth, messageHeight) { Position = new Point(0, mapHeight) };
            playerConsole = new TitleConsole("< The Rogue @ >", rogueWidth, rogueHeight) { Position = new Point(mapWidth, 0) };
            inventoryConsole = new TitleConsole("< Inventory >", inventoryWidth, inventoryHeight) { Position = new Point(mapWidth, 0) };
            targetConsole = new TitleConsole("< Targeting >", targetWidth, targetHeight) { Position = new Point(mapWidth, 0) };
            lookConsole = new TitleConsole("< Looking >", lookWidth, lookHeight) { Position = new Point(mapWidth, 0) };
            interactionConsole = new TitleConsole("< Interaction >", interactionWidth, interactionHeight) { Position = new Point(interactionWidth - (screenWidth / 2), interactionHeight - (int)(screenHeight / 1.5f)) };
            titleConsole = new Console(screenWidth, screenHeight) { Position = new Point(0, 0) };

            rootConsole.Children.Add(mapConsole);
            rootConsole.Children.Add(logConsole);
            rootConsole.Children.Add(playerConsole);
            rootConsole.Children.Add(inventoryConsole);
            rootConsole.Children.Add(targetConsole);
            rootConsole.Children.Add(lookConsole);
            rootConsole.Children.Add(titleConsole);
            rootConsole.Children.Add(interactionConsole);

            Game.Instance.Keyboard.InitialRepeatDelay = .4f;
            Game.Instance.Keyboard.RepeatDelay = .05f;
            rootConsole.SadComponents.Add(new KeyboardComponent());

            Game.Instance.Screen = rootConsole;
            Game.Instance.Screen.IsFocused = true;
            rootConsole.Children.MoveToTop(logConsole);
            rootConsole.Children.MoveToTop(playerConsole);
            rootConsole.Children.MoveToBottom(interactionConsole);


            Game.Instance.Screen = rootConsole;

            // This is needed because we replaced the initial screen object with our own.
            Game.Instance.DestroyDefaultStartingConsole();

            CreateTitleScreen();
        }
        public static void CreateTitleScreen()
        {
            rootConsole.Children.MoveToTop(titleConsole);

            titleConsole.Fill(Color.Black, Color.Black);

            for (int i = 0; i < 27; i++)
            {
                Color color = new Color(Color.AntiqueWhite, + + (10 * (i + 1)));
                titleConsole.DrawBox(new Rectangle(i, i, titleConsole.Width - (i * 2), titleConsole.Height - (i * 2)),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(color, Color.Black)));
            }

            titleConsole.Print(28, 28, "Servants of Arcana", Color.AntiqueWhite, Color.Black);
            titleConsole.Print(28, 31, "New Game: N", Color.AntiqueWhite, Color.Black);
        }
        public static void UpdateNewPlayer()
        {
            player.GetComponent<Movement>().onMovement += MoveCamera;
            player.GetComponent<Movement>().onMovement += AttributeManager.UpdateAttributes;

            AttributeManager.UpdateAttributes(player);
        }
        public static void StartNewGame()
        {
            rootConsole.Children.MoveToBottom(titleConsole);

            JsonDataManager jsonDataManager = new JsonDataManager();
            RandomTableManager randomTableManager = new RandomTableManager();
            AStar aStar = new AStar();
            Log log = new Log();
            AttributeManager attributeManager = new AttributeManager();

            player = new Entity();
            player.AddComponent(new Vector(0, 0));
            player.AddComponent(new Draw(Color.White, Color.Black, '@'));
            player.AddComponent(new Description("You", "It's you."));
            //player.AddComponent(PronounReferences.pronounSets["Player"]);
            player.AddComponent(new Attributes(20, 1f, 1, 1, 10, 10, 10));
            player.AddComponent(new TurnComponent());
            player.AddComponent(new Movement(new List<int> { 1, 2 }));
            player.AddComponent(new InventoryComponent(5));
            //player.AddComponent(new Harmable());
            //player.AddComponent(new Faction("Player"));
            //player.AddComponent(new UpdateCameraOnMove());
            player.AddComponent(new PlayerController());

            GenerateNewFloor();

            UpdateNewPlayer();

            TurnManager.AddActor(player.GetComponent<TurnComponent>());
            player.GetComponent<TurnComponent>().StartTurn();


            Entity testItem = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Blade of Testing", "The magical blade of testing. It does testing things as testing blades tend to do."),
                new Equipable(true, "Weapon"),
            });

            JsonDataManager.SaveEntity(testItem, "Blade of Testing");
            JsonDataManager.SaveEntity(player, "Player");

            for (int i = 0; i < 5; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Blade of Testing"), player);
            }

            isGameActive = true;
        }
        public static void GenerateNewFloor(Random seed = null)
        {
            depth++;

            DungeonGenerator generator = new DungeonGenerator(null, null, null, null);

            switch (depth)
            {
                case 1:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.White, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 2:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.LightBlue, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 3:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.SteelBlue, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 4:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.Blue, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 5:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.BlueViolet, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 6:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.DarkViolet, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 7:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.Violet, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
                case 8:
                    {
                        generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.PaleVioletRed, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."), seed);
                        generator.GenerateDungeon();
                        break;
                    }
            }

            generator.PlacePlayer();
        }
        public static void MoveCamera(Entity entity)
        {
            Vector vector = entity.GetComponent<Vector>();

            minX = vector.x - mapWidth / 2;
            maxX = minX + mapWidth;
            minY = vector.y - mapHeight / 2;
            maxY = minY + mapHeight;

            offSetX = (minX + maxX) / 2;
            offSetY = (minY + maxY) / 2;
        }
        public static void MoveCamera(Vector vector)
        {
            minX = vector.x - mapWidth / 2;
            maxX = minX + mapWidth;
            minY = vector.y - mapHeight / 2;
            maxY = minY + mapHeight;

            offSetX = (minX + maxX) / 2;
            offSetY = (minY + maxY) / 2;
        }
        public static void DrawMap()
        {
            int y = 0;
            for (int ty = minY; ty < maxY; ty++)
            {
                int x = 0;
                for (int tx = minX; tx < maxX; tx++)
                {
                    if (Math.CheckBounds(tx, ty))
                    {
                        Tile tile = tiles[tx, ty];
                        Visibility visibility = tile.GetComponent<Visibility>();

                        if (sfx[tx, ty] != null) { sfx[tx, ty].GetComponent<Draw>().DrawToScreen(mapConsole, x, y); }
                        else if (!visibility.visible && !visibility.explored) { mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, (char)0)); }
                        else if (!visibility.visible && visibility.explored)
                        {
                            Draw draw = tile.GetComponent<Draw>();
                            mapConsole.SetCellAppearance(x, y, new ColoredGlyph(new Color(draw.fColor, .5f), Color.Black, draw.character));
                        }
                        else if (tile.actor != null) { tile.actor.GetComponent<Draw>().DrawToScreen(mapConsole, x, y); }
                        else if (tile.item != null) { tile.item.GetComponent<Draw>().DrawToScreen(mapConsole, x, y); }
                        else { tile.GetComponent<Draw>().DrawToScreen(mapConsole, x, y); }
                    }
                    else
                    {
                        mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, (char)0));
                    }
                    x++;
                }
                y++;
            }

            CreateConsoleBorder(mapConsole);

            mapConsole.IsDirty = true;
        }
        public static void ClearSFX()
        {
            for (int x = 0; x < gameWidth; x++)
            {
                for (int y = 0; y < gameHeight; y++)
                {
                    sfx[x, y] = null;
                }
            }
        }
        public static void CreateConsoleBorder(TitleConsole console, bool includeTitle = true)
        {
            int finalHeight = 0;
            for (int i = 0; i < 3; i++)
            {
                finalHeight++;

                Color color = new Color(Color.AntiqueWhite, + + (30 * (i + 1)));
                console.DrawBox(new Rectangle(i, i, console.Width - (i * 2), console.Height - (i * 2)),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(color, Color.Black)));
            }
            if (includeTitle)
            {
                console.Print(finalHeight, finalHeight, console.title.Align(HorizontalAlignment.Center, console.Width - (finalHeight * 2), (char)196), Color.AntiqueWhite, Color.Black);
            }
            console.IsDirty = true;
        }
    }
    public class RootConsole : Console
    {
        //public List<ParticleComponent> particles = new List<ParticleComponent>();
        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            /*
            if (particles.Count > 0)
            {
                World.ClearSFX();

                for (int i = 0; i < particles.Count; i++)
                {
                    ParticleComponent particle = particles[i];
                    particle?.Progress();
                }

                if (particles.Count == 0)
                {
                    World.ClearSFX();
                }

                //The_Ruins_of_Ipsus.Renderer.DrawMapToScreen();

                IsDirty = true;
            }
            */
        }
        public RootConsole(int _width, int _height)
            : base(_width, _height) { }
    }
    public class TitleConsole : Console
    {
        public string title { get; set; }
        public TitleConsole(string _title, int _width, int _height)
            : base(_width, _height)
        {
            title = _title;
            this.Fill(Color.Black, Color.Black, 176);

            Program.CreateConsoleBorder(this);
        }
    }
}