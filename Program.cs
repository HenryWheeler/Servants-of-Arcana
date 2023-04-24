using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using SadConsole;
using SadConsole.Components;
using SadConsole.Entities;
using SadConsole.Input;
using SadConsole.Instructions;
using SadRogue.Primitives;
using Servants_of_Arcana.Components;
using Servants_of_Arcana.Systems;
using System;
using System.Collections.Generic;
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
        public static int screenWidth = 100;
        public static int screenHeight = 55;

        //The size of the map console
        public static int mapWidth = (int)MathF.Ceiling((70 / 1.5f));
        public static int mapHeight = (int)MathF.Ceiling((40 / 1.5f));

        public static int interactionWidth = 50;
        public static int interactionHeight = 35;

        //The size of the ingame map
        public static int gameWidth = 120;
        public static int gameHeight = 120;

        private static int messageWidth = 70;
        private static int messageHeight = 15;

        private static int rogueWidth = 30;
        private static int rogueHeight = 23;

        private static int inventoryWidth = 30;
        private static int inventoryHeight = 32;

        private static int targetWidth = 30;
        private static int targetHeight = 55;

        private static int lookWidth = 30;
        private static int lookHeight = 55;
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
            mapConsole.FontSize = new Point(18, 18);
            logConsole = new TitleConsole("< Message Log >", messageWidth, messageHeight) { Position = new Point(0, (int)(mapHeight * 1.5f)) };
            playerConsole = new TitleConsole("< The Rogue @ >", rogueWidth, rogueHeight) { Position = new Point((int)(mapWidth * 1.5f), 0) };
            inventoryConsole = new InventoryConsole(inventoryWidth, inventoryHeight) { Position = new Point((int)(mapWidth * 1.5f), rogueHeight) };
            targetConsole = new TitleConsole("< Targeting >", targetWidth, targetHeight) { Position = new Point((int)(mapWidth * 1.5f), 0) };
            lookConsole = new TitleConsole("< Looking >", lookWidth, lookHeight) { Position = new Point((int)(mapWidth * 1.5f), 0) };
            interactionConsole = new InteractionConsole(interactionWidth, interactionHeight) { Position = new Point((screenWidth / 2) - (interactionWidth / 2), (screenHeight / 2) - (interactionHeight / 2)) };
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

            Log.ClearLog();
            Log.DisplayLog();
            rootConsole.particles.Clear();
            ClearSFX();

            /*
            Tile nest = new Tile(0, 0, Color.LightGray, Color.Black, '*', "Nest", "A large messy pile of soft down, sticks, and fur. It is rather comfortable, no wonder eggs tend to like it.", 1, false);
            JsonDataManager.SaveEntity(nest, "Nest");

            Event drakeLair = new Event(0, 0, new List<Component>()
            {
                new SpawnMinions(new List<string>() { "Sea-Drake" }, 1, "Spawn"),
            }, 4, 4, 10, 10, "Sphere");
            JsonDataManager.SaveEvent(drakeLair, "Drake Lair 1");

            Entity seaDrake = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.DarkBlue, Color.Black, 'D'),
                new Description("Sea-Drake", "A large scaled drake, this large creature razes the high seas and is known for keeping large hoards of treasures."),
                new InventoryComponent(),
                new Faction("Drake"),
                new Harmable(),
                new Attributes(50, .8f, 2, 3, 14, 10),
                new Movement(new List<int>() { 1, 2, 3 }),
                new TurnComponent(),
                new BeastAI(50, 0, 10, 25, 100, 5, 40),
                new Usable(15, 3, 0, "", "shoots a ball of flame!", new List<int>() { 1, 2, 3 }),
                new Explode(3, true, true),
            });
            Entity seaDrakeTalons = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Talons", "Talons."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 0, 2, 8),
            });
            InventoryManager.EquipItem(seaDrake, seaDrakeTalons);
            Math.ClearTransitions(seaDrake);
            JsonDataManager.SaveEntity(seaDrake, "Sea-Drake");

            Entity orbOfAffliction = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Green, Color.Black, '/'),
                new Description("Orb of Affliction", "A putrid sphere made from bile and rot. It was created by foul creatures for even fouler purposes. Each use causes it to dry more and more."),
                new Usable(15, 5, 0, "Squeeze", "squeezes the orb, from which putrid bile and filth spew out.", new List<int>() { 1, 2, 3 }),
                new ApplyPoison(10, 5, 0, "Line"),
                new Charges(10),
            });
            JsonDataManager.SaveEntity(orbOfAffliction, "Orb of Affliction");
            */

            Entity scion = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.DarkGreen, Color.Black, 'E'),
                new Description("Scion of Affliction", "A lithe foul humanoid, it is covered in bulging pustules and warts, each seeping with bile. " +
                "Beneath its foul exterior lies a more horrid core, an air of burning trash and scum is about this creature."),
                new InventoryComponent(),
                new Faction("Elemental"),
                new Harmable(),
                new Attributes(20, .8f, 2, 0, 11, 6),
                new Movement(new List<int>() { 1 }),
                new TurnComponent(),
                new ElementalAI(50, 0, 10, 25, 100, 0, 0),
                new Usable(15, 3, 0, "", "spits a fetid ball of bile!", new List<int>() { 1, 2, 3 }),
                new ApplyPoison(3, 1, 3, "Sphere"),
                new SpawnDetails(),
                new SpawnItems(1),
                new SpawnItems(new List<string>() { "Wretched Claw" })
            });
            Entity scionClaw = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Wretched Claw", "A horrid thing."),
                new Equipable(false, "Weapon"),
                new ApplyPoison(3, 1, 0, "Hit"),
                new WeaponComponent(0, 0, 2, 3),
            });

            JsonDataManager.SaveEntity(scionClaw, "Wretched Claw");

            Math.ClearTransitions(scion);
            JsonDataManager.SaveEntity(scion, "Scion of Affliction");

            Entity wandofDigging = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Wand of Digging", "A brown rod with the appearance of rusted iron. The sharpened tip files off with each use."),
                new Usable(3, 0, 0, "Dig", "activates the wand, from which white light blazes from the wand destroying the matter in front of it.", new List<int>() { 0, 1, 2, 3 }),
                new Charges(10),
                new Dig(),
            });

            JsonDataManager.SaveEntity(wandofDigging, "Wand of Digging");

            Entity ringOfGreed = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Gold, Color.Black, '='),
                new Description("Ring of Greed", "A small golden band, it is the last remnant of a creature whose existence was dominated by greed. Donning the ring is a choice one cannot return from. Will you repeat that cycle?"),
                new Equipable(false, "Magic Item"),
                new TrueSight(50, "Item")
            });

            JsonDataManager.SaveEntity(ringOfGreed, "Ring of Greed");

            Entity dagger = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, ')'),
                new Description("Dagger", "A small sharp pointy item."),
                new Equipable(true, "Weapon"),
                new WeaponComponent(2, 0, 1, 4),
            });

            JsonDataManager.SaveEntity(dagger, "Dagger");

            Entity manBeast = new Entity(new List<Component>()
            {
                new Vector(0, 0),
                new Draw(Color.DarkBlue, Color.Black, 'm'),
                new Description("Man Beast", "There is nothing left here, not in this place any longer. All of man has abandoned their creations, " +
                "their monuments left to crumble into the sea. Even still there remains these creatures, " +
                "large and highly intelligent are these bipedal humanoids. Their eyes glint in the dark, watching, waiting."),
                new Actor(),
                new InventoryComponent(),
                new Faction("Evil-Humanoid"),
                new Harmable(),
                new Attributes(15, .8f, 1, 3, 8, 6),
                new Movement(new List<int>() { 1, 2 }),
                new TurnComponent(),
                new BeastAI(25, 0, 10, 25, 100, 10, 0),
                new SpawnDetails(),
                new SpawnItems("Man Beast", 2),
            });

            Tile explosionTrap = new Tile(0, 0, Color.Orange, Color.Black, '^', "Firebomb Trap", "A handmade trap lodged into broken rubble, " +
                "it is clearly not a native part of the tower, be careful not to disturb it.", 1, false);
            explosionTrap.AddComponent(new SpawnDetails());
            explosionTrap.AddComponent(new Trap());
            explosionTrap.AddComponent(new Explode(3, "Step", false));
            explosionTrap.AddComponent(new Attributes(0, 0, 0, 2, 0, 0));

            JsonDataManager.SaveEntity(explosionTrap, "Firebomb Trap");

            Tile fern1 = new Tile(0, 0, Color.LightGreen, Color.Black, (char)244, "Ferns", "A dense patch of ferns.", 1, true);
            Tile fern2 = new Tile(0, 0, Color.Green, Color.Black, (char)244, "Ferns", "A dense patch of ferns.", 1, true);
            Tile fern3 = new Tile(0, 0, Color.DarkGreen, Color.Black, (char)244, "Ferns", "A dense patch of ferns.", 1, true);

            JsonDataManager.SaveEntity(fern1, "Fern 1");
            JsonDataManager.SaveEntity(fern2, "Fern 2");
            JsonDataManager.SaveEntity(fern3, "Fern 3");

            Tile plantClump = new Tile(0, 0, Color.Green, Color.Black, (char)244, "Ferns", "A dense patch of ferns.", 1, true);
            plantClump.AddComponent(new SpawnDetails());
            plantClump.AddComponent(new SpawnTiles(new List<string>() { "Fern 1", "Fern 2", "Fern 3" }, 4));

            JsonDataManager.SaveEntity(plantClump, "Plants");

            Tile water1 = new Tile(0, 0, Color.DarkBlue, Color.Black, (char)247, "Water", "A deep pool.", 2, false);
            Tile water2 = new Tile(0, 0, Color.Blue, Color.Black, (char)247, "Water", "A deep pool.", 2, false);
            Tile water3 = new Tile(0, 0, Color.DarkBlue, Color.Black, '~', "Water", "A deep pool.", 2, false);
            Tile water4 = new Tile(0, 0, Color.Blue, Color.Black, '~', "Water", "A deep pool.", 2, false);

            JsonDataManager.SaveEntity(water1, "Water 1");
            JsonDataManager.SaveEntity(water2, "Water 2");
            JsonDataManager.SaveEntity(water3, "Water 3");
            JsonDataManager.SaveEntity(water4, "Water 4");

            Tile waterpool = new Tile(0, 0, Color.Blue, Color.Black, '~', "Water", "A deep pool.", 2, false);
            waterpool.AddComponent(new SpawnDetails());
            waterpool.AddComponent(new SpawnTiles(new List<string>() { "Water 1", "Water 2", "Water 3", "Water 4" }, 4));

            JsonDataManager.SaveEntity(waterpool, "Pool of Water");


            Entity fish = new Entity(new List<Component>()
            {
                new Actor(),
                new Vector(0, 0),
                new Draw(Color.LightBlue, Color.Black, 'f'),
                new Description("Fish", "A very bland nondescript fish. It is not your friend. You do not feel safe around it."),
                new InventoryComponent(),
                new Faction("Beast"),
                new Harmable(),
                new Attributes(8, .8f, 0, -1, 12, 10),
                new Movement(new List<int>() { 2 }),
                new TurnComponent(),
                new BeastAI(50, 0, 10, 25, 100, 5, 40),
            });
            Entity fishPower = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.White, Color.Black, '/'),
                new Description("Fish Teeth", "Fishy."),
                new Equipable(false, "Weapon"),
                new WeaponComponent(0, 0, 1, 3),
            });
            InventoryManager.EquipItem(fish, fishPower);
            Math.ClearTransitions(fish);
            JsonDataManager.SaveEntity(fish, "Fish");


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

            rootConsole.Children.MoveToTop(loadingConsole);
            CreateConsoleBorder(loadingConsole);

            dungeonGenerator.GenerateTowerFloor();
            dungeonGenerator.PlacePlayer();
            dungeonGenerator.decayChance += 2;

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

            int y = 1;

            for (int i = 0; i < 10; i++)
            {
                if (i < items.Count && items[i] != null)
                {
                    if (i == 9)
                    {
                        this.items.Add(new InventoryDisplaySlot(this, items[i], new Vector(1, y), 28, 3, 0));
                    }
                    else
                    {
                        this.items.Add(new InventoryDisplaySlot(this, items[i], new Vector(1, y), 28, 3, i + 1));
                    }
                }
                else
                {
                    if (i == 9)
                    {
                        this.items.Add(new InventoryDisplaySlot(this, null, new Vector(1, y), 28, 3, 0));
                    }
                    else
                    {
                        this.items.Add(new InventoryDisplaySlot(this, null, new Vector(1, y), 28, 3, i + 1));
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
                            parent.Print(corner.x, corner.y + 1, $"{item.GetComponent<Description>().name} - Equipped".Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
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
                            parent.Print(corner.x, corner.y + 1, $"Unknown Item - Equipped".Align(HorizontalAlignment.Center, width), Color.Black, Color.White);
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
        public bool selected { get; set; } = false;
        public bool equipSelected { get; set; } = false;
        public bool useSelected { get; set; } = false;
        public bool dropSelected { get; set; } = false;
        public bool confirmSelected { get; set; } = false;
        public bool denySelected { get; set; } = false;
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
                confirmSelected = false;
                denySelected = false;
                selected = false;
                equipSelected = false;
                useSelected = false;
                dropSelected = false;

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