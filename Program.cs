using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadConsole.Instructions;
using SadRogue.Primitives;
using Servants_of_Arcana.Components;
using Servants_of_Arcana.Systems;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
        public static TitleConsole manualConsole { get; set; }
        public static TitleConsole loadingConsole { get; set; }
        public static Console titleConsole { get; set; }
        public static Entity player { get; set; }
        public static bool isGameActive { get; set; } = false;
        public static DungeonGenerator generator { get; set; }

        //The Size of the root console
        public static int screenWidth = 100;
        public static int screenHeight = 55;

        //The size of the map console
        public static int mapWidth = 70;
        public static int mapHeight = 40;

        public static int interactionWidth = 66;
        public static int interactionHeight = 42;

        //The size of the ingame map
        public static int gameWidth = 115;
        public static int gameHeight = 115;

        private static int messageWidth = 100;
        private static int messageHeight = 15;

        private static int rogueWidth = 30;
        private static int rogueHeight = 40;

        private static int inventoryWidth = 100;
        private static int inventoryHeight = 40;

        private static int targetWidth = 30;
        private static int targetHeight = 40;

        private static int lookWidth = 30;
        private static int lookHeight = 40;
        public static int offSetX { get; set; }
        public static int offSetY { get; set; }
        public static int minX { get; set; }
        public static int maxX { get; set; }
        public static int minY { get; set; }
        public static int maxY { get; set; }
        public static int floor { get; set; } = 0;

        public static Tile[,] tiles = new Tile[gameWidth, gameHeight];
        public static Particle[,] sfx = new Particle[gameWidth, gameHeight];
        public static Draw[,] uiSfx = new Draw[gameWidth, gameHeight];
        public static Random random = new Random();
        public static DungeonGenerator dungeonGenerator;
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

            SadConsole.Game.Instance.SetSplashScreens(new SadConsole.SplashScreens.Ansi1());

            rootConsole = new RootConsole(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);

            mapConsole = new TitleConsole("< Map >", mapWidth, mapHeight) { Position = new Point(0, 0) };
            logConsole = new TitleConsole("< Message Log >", messageWidth, messageHeight) { Position = new Point(0, mapHeight) };
            playerConsole = new TitleConsole("< The Rogue @ >", rogueWidth, rogueHeight) { Position = new Point(mapWidth, 0) };
            inventoryConsole = new TitleConsole("< Inventory >", inventoryWidth, inventoryHeight) { Position = new Point(0, 0) };
            targetConsole = new TitleConsole("< Targeting >", targetWidth, targetHeight) { Position = new Point(mapWidth, 0) };
            lookConsole = new TitleConsole("< Looking >", lookWidth, lookHeight) { Position = new Point(mapWidth, 0) };
            interactionConsole = new TitleConsole("< Interaction >", interactionWidth, interactionHeight) { Position = new Point(interactionWidth - ((screenWidth - 1) / 2), interactionHeight - (int)(screenHeight / 1.5f)) };
            manualConsole = new TitleConsole("< Manual >", screenWidth, screenHeight) { Position = new Point(0, 0) };
            loadingConsole = new TitleConsole("< Loading >", screenWidth, screenHeight) { Position = new Point(0, 0) };
            titleConsole = new Console(screenWidth, screenHeight) { Position = new Point(0, 0) };

            rootConsole.Children.Add(mapConsole);
            rootConsole.Children.Add(logConsole);
            rootConsole.Children.Add(playerConsole);
            rootConsole.Children.Add(inventoryConsole);
            rootConsole.Children.Add(targetConsole);
            rootConsole.Children.Add(lookConsole);
            rootConsole.Children.Add(titleConsole);
            rootConsole.Children.Add(interactionConsole);
            rootConsole.Children.Add(manualConsole);
            rootConsole.Children.Add(loadingConsole);

            Game.Instance.Keyboard.InitialRepeatDelay = .4f;
            Game.Instance.Keyboard.RepeatDelay = .05f;
            rootConsole.SadComponents.Add(new KeyboardComponent());

            Game.Instance.Screen = rootConsole;
            Game.Instance.Screen.IsFocused = true;
            rootConsole.Children.MoveToTop(logConsole);
            rootConsole.Children.MoveToTop(playerConsole);
            rootConsole.Children.MoveToBottom(interactionConsole);
            rootConsole.Children.MoveToBottom(inventoryConsole);
            rootConsole.Children.MoveToBottom(manualConsole);
            rootConsole.Children.MoveToBottom(loadingConsole);

            Game.Instance.Screen = rootConsole;

            // This is needed because we replaced the initial screen object with our own.
            Game.Instance.DestroyDefaultStartingConsole();

            CreateTitleScreen();
        }
        public static void CreateTitleScreen()
        {
            rootConsole.Children.MoveToTop(titleConsole);

            titleConsole.Fill(Color.Black, Color.Black);

            for (int i = 0; i < 24; i++)
            {
                Color color = new Color(Color.AntiqueWhite, + + (10 * (i + 1)));
                titleConsole.DrawBox(new Rectangle(i, i, titleConsole.Width - (i * 2), titleConsole.Height - (i * 2)),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(color, Color.Black)));
            }

            titleConsole.Print(25, 25, "Servants of Arcana - by Henry Wheeler", Color.AntiqueWhite, Color.Black);
            titleConsole.Print(25, 29, "New Game: N             Quit Game: Q", Color.AntiqueWhite, Color.Black);
        }
        public static void UpdateNewPlayer()
        {
            player.GetComponent<Movement>().onMovement += MoveCamera;
            player.GetComponent<Movement>().onMovement += AttributeManager.UpdateAttributes;

            player.GetComponent<Harmable>().onDeath += PlayerDeath;

            AttributeManager.UpdateAttributes(player);
            ShadowcastFOV.Compute(player.GetComponent<Vector>(), player.GetComponent<Attributes>().sight);

            player.SetDelegates();
        }
        public static void StartNewGame()
        {
            rootConsole.Children.MoveToBottom(titleConsole);

            JsonDataManager jsonDataManager = new JsonDataManager();
            RandomTableManager randomTableManager = new RandomTableManager();
            AStar aStar = new AStar();
            DijkstraMap dijkstraMap = new DijkstraMap();
            Log log = new Log();
            AttributeManager attributeManager = new AttributeManager();
            ManualManager manualManager = new ManualManager();

            player = new Entity();
            player.AddComponent(new Vector(0, 0));
            player.AddComponent(new Draw(Color.Lime, Color.Black, '@'));
            player.AddComponent(new Description("You", "It's you."));
            player.AddComponent(new Attributes(20, 1f, 1, 1, 10, 10));
            player.AddComponent(new TurnComponent());
            player.AddComponent(new Movement(new List<int> { 1, 2 }));
            player.AddComponent(new InventoryComponent());
            player.AddComponent(new Actor());
            player.AddComponent(new PlayerController());
            player.AddComponent(new Faction("Player"));
            player.AddComponent(new Harmable());

            dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.LightGray, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), 90, new Random());

            Log.ClearLog();
            Log.DisplayLog();
            rootConsole.particles.Clear();
            ClearSFX();

            GenerateNewFloor();

            UpdateNewPlayer();

            TurnManager.AddActor(player.GetComponent<TurnComponent>());
            player.GetComponent<TurnComponent>().StartTurn();

            Entity ram = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.SandyBrown, Color.Black, 'r'),
                new Description("Cave-Dweller Ram", "A shaggy ram with knotted long fur. It is highly aggresive and seems to attack anything that it sees move."),
                new InventoryComponent(),
                new Faction("Enemy"),
                new Harmable(),
                new Attributes(5, .6f, 1, -1, 8, 3),
                new Movement(new List<int>() { 1, 2 }),
                new TurnComponent(),
                new BeastAI(15, 0, 1, 10, 150, 0, 0),

            });

            Entity horns = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Horns", "Pointy Horns."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 2, 2, 2),
            });

            InventoryManager.EquipItem(ram, horns);
            Math.ClearTransitions(ram);
            JsonDataManager.SaveEntity(ram, "Cave-Dweller Ram");


            Entity boneling = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.SlateBlue, Color.Black, 's'),
                new Description("Skeletal Boneling", "While one of the weakest of those past death, it still strikes a deep fear into you."),
                new InventoryComponent(),
                new Faction("Enemy"),
                new Harmable(),
                new Attributes(6, .8f, 1, 0, 6, 4),
                new Movement(new List<int>() { 1, 2 }),
                new TurnComponent(),
                new MinionAI(50, 0, 1, 10, 150, 0, 0),

            });

            Math.ClearTransitions(boneling);
            JsonDataManager.SaveEntity(boneling, "Skeletal Boneling");

            Entity boneLord = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.SlateGray, Color.Black, 's'),
                new Description("Skeletal Bonelord", "A lord of the those past death, while made of little more than bone and sinew the strength from which it commands strikes fear into you."),
                new InventoryComponent(),
                new Faction("Enemy"),
                new Harmable(),
                new Attributes(15, .8f, 2, 1, 8, 6),
                new Movement(new List<int>() { 1, 2 }),
                new TurnComponent(),
                new UndeadAI(50, 0, 1, 10, 150, 0, 0),
                new SpawnDetails(),
                new SummonMinions("", new List<string>() { "Skeletal Boneling" }, 2, "Spawn"),
            });

            Math.ClearTransitions(boneLord);
            JsonDataManager.SaveEntity(boneLord, "Skeletal Bonelord");


            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Scroll of Mapping"), player);
            }

            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Fireball"), player);
            }

            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Lightning"), player);
            }

            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Transposition"), player);
            }


            isGameActive = true;
        }
        public static void GenerateNewFloor(Random seed = null)
        {
            floor++;

            rootConsole.Children.MoveToTop(loadingConsole);
            CreateConsoleBorder(loadingConsole);

            dungeonGenerator.GenerateTowerFloor();
            dungeonGenerator.PlacePlayer();

            rootConsole.Children.MoveToBottom(loadingConsole);

            //ParticleEffects.RevealNewFloor();
        }
        public static void PlayerDeath(Entity killingEntity, Vector position)
        {
            foreach (TurnComponent component in TurnManager.entities)
            {
                component.isAlive = false;
                component.isTurnActive = false;
            }
            TurnManager.entities.Clear();
            isGameActive = false;

            floor = 0;
            ClearSFX();
            ShadowcastFOV.RevealAll();
            rootConsole.particles.Clear();
            FloorFadeAway();
            CreateDeathMessage();
        }
        public static void FloorFadeAway()
        {
            for (int x = 0; x < gameWidth; x++)
            {
                for (int y = 0; y < gameHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        Draw draw1 = new Draw();
                        if (tiles[x, y].actor != null)
                        {
                            draw1.fColor = tiles[x, y].actor.GetComponent<Draw>().fColor;
                            draw1.bColor = tiles[x, y].actor.GetComponent<Draw>().bColor;
                            draw1.character = tiles[x, y].actor.GetComponent<Draw>().character;
                        }
                        else if (tiles[x, y].item != null)
                        {
                            draw1.fColor = tiles[x, y].item.GetComponent<Draw>().fColor;
                            draw1.bColor = tiles[x, y].item.GetComponent<Draw>().bColor;
                            draw1.character = tiles[x, y].item.GetComponent<Draw>().character;
                        }
                        else
                        {
                            draw1.fColor = tiles[x, y].GetComponent<Draw>().fColor;
                            draw1.bColor = tiles[x, y].GetComponent<Draw>().bColor;
                            draw1.character = tiles[x, y].GetComponent<Draw>().character;
                        }

                        ParticleManager.CreateParticle(true, new Vector(x, y), random.Next(5, 10) + y, 5, "None", draw1, null, true, true);

                        tiles[x, y].GetComponent<Draw>().fColor = Color.Black;
                        tiles[x, y].GetComponent<Draw>().bColor = Color.Black;
                    }
                }
            }

            foreach (Tile tile in tiles)
            {
                if (tile != null)
                {
                    if (tile.actor != null) { tile.actor = null; }
                    if (tile.item != null) { tile.item = null; }
                }
            }
        }
        public static void CreateDeathMessage()
        {
            string deathMessage = " < You have died. > ";
            string newGame = "< New Game: N >";
            string quitGame = "< Quit Game: Q ";
            int baseLength = deathMessage.Length + 2;

            playerConsole.DrawBox(new Rectangle(3, 4, playerConsole.Width - 6, playerConsole.Height - 7),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            playerConsole.DrawBox(new Rectangle((playerConsole.Width / 2) - (baseLength / 2), (playerConsole.Height / 3) - 3, baseLength, playerConsole.Height / 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black, 177)));

            int startY = (playerConsole.Height / 2) - 7;
            playerConsole.Print((playerConsole.Width / 2) - ($"{deathMessage}".Length / 2), startY, $"{deathMessage}", Color.Yellow, Color.Black);
            startY += 6;
            playerConsole.Print((playerConsole.Width / 2) - ($"{newGame}".Length / 2), startY, $"{newGame}", Color.Yellow, Color.Black);
            startY += 6;
            playerConsole.Print((playerConsole.Width / 2) - ($"{quitGame}".Length / 2), startY, $"{quitGame}", Color.Yellow, Color.Black);

            CreateConsoleBorder(playerConsole);
        }
        public static void CreateWinMessage()
        {
            string deathMessage = " < You have ascended!. > ";
            string newGame = "< New Game: N >";
            string quitGame = "< Quit Game: Q ";
            int baseLength = deathMessage.Length + 2;

            playerConsole.DrawBox(new Rectangle(3, 4, playerConsole.Width - 6, playerConsole.Height - 7),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            playerConsole.DrawBox(new Rectangle((playerConsole.Width / 2) - (baseLength / 2), (playerConsole.Height / 3) - 3, baseLength, playerConsole.Height / 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black, 177)));

            int startY = (playerConsole.Height / 2) - 7;
            playerConsole.Print((playerConsole.Width / 2) - ($"{deathMessage}".Length / 2), startY, $"{deathMessage}", Color.Yellow, Color.Black);
            startY += 6;
            playerConsole.Print((playerConsole.Width / 2) - ($"{newGame}".Length / 2), startY, $"{newGame}", Color.Yellow, Color.Black);
            startY += 6;
            playerConsole.Print((playerConsole.Width / 2) - ($"{quitGame}".Length / 2), startY, $"{quitGame}", Color.Yellow, Color.Black);

            CreateConsoleBorder(playerConsole);
        }
        public static void MoveCamera(Entity entity)
        {
            Vector vector = entity.GetComponent<Vector>();
            MoveCamera(vector);
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

                        if (uiSfx[tx, ty] != null) { uiSfx[tx, ty].DrawToScreen(mapConsole, x, y); }
                        else if (sfx[tx, ty] != null && sfx[tx, ty].alwaysVisible) { sfx[tx, ty].Draw(new Vector(x, y)); }
                        else if (!visibility.visible && !visibility.explored) { mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, (char)0)); }
                        else if (!visibility.visible && visibility.explored)
                        {
                            Draw draw = tile.GetComponent<Draw>();
                            mapConsole.SetCellAppearance(x, y, new ColoredGlyph(new Color(draw.fColor, .5f), Color.Black, draw.character));
                        }
                        else if (sfx[tx, ty] != null && sfx[tx, ty].showOverActors) { sfx[tx, ty].Draw(new Vector(x, y)); }
                        else if (tile.actor != null) { tile.actor.GetComponent<Draw>().DrawToScreen(mapConsole, x, y); }
                        else if (sfx[tx, ty] != null) { sfx[tx, ty].Draw(new Vector(x, y)); }
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
        public static void DrawLoadingScreen()
        {
            int particleCount = 10;
            if (rootConsole.particles.Count < particleCount)
            {
                int creationCount = random.Next((rootConsole.particles.Count - particleCount) - 1, (rootConsole.particles.Count - particleCount) + 1);

                List<int> exemptTiles = new List<int>() { };
                for (int i = 0; i < creationCount; i++)
                {
                    int chosenTile = random.Next(0, loadingConsole.Width);
                    exemptTiles.Add(chosenTile);

                    //CreateSFX(new Vector(chosenTile, 0), new Draw[] { new Draw(Color.Gray, Color.Black, (char)177) }, 100, "WanderNorth", 2);
                }
            }

            for (int y = 0; y < loadingConsole.Height; y++)
            {
                for (int x = 0; x < loadingConsole.Width; x++)
                {
                    sfx[x, y]?.GetComponent<Draw>().DrawToScreen(loadingConsole, x, y);
                }
            }

            loadingConsole.Print((loadingConsole.Width / 2) - ("< Loading >".Length / 2), loadingConsole.Height / 2, "< Loading >");

            loadingConsole.IsDirty = true;

            CreateConsoleBorder(loadingConsole);
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
        public static void ClearUISFX()
        {
            for (int x = 0; x < gameWidth; x++)
            {
                for (int y = 0; y < gameHeight; y++)
                {
                    uiSfx[x, y] = null;
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
        public List<Particle> particles = new List<Particle>();
        public float timeLeft = 250;
        public Timer timer = null;
        public int lastUpdate = 5;
        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            if (timer == null)
            {
                timer = new Timer(new TimeSpan(0, 0, 0, 0, 50));
                timer.TimerElapsed += UpdateParticle;
            }

            timer.Update(this, delta);

            Program.DrawMap();
        }
        public void UpdateParticle(object sender, EventArgs e)
        {
            if (particles.Count > 0)
            {
                Program.ClearSFX();

                for (int i = 0; i < particles.Count; i++)
                {
                    Particle particle = particles[i];
                    if (lastUpdate <= particle.speed)
                    {
                        particle?.Progress();
                    }
                    else
                    {
                        particle?.SetParticleInPlace(particle.GetComponent<Vector>());
                    }
                }

                if (particles.Count == 0)
                {
                    Program.ClearSFX();
                }

                if (lastUpdate <= 1) { lastUpdate = 5; }
                else { lastUpdate--; }

                timer.Restart();
            }
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