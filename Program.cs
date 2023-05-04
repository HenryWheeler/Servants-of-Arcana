using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadConsole.Input;
using SadConsole.Instructions;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using Console = SadConsole.Console;

namespace Servants_of_Arcana
{
    public class Program
    {
        public static RootConsole rootConsole { get; set; }
        public static TitleConsole logConsole { get; set; }
        public static Console mapConsole { get; set; }
        public static TitleConsole playerConsole { get; set; }
        public static InventoryConsole inventoryConsole { get; set; }
        public static TitleConsole lookConsole { get; set; }
        public static TitleConsole targetConsole { get; set; }
        public static InteractionConsole interactionConsole { get; set; }
        public static TitleConsole manualConsole { get; set; }
        public static TitleConsole loadingConsole { get; set; }
        public static Console titleConsole { get; set; }
        public static Entity player { get; set; }
        public static bool isGameActive { get; set; } = false;
        public static bool isPlayerCreatingCharacter { get; set; } = false;
        public static bool devTesting = true;
        public static string playerName { get; set; }
        public static DungeonGenerator generator { get; set; }

        //The Size of the root console
        public static int screenWidth = 120;
        public static int screenHeight = 55;

        //The size of the map console
        public static int mapWidth = 70;
        public static int mapHeight = 55;

        public static int interactionWidth = 50;
        public static int interactionHeight = 35;

        private static int messageWidth = 28;
        private static int messageHeight = 55;

        private static int rogueWidth = 22;
        private static int rogueHeight = 23;

        private static int inventoryWidth = 22;
        private static int inventoryHeight = 32;

        private static int targetWidth = 28;
        private static int targetHeight = 55;

        private static int lookWidth = 28;
        private static int lookHeight = 55;
        public static int offSetX { get; set; }
        public static int offSetY { get; set; }
        public static int minX { get; set; }
        public static int maxX { get; set; }
        public static int minY { get; set; }
        public static int maxY { get; set; }
        public static int floor { get; set; } = 0;

        public static Tile[,] tiles = new Tile[mapWidth, mapHeight];
        public static Particle[,] sfx = new Particle[mapWidth, mapHeight];
        public static Draw[,] uiSfx = new Draw[mapWidth, mapHeight];
        public static Random random = new Random();
        public static DungeonGenerator dungeonGenerator;
        private static void Main(string[] args)
        {
            Settings.WindowTitle = "Servants of Arcana";

            Game.Create(screenWidth, screenHeight, "Resources/Fonts/ascii_6x6.font.json");
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

            mapConsole = new Console(mapWidth, mapHeight) { Position = new Point(0, 0) };
            mapConsole.FontSize = new Point(12, 12);
            logConsole = new TitleConsole("< Message Log >", messageWidth, messageHeight) { Position = new Point((int)rogueWidth + (int)mapWidth, 0) };
            playerConsole = new TitleConsole("< The Rogue @ >", rogueWidth, rogueHeight) { Position = new Point((int)(mapWidth), 0) };
            inventoryConsole = new InventoryConsole(inventoryWidth, inventoryHeight) { Position = new Point((int)(mapWidth), rogueHeight) };
            targetConsole = new TitleConsole("< Targeting >", targetWidth, targetHeight) { Position = new Point((int)rogueWidth + (int)mapWidth, 0) };
            lookConsole = new TitleConsole("< Looking >", lookWidth, lookHeight) { Position = new Point((int)rogueWidth + (int)mapWidth, 0) };
            interactionConsole = new InteractionConsole(interactionWidth, interactionHeight) { Position = new Point((screenWidth / 2) - (interactionWidth / 2), (screenHeight / 2) - (interactionHeight / 2)) };
            manualConsole = new TitleConsole("< Manual >", screenWidth, screenHeight) { Position = new Point(0, 0) };
            loadingConsole = new TitleConsole("< Loading >", screenWidth, screenHeight) { Position = new Point(0, 0) };

            using System.IO.Stream fileStream = new System.IO.FileStream("Resources/title.xp", System.IO.FileMode.Open);
            var image = SadConsole.Readers.REXPaintImage.Load(fileStream);
            titleConsole = new Console(92, 55, image.ToLayersComponent().ToArray()[0].ToArray()) { Position = new Point(14, 0) };
            titleConsole.DrawBox(new Rectangle(0, 0, titleConsole.Width, titleConsole.Height),
            ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));
            titleConsole.IsDirty = true;

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
            rootConsole.Children.MoveToTop(inventoryConsole);

            rootConsole.Children.MoveToBottom(interactionConsole);
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
        }
        public static void UpdateNewPlayer()
        {
            player.GetComponent<Movement>().onMovement += MoveCamera;
            player.GetComponent<Movement>().onMovement += AttributeManager.UpdateAttributes;

            player.GetComponent<Harmable>().onDeath += PlayerDeath;

            player.GetComponent<Description>().name = playerName;

            AttributeManager.UpdateAttributes(player);
            ShadowcastFOV.Compute(player.GetComponent<Vector>(), player.GetComponent<Attributes>().sight);

            player.SetDelegates();
        }
        public static void StartCharacterCreation()
        {
            if (devTesting)
            {
                playerName = "Gork";
                StartNewGame();
            }
            else
            {
                isPlayerCreatingCharacter = true;
                rootConsole.Children.MoveToTop(titleConsole);

                playerName = "";

                InteractionManager.CharacterCreationDisplay();
            }
        }
        public static void StartNewGame()
        {
            rootConsole.Children.MoveToTop(loadingConsole);
            CreateConsoleBorder(loadingConsole);
            loadingConsole.IsDirty = true;


            isPlayerCreatingCharacter = false;
            rootConsole.Children.MoveToBottom(titleConsole);

            ItemIdentityManager itemIdentityManager = new ItemIdentityManager();
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
            player.AddComponent(new Description(playerName, "It's you."));
            player.AddComponent(new Attributes(30, 1f, 1, 1, 10, 10));
            player.AddComponent(new TurnComponent());
            player.AddComponent(new Movement(new List<int> { 1, 2 }));
            player.AddComponent(new InventoryComponent());
            player.AddComponent(new Actor());
            player.AddComponent(new PlayerController());
            player.AddComponent(new Faction("Player"));
            player.AddComponent(new Harmable());

            dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.LightGray, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), 90, new Random());

            Log.DisplayLog();
            rootConsole.particles.Clear();
            ClearSFX();

            if (dungeonGenerator.seed.Next(1, 101) > 50)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Dagger"), player);
            }
            else
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Short Blade"), player);
            }

            Entity beetle = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.DarkSlateGray, Color.Black, 'b'),
                new Description("Ruin Beetle", "A large flat beetle, it is very slow but surprisingly powerful."),
                new InventoryComponent(),
                new Faction("Insect"),
                new Harmable(),
                new Attributes(10, .25f, 2, -1, 15, 3),
                new Movement(new List<int>() { 1 }),
                new TurnComponent(),
                new MinionAI(50, 0, 10, 25, 100, 0, 0),
            });

            Entity mandibles2 = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Mandible", "A horrid thing."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 0, 1, 6),
            });

            InventoryManager.EquipItem(beetle, mandibles2);
            Math.ClearTransitions(beetle);
            JsonDataManager.SaveEntity(beetle, "Ruin Beetle");

            JsonDataManager.SaveEntity(new Tile(0, 0, Color.Green, Color.Black, '*', "Bush", "A small dense bush, you cannot fathom how it lives in this environment.", 1, true), "Bush 1");
            JsonDataManager.SaveEntity(new Tile(0, 0, Color.DarkGreen, Color.Black, '*', "Bush", "A small dense bush, you cannot fathom how it lives in this environment.", 1, true), "Bush 2");
            JsonDataManager.SaveEntity(new Tile(0, 0, Color.Gray, Color.Black, '*', "Bush", "A small dense bush, you cannot fathom how it lives in this environment.", 1, true), "Bush 3");

            Tile tile = new Tile(0, 0, Color.Green, Color.Black, (char)15, "Thorny Bush", "Not only has this small bush survived in a strange environment, it has grown thorns to fight back.", 1, true);
            tile.AddComponent(new Trap(false));
            tile.AddComponent(new Thorns());
            JsonDataManager.SaveEntity(tile, "Thorny Bush");

            Entity key = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Gold, Color.Black, '/'),
                new Description("Golden Key", "A horrid thing, dripping with ichor and bile it forms the primary method of unlocking upward ascension."),
                new Usable(0, 0, 0, "Unlock", "causes the key to evaporate. You hear the sounds of mechanisms unlocking and shifting."),
                new Charges(1),
                new Key(),
            });

            JsonDataManager.SaveEntity(key, "Ichor Key");

            JsonDataManager.SaveEntity(new Tile(0, 0, Color.White, Color.Black, (char)8, "Terminal", "A deep hole in the stone of the surrounding wall. It weeps black ichor from granite veins, the hole looks to be deep enough to fit your entire arm inside.", 0, true), "Terminal");


            Entity ichorSentinel = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.DarkSlateGray, Color.Black, 'i'),
                new Description("Tower Sentinel", "A large construct of ichor and stone, far taller than you its smooth stone skin is carved with channels of bleeding ichor." +
                " Unlike the infestation of the tower, it is designed and intentional. They were made to help the cleansing of the tower."),
                new InventoryComponent(),
                new Faction("Construct"),
                new Harmable(),
                new Attributes(20, .5f, 3, -3, 12, 5),
                new Movement(new List<int>() { 1, 2 }),
                new TurnComponent(),
                new Usable(10, 3, 1, "", "A spike of ichor is formed and fired from the Tower Sentinel's arm."),
                new IchorSpike(3),
                new GuardAI(50, 0, 10, 25, 100, 0, 0),
            });

            Entity ichorWeapon = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Ichor Spike", "A horrid thing."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 0, 1, 8),
            });

            InventoryManager.EquipItem(ichorSentinel, ichorWeapon);
            Math.ClearTransitions(ichorSentinel);
            JsonDataManager.SaveEntity(ichorSentinel, "Tower Sentinel");

            Entity brokenIchorSentinel = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.DarkRed, Color.Black, 'i'),
                new Description("Broken Sentinel", "A broken sentinel, while made of perfected ichor stone like its brothers," +
                " this one has cracked. Deadened and weakened it is no longer welcome, it takes out its fury on everything around it."),
                new InventoryComponent(),
                new Faction("Broken-Construct"),
                new Harmable(),
                new Attributes(10, .5f, 1, -3, 11, 4),
                new Movement(new List<int>() { 1, 2 }),
                new TurnComponent(),
                new Usable(10, 2, 1, "", "A spike of ichor is formed and fired from the Tower Sentinel's arm."),
                new IchorSpike(2),
                new GuardAI(50, 0, 10, 25, 100, 0, 0),
            });

            Entity brokenIchorWeapon = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Ichor Spike", "A horrid thing."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 0, 1, 4),
            });

            InventoryManager.EquipItem(brokenIchorSentinel, brokenIchorWeapon);
            Math.ClearTransitions(brokenIchorSentinel);
            JsonDataManager.SaveEntity(brokenIchorSentinel, "Broken Sentinel");

            Entity ichorFly = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.DarkSlateGray, Color.Black, 'i'),
                new Description("Tower Fly", "A small construct made in the image of a warped fly, it flits around the tower quickly. " +
                "Ironically the construct made to look like a pest is designed to cleanse them."),
                new InventoryComponent(),
                new Faction("Construct"),
                new Harmable(),
                new Attributes(8, .8f, 1, -3, 11, 7),
                new Movement(new List<int>() { 1, 2, 3 }),
                new TurnComponent(),
                new Usable(10, 2, 1, "", "A spike of ichor is formed and fired from the maw of the Tower Fly."),
                new IchorSpike(2),
                new MinionAI(50, 0, 10, 25, 100, 0, 0),
            });

            Entity ichorFlyWeapon = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Ichor Maw", "A horrid thing."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 0, 1, 4),
            });

            InventoryManager.EquipItem(ichorFly, ichorFlyWeapon);
            Math.ClearTransitions(ichorFly);
            JsonDataManager.SaveEntity(ichorFly, "Tower Fly");

            /*
            if (devTesting)
            {
                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Skin Coat"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Ring of Greed"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Digging"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Hidden Fang"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Boomerang"), player);
                }

                for (int i = 0; i < 2; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Scroll of Mapping"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Fireball"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Lightning"), player);
                }

                for (int i = 0; i < 1; i++)
                {
                    InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Wand of Transposition"), player);
                }
            }
            */

            GenerateNewFloor();

            UpdateNewPlayer();

            TurnManager.AddActor(player.GetComponent<TurnComponent>());
            player.GetComponent<TurnComponent>().StartTurn();

            isGameActive = true;
        }
        public static void GenerateNewFloor(Random seed = null)
        {
            floor++;

            dungeonGenerator.GenerateTowerFloor();
            dungeonGenerator.PlacePlayer();
            dungeonGenerator.decayChance += 2;

            rootConsole.Children.MoveToBottom(loadingConsole);
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
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (Math.CheckBounds(x, y) && Program.tiles[x, y].GetComponent<Visibility>().explored)
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
            string quitGame = "< Quit Game: Q >";
            int baseLength = deathMessage.Length + 4;

            playerConsole.DrawBox(new Rectangle(0, 0, playerConsole.Width, playerConsole.Height),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            playerConsole.DrawBox(new Rectangle((playerConsole.Width / 2) - (baseLength / 2), 1, baseLength, playerConsole.Height - 2),
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
            string quitGame = "< Quit Game: Q >";
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
            if (player == null) { return; }

            Vector vector = player.GetComponent<Vector>();
            int range = player.GetComponent<Attributes>().sight;
            int offset = range * 8;

            for (int y = 0; y <= mapHeight; y++)
            {
                for (int x = 0; x <= mapWidth; x++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        Tile tile = tiles[x, y];
                        Visibility visibility = tile.GetComponent<Visibility>();

                        if (uiSfx[x, y] != null) { uiSfx[x, y].DrawToScreen(mapConsole, x, y); }
                        else if (sfx[x, y] != null && sfx[x, y].alwaysVisible) { sfx[x, y].Draw(new Vector(x, y)); }
                        else if (!visibility.visible && !visibility.explored)
                        {
                            mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, '?'));
                        }
                        else if (!visibility.visible && visibility.explored)
                        {
                            Draw draw = tile.GetComponent<Draw>();
                            mapConsole.SetCellAppearance(x, y, new ColoredGlyph(new Color(draw.fColor, .3f), Color.Black, draw.character));
                        }
                        else if (sfx[x, y] != null && sfx[x, y].showOverActors) { sfx[x, y].Draw(new Vector(x, y)); }
                        else if (tile.actor != null) 
                        {
                            Draw draw = tile.actor.GetComponent<Draw>();
                            mapConsole.SetCellAppearance(x, y, new ColoredGlyph(draw.fColor, draw.bColor, draw.character));
                        }
                        else if (sfx[x, y] != null) { sfx[x, y].Draw(new Vector(x, y)); }
                        else if (tile.item != null) 
                        {
                            Draw draw = tile.item.GetComponent<Draw>();
                            mapConsole.SetCellAppearance(x, y, new ColoredGlyph(draw.fColor, draw.bColor, draw.character));
                        }
                        else 
                        {
                            Draw draw = tile.GetComponent<Draw>();
                            int distance = (int)Math.Distance(vector.x, vector.y, x, y) * 30;
                            if (devTesting)
                            {
                                mapConsole.SetCellAppearance(x, y, new ColoredGlyph(draw.fColor, Color.Black, draw.character));
                            }
                            else
                            {
                                mapConsole.SetCellAppearance(x, y, new ColoredGlyph(new Color((int)MathF.Max((draw.fColor.R + offset) - distance, .3f),
                                    (int)MathF.Max((draw.fColor.G + offset) - distance, .15f),
                                    (int)MathF.Max((draw.fColor.B + offset) - distance, .15f)), Color.Black, draw.character));
                            }
                        }
                    }
                    else
                    {
                        mapConsole.SetCellAppearance(x, y, new ColoredGlyph(Color.Black, Color.Black, (char)0));
                    }
                }
            }

            //CreateConsoleBorder(mapConsole);

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
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    sfx[x, y] = null;
                }
            }
        }
        public static void ClearUISFX()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    uiSfx[x, y] = null;
                }
            }
        }
        public static void CreateConsoleBorder(TitleConsole console, bool includeTitle = true)
        {
            console.DrawBox(new Rectangle(0, 0, console.Width, console.Height),
            ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));

            if (includeTitle)
            {
                console.Print(0, 0, console.title.Align(HorizontalAlignment.Center, console.Width, (char)196), Color.AntiqueWhite, Color.Black);
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
    public class InventoryConsole : Console
    {
        public List<InventoryDisplaySlot> items = new List<InventoryDisplaySlot>();
        public override void Render(TimeSpan delta)
        {
            this.DrawBox(new Rectangle(0, 0, Width, Height),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));
            base.Render(delta);
        }
        public void UpdateLists(List<Entity> items)
        {
            this.Fill(Color.Black, Color.Black, 176);
            this.items.Clear();
            int width = 20;

            int y = 1;

            for (int i = 0; i < 10; i++)
            {
                if (i < items.Count && items[i] != null)
                {
                    if (i == 9)
                    {
                        this.items.Add(new InventoryDisplaySlot(this, items[i], new Vector(1, y), width, 3, 0));
                    }
                    else
                    {
                        this.items.Add(new InventoryDisplaySlot(this, items[i], new Vector(1, y), width, 3, i + 1));
                    }
                }
                else
                {
                    if (i == 9)
                    {
                        this.items.Add(new InventoryDisplaySlot(this, null, new Vector(1, y), width, 3, 0));
                    }
                    else
                    {
                        this.items.Add(new InventoryDisplaySlot(this, null, new Vector(1, y), width, 3, i + 1));
                    }
                }

                y += 3;
            }

            foreach (InventoryDisplaySlot slot in this.items)
            {
                slot.Draw(false);
            }

            IsDirty = true;
        }
        public void SelectItem(int number)
        {
            if (number <= 9) 
            {
                InventoryDisplaySlot slot = items[number];

                if (slot.item != null && !Look.looking && !TargetingSystem.isTargeting &&
                    Program.player.GetComponent<TurnComponent>().isAlive && !Program.player.GetComponent<PlayerController>().confirming)
                {
                    slot.selected = true;
                    InventoryManager.OpenInventoryDisplay(slot.item);

                    foreach (InventoryDisplaySlot temp in items)
                    {
                        temp.selected = false;
                    }
                }
                else
                {
                    Log.Add("You cannot select an empty slot.");
                }
            }
        }
        public void ProcessSlotUpdate(MouseScreenObjectState state)
        {
            foreach (InventoryDisplaySlot slot in items)
            {
                if (slot.CheckIfMouseValid(state))
                {
                    if (state.Mouse.LeftClicked)
                    {
                        if (slot.item != null && !Look.looking && !TargetingSystem.isTargeting && 
                            Program.player.GetComponent<TurnComponent>().isAlive && !Program.player.GetComponent<PlayerController>().confirming)
                        {
                            slot.selected = true;
                            InventoryManager.OpenInventoryDisplay(slot.item);

                            foreach (InventoryDisplaySlot temp in items)
                            {
                                temp.selected = false;
                            }
                        }
                        else
                        {
                            Log.Add("You cannot select an empty slot.");
                        }
                    }

                    slot.Draw(true);
                }
                else
                {
                    slot.Draw(false);
                }
            }
        }
        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            ProcessSlotUpdate(state);

            return base.ProcessMouse(state);
        }
        public InventoryConsole(int _width, int _height)
            : base(_width, _height)
        {
            this.Fill(Color.Black, Color.Black, 176);
        }
    }
    public class InventoryDisplaySlot
    {
        public InventoryConsole parent { get; set; }
        public Entity item { get; set; }
        public Vector corner { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool selected { get; set; }
        public int number { get; set; }
        public bool CheckIfMouseValid(MouseScreenObjectState state)
        {
            if (!state.IsOnScreenObject) return false;
            if (corner.x + 1 <= state.SurfaceCellPosition.X && corner.y + 1 <= state.SurfaceCellPosition.Y &&
                corner.x + width - 1 >= state.SurfaceCellPosition.X && corner.y + height - 1 >= state.SurfaceCellPosition.Y)
            {
                return true;
            }
            else return false;
        }
        public void Draw(bool highlighted)
        {
            if (highlighted || selected)
            {
                parent.DrawBox(new Rectangle(corner.x, corner.y, width, height),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, 
                new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.White, Color.White)));

                if (item != null)
                {
                    Description description = item.GetComponent<Description>();

                    if (ItemIdentityManager.IsItemIdentified(description.name))
                    {
                        if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                        {
                            parent.Print(corner.x, corner.y + 1, $"{item.GetComponent<Description>().name} - E".Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
                        }
                        else
                        {
                            parent.Print(corner.x, corner.y + 1, item.GetComponent<Description>().name.Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
                        }
                    }
                    else
                    {
                        if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                        {
                            parent.Print(corner.x, corner.y + 1, $"Unknown Item - E".Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
                        }
                        else
                        {
                            parent.Print(corner.x, corner.y + 1, "Unknown Item".Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
                        }
                    }
                }
                else
                {
                    parent.Print(corner.x, corner.y + 1, "Empty".Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
                }

                parent.Print(corner.x + 1, corner.y + 1, $"{number}", Color.Black, Color.White);
            }
            else
            {
                parent.DrawBox(new Rectangle(corner.x, corner.y, width, height),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, 
                new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black)));

                if (item != null)
                {
                    Description description = item.GetComponent<Description>();

                    if (ItemIdentityManager.IsItemIdentified(description.name))
                    {
                        if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                        {
                            parent.Print(corner.x, corner.y + 1, $"{item.GetComponent<Description>().name} - Equipped".Align(HorizontalAlignment.Center, width), Color.White, Color.Black);
                        }
                        else
                        {
                            parent.Print(corner.x, corner.y + 1, item.GetComponent<Description>().name.Align(HorizontalAlignment.Center, width), Color.White, Color.Black);
                        }
                    }
                    else
                    {
                        if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                        {
                            parent.Print(corner.x, corner.y + 1, $"Unknown Item - Equipped".Align(HorizontalAlignment.Center, width), Color.White, Color.Black);
                        }
                        else
                        {
                            parent.Print(corner.x, corner.y + 1, "Unknown Item".Align(HorizontalAlignment.Center, width), Color.White, Color.Black);
                        }
                    }
                }
                else
                {
                    parent.Print(corner.x, corner.y + 1, "Empty".Align(HorizontalAlignment.Center, width), Color.White, Color.Black);
                }

                parent.Print(corner.x + 1, corner.y + 1, $"{number}", Color.White, Color.Black);
            }

            parent.IsDirty = true;
        }
        public InventoryDisplaySlot(InventoryConsole parent, Entity item, Vector corner, int width, int height, int number)
        {
            this.parent = parent;
            this.item = item;
            this.corner = corner;
            this.width = width;
            this.height = height;
            this.number = number;
        }
    }
    public class InteractionConsole : Console
    {
        public int equipY = 26;
        public int useY = 29;
        public int dropY = 32;
        public int firstY = 10;
        public int secondY = 27;
        public bool selected { get; set; } = false;
        public bool equipSelected { get; set; } = false;
        public bool useSelected { get; set; } = false;
        public bool dropSelected { get; set; } = false;
        public bool confirmSelected { get; set; } = false;
        public bool denySelected { get; set; } = false;
        public bool firstSelected { get; set; } = false;
        public bool secondSelected { get; set; } = false;
        public override void Render(TimeSpan delta)
        {
            this.DrawBox(new Rectangle(0, 0, Width, Height),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));

            if (InteractionManager.popupActive && InventoryManager.isInventoryOpen &&
                !Program.rootConsole.GetSadComponent<KeyboardComponent>().confirming)
            {
                this.DrawBox(new Rectangle(Width - 3, 0, 3, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));

                if (selected)
                {
                    this.Print(Width - 2, 1, "X", Color.Black, Color.White);
                }
                else
                {
                    this.Print(Width - 2, 1, "X", Color.Yellow, Color.Black);
                }

                if (InteractionManager.canUnequip || InteractionManager.canEquip)
                {
                    if (equipSelected)
                    {
                        if (InteractionManager.canUnequip)
                        {
                            this.Print(1, equipY, "Unequip - E".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Black, Color.White);
                        }
                        else if (InteractionManager.canEquip)
                        {
                            this.Print(1, equipY, "Equip - E".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Black, Color.White);
                        }
                    }
                    else
                    {
                        if (InteractionManager.canUnequip)
                        {
                            this.Print(1, equipY, "Unequip - E".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Yellow, Color.Black);
                        }
                        else if (InteractionManager.canEquip)
                        {
                            this.Print(1, equipY, "Equip - E".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Yellow, Color.Black);
                        }
                    }
                }
                else
                {
                    this.Print(1, equipY, "Equip - E".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Gray, Color.Black);
                }
                this.DrawBox(new Rectangle(1, equipY - 1, Program.interactionWidth - 2, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));

                if (InteractionManager.canUse)
                {
                    if (useSelected)
                    {
                        this.Print(1, useY, $"{InventoryManager.selectedItem.GetComponent<Usable>().action} - U".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Black, Color.White);
                    }
                    else
                    {
                        this.Print(1, useY, $"{InventoryManager.selectedItem.GetComponent<Usable>().action} - U".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Yellow, Color.Black);
                    }
                }
                else
                {
                    this.Print(1, useY, "Use - U".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Gray, Color.Black);
                }
                this.DrawBox(new Rectangle(1, useY - 1, Program.interactionWidth - 2, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));

                if (dropSelected)
                {
                    this.Print(1, dropY, "Drop - D".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Black, Color.White);
                }
                else
                {
                    this.Print(1, dropY, "Drop - D".Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Yellow, Color.Black);
                }
                this.DrawBox(new Rectangle(1, dropY - 1, Program.interactionWidth - 2, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));
            }
            else if (InteractionManager.popupActive && PrayerManager.isTerminalActive &&
                !Program.rootConsole.GetSadComponent<KeyboardComponent>().confirming)
            {
                this.DrawBox(new Rectangle(Width - 3, 0, 3, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));

                if (selected)
                {
                    this.Print(Width - 2, 1, "X", Color.Black, Color.White);
                }
                else
                {
                    this.Print(Width - 2, 1, "X", Color.Yellow, Color.Black);
                }

                if (firstSelected)
                {
                    this.Print(1, firstY, PrayerManager.firstOption.Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Black, Color.White);
                }
                else
                {
                    this.Print(1, firstY, PrayerManager.firstOption.Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Yellow, Color.Black);
                }
                this.DrawBox(new Rectangle(1, firstY - 1, Program.interactionWidth - 2, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));

                if (secondSelected)
                {
                    this.Print(1, secondY, PrayerManager.secondOption.Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Black, Color.White);
                }
                else
                {
                    this.Print(1, secondY, PrayerManager.secondOption.Align(HorizontalAlignment.Center, Program.interactionWidth - 2), Color.Yellow, Color.Black);
                }
                this.DrawBox(new Rectangle(1, secondY - 1, Program.interactionWidth - 2, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));
            }
            else if (Program.rootConsole.GetSadComponent<KeyboardComponent>().confirming)
            {
                this.DrawBox(new Rectangle(InteractionManager.confirmPromptPosition.x - 1, InteractionManager.confirmPromptPosition.y - 1, 3, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));

                this.DrawBox(new Rectangle(InteractionManager.denyPromptPosition.x - 1, InteractionManager.denyPromptPosition.y - 1, 3, 3),
                    ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black)));

                if (confirmSelected)
                {
                    Program.interactionConsole.Print(InteractionManager.confirmPromptPosition.x, InteractionManager.confirmPromptPosition.y, "Y", Color.Black, Color.White);
                }
                else
                {
                    Program.interactionConsole.Print(InteractionManager.confirmPromptPosition.x, InteractionManager.confirmPromptPosition.y, "Y", Color.Yellow, Color.Black);
                }

                if (denySelected)
                {
                    Program.interactionConsole.Print(InteractionManager.denyPromptPosition.x, InteractionManager.denyPromptPosition.y, "N", Color.Black, Color.White);
                }
                else
                {
                    Program.interactionConsole.Print(InteractionManager.denyPromptPosition.x, InteractionManager.denyPromptPosition.y, "N", Color.Yellow, Color.Black);
                }
            }

            base.Render(delta);
        }
        public override bool ProcessMouse(MouseScreenObjectState state)
        {
            if (state.IsOnScreenObject && InteractionManager.popupActive && InventoryManager.isInventoryOpen && 
                !Program.rootConsole.GetSadComponent<KeyboardComponent>().confirming)
            {
                if (state.SurfaceCellPosition.X <= Width && state.SurfaceCellPosition.X >= Width - 3 && 
                    state.SurfaceCellPosition.Y <= 3 && state.SurfaceCellPosition.Y >= 0)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        InventoryManager.CloseInventoryDisplay();
                    }

                    selected = true;
                    equipSelected = false;
                    useSelected = false;
                    dropSelected = false;

                    IsDirty = true;
                }
                else if (state.SurfaceCellPosition.X <= Width - 1 && state.SurfaceCellPosition.X >= 1 &&
                    state.SurfaceCellPosition.Y >= equipY - 1 && state.SurfaceCellPosition.Y <= equipY + 1)
                {
                    if (InteractionManager.canEquip || InteractionManager.canUnequip)
                    {
                        if (state.Mouse.LeftClicked)
                        {
                            Program.rootConsole.GetSadComponent<KeyboardComponent>().Equip();
                        }

                        selected = false;
                        equipSelected = true;
                        useSelected = false;
                        dropSelected = false;

                        IsDirty = true;
                    }
                }
                else if (state.SurfaceCellPosition.X <= Width - 1 && state.SurfaceCellPosition.X >= 1 &&
                    state.SurfaceCellPosition.Y >= useY - 1 && state.SurfaceCellPosition.Y <= useY + 1 && InteractionManager.canUse)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        Program.rootConsole.GetSadComponent<KeyboardComponent>().Use();
                    }

                    selected = false;
                    equipSelected = false;
                    useSelected = true;
                    dropSelected = false;

                    IsDirty = true;
                }
                else if (state.SurfaceCellPosition.X <= Width - 1 && state.SurfaceCellPosition.X >= 1 &&
                    state.SurfaceCellPosition.Y >= dropY - 1 && state.SurfaceCellPosition.Y <= dropY + 1)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        Program.rootConsole.GetSadComponent<KeyboardComponent>().Drop();
                    }

                    selected = false;
                    equipSelected = false;
                    useSelected = false;
                    dropSelected = true;

                    IsDirty = true;
                }
                else
                {
                    selected = false;
                    equipSelected = false;
                    useSelected = false;
                    dropSelected = false;
                }
            }
            if (state.IsOnScreenObject && InteractionManager.popupActive && PrayerManager.isTerminalActive &&
                    !Program.rootConsole.GetSadComponent<KeyboardComponent>().confirming)
            {
                if (state.SurfaceCellPosition.X <= Width && state.SurfaceCellPosition.X >= Width - 3 &&
                    state.SurfaceCellPosition.Y <= 3 && state.SurfaceCellPosition.Y >= 0)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        PrayerManager.CloseAltar();
                    }

                    selected = true;
                    firstSelected = false;
                    secondSelected = false;

                    IsDirty = true;
                }
                else if (state.SurfaceCellPosition.X <= Width - 1 && state.SurfaceCellPosition.X >= 1 &&
                    state.SurfaceCellPosition.Y >= firstY - 1 && state.SurfaceCellPosition.Y <= firstY + 1)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        Program.rootConsole.GetSadComponent<KeyboardComponent>().UseTerminal(true);
                    }

                    selected = false;
                    firstSelected = true;
                    secondSelected = false;

                    IsDirty = true;
                }
                else if (state.SurfaceCellPosition.X <= Width - 1 && state.SurfaceCellPosition.X >= 1 &&
                    state.SurfaceCellPosition.Y >= secondY - 1 && state.SurfaceCellPosition.Y <= secondY + 1)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        Program.rootConsole.GetSadComponent<KeyboardComponent>().UseTerminal(false);
                    }

                    selected = false;
                    firstSelected = false;
                    secondSelected = true;

                    IsDirty = true;
                }
                else
                {
                    selected = false;
                    firstSelected = false;
                    secondSelected = false;

                    IsDirty = true;
                }
            }
            else if (state.IsOnScreenObject && Program.rootConsole.GetSadComponent<KeyboardComponent>().confirming)
            {
                if (state.SurfaceCellPosition.X <= InteractionManager.confirmPromptPosition.x + 1 && state.SurfaceCellPosition.X >= InteractionManager.confirmPromptPosition.x - 1 && 
                    state.SurfaceCellPosition.Y <= InteractionManager.confirmPromptPosition.y + 1 && state.SurfaceCellPosition.Y >= InteractionManager.confirmPromptPosition.y - 1)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        Program.rootConsole.GetSadComponent<KeyboardComponent>().Confirm();
                    }

                    confirmSelected = true;
                    denySelected = false;

                    IsDirty = true;
                }
                else if (state.SurfaceCellPosition.X <= InteractionManager.denyPromptPosition.x + 1 && state.SurfaceCellPosition.X >= InteractionManager.denyPromptPosition.x - 1 &&
                    state.SurfaceCellPosition.Y <= InteractionManager.denyPromptPosition.y + 1 && state.SurfaceCellPosition.Y >= InteractionManager.denyPromptPosition.y - 1)
                {
                    if (state.Mouse.LeftClicked)
                    {
                        Program.rootConsole.GetSadComponent<KeyboardComponent>().Deny();
                    }

                    confirmSelected = false;
                    denySelected = true;

                    IsDirty = true;
                }
                else
                {
                    confirmSelected = false;
                    denySelected = false;
                    selected = false;
                    equipSelected = false;
                    useSelected = false;
                    dropSelected = false;

                    IsDirty = true;
                }
            }
            else
            {

                IsDirty = true;
            }

            return base.ProcessMouse(state);
        }
        public InteractionConsole(int _width, int _height)
            : base(_width, _height)
        {
            this.Fill(Color.Black, Color.Black, 176);
        }
    }
    public class TitleConsole : Console
    {
        public string title { get; set; }
        public override void Render(TimeSpan delta)
        {
            this.DrawBox(new Rectangle(0, 0, Width, Height),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));
            base.Render(delta);
        }
        public TitleConsole(string _title, int _width, int _height)
            : base(_width, _height)
        {
            title = _title;
            this.Fill(Color.Black, Color.Black, 176);

            Program.CreateConsoleBorder(this);
        }
    }
}