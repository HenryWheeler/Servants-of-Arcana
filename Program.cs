using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;
using Console = SadConsole.Console;

namespace Servants_of_Arcana
{
    public class Program
    {
        public static RootConsole rootConsole { get; set; }
        public static TitleConsole logConsole { get;set; }
        public static TitleConsole mapConsole { get; set; }
        public static TitleConsole playerConsole { get; set; }
        public static TitleConsole inventoryConsole { get; set; }
        public static TitleConsole lookConsole { get; set; }
        public static TitleConsole targetConsole { get; set; }
        public static Entity player { get; set; }
        public static bool isGameActive { get; set; } = false;

        //The Size of the root console
        public static int screenWidth = 120;
        public static int screenHeight = 60;

        //The size of the map console
        public static int mapWidth = 80;
        public static int mapHeight = 45;

        //The size of the ingame map
        public static int gameWidth = 100;
        public static int gameHeight = 100;

        public static Tile[,] tiles = new Tile[gameWidth, gameHeight];

        private static int messageWidth = 120;
        private static int messageHeight = 15;

        private static int rogueHeight = 45;
        private static int rogueWidth = 40;

        private static int inventoryWidth = 45;
        private static int inventoryHeight = 40;

        private static int targetWidth = 45;
        private static int targetHeight = 40;

        private static int lookWidth = 45;
        private static int lookHeight = 40;
        public static int offSetX { get; set; }
        public static int offSetY { get; set; }
        public static int minX { get; set; }
        public static int maxX { get; set; }
        public static int minY { get; set; }
        public static int maxY { get; set; }

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

            rootConsole = new RootConsole(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);

            mapConsole = new TitleConsole("< Map >", mapWidth, mapHeight) { Position = new Point(0, 0) };
            logConsole = new TitleConsole("< Message Log >", messageWidth, messageHeight) { Position = new Point(0, mapHeight) };
            playerConsole = new TitleConsole("< The Rogue @ >", rogueWidth, rogueHeight) { Position = new Point(mapWidth, 0) };
            inventoryConsole = new TitleConsole("< Inventory >", inventoryWidth, inventoryHeight) { Position = new Point(mapWidth, 0) };
            targetConsole = new TitleConsole("< Targeting >", targetWidth, targetHeight) { Position = new Point(mapWidth, 0) };
            lookConsole = new TitleConsole("< Looking >", lookWidth, lookHeight) { Position = new Point(mapWidth, 0) };

            rootConsole.Children.Add(mapConsole);
            rootConsole.Children.Add(logConsole);
            rootConsole.Children.Add(playerConsole);
            rootConsole.Children.Add(inventoryConsole);
            rootConsole.Children.Add(targetConsole);
            rootConsole.Children.Add(lookConsole);

            Game.Instance.Keyboard.InitialRepeatDelay = .4f;
            Game.Instance.Keyboard.RepeatDelay = .05f;
            rootConsole.SadComponents.Add(new KeyboardComponent());

            Game.Instance.Screen = rootConsole;
            Game.Instance.Screen.IsFocused = true;
            rootConsole.Children.MoveToTop(logConsole);
            rootConsole.Children.MoveToTop(playerConsole);


            Game.Instance.Screen = rootConsole;

            // This is needed because we replaced the initial screen object with our own.
            Game.Instance.DestroyDefaultStartingConsole();

            StartNewGame();
        }
        public static void StartNewGame()
        {
            Log log = new Log();

            DungeonGenerator generator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.White, Color.Black, '#') }, new Description("Stone Wall", "A simple stone wall."));
            generator.GenerateDungeon();


            player = new Entity();
            //player.AddComponent(new ID("Actor"));
            player.AddComponent(new Vector(0, 0));
            player.AddComponent(new Draw(Color.White, Color.Black, '@'));
            player.AddComponent(new Description("You", "It's you."));
            //player.AddComponent(PronounReferences.pronounSets["Player"]);
            player.AddComponent(new Attributes(2f));
            player.AddComponent(new TurnComponent());
            player.AddComponent(new Movement(new List<int> { 1, 2 }));
            //player.AddComponent(new Inventory());
            //player.AddComponent(new Harmable());
            //player.AddComponent(new Faction("Player"));
            //player.AddComponent(new UpdateCameraOnMove());
            player.AddComponent(new PlayerController());

            generator.PlacePlayer();

            TurnManager.AddActor(player.GetComponent<TurnComponent>());
            player.GetComponent<TurnComponent>().StartTurn();

            isGameActive = true;
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

                        //if (World.sfx[tx, ty] != null) { World.sfx[tx, ty].DrawToScreen(Program.mapConsole, x, y); }
                        if (!visibility.visible && !visibility.explored) { mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, '?')); }
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
                        mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, '?'));
                    }
                    x++;
                }
                y++;
            }

            CreateConsoleBorder(mapConsole);

            mapConsole.IsDirty = true;
        }
        public static void CreateConsoleBorder(TitleConsole console, bool includeTitle = true)
        {
            console.DrawBox(new Rectangle(0, 0, console.Width, console.Height),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));
            if (includeTitle)
            {
                console.Print(0, 0, console.title.Align(HorizontalAlignment.Center, console.Width, (char)196), Color.Black, Color.AntiqueWhite);
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