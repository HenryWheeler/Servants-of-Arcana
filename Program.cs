using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Entities;
using SadRogue.Primitives;
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
        public static bool loadingScreen { get; set; } = true;
        public static Action onParticleEmpty;

        //The Size of the root console
        public static int screenWidth = 100;
        public static int screenHeight = 55;

        //The size of the map console
        public static int mapWidth = 70;
        public static int mapHeight = 40;

        public static int interactionWidth = 65;
        public static int interactionHeight = 40;

        public static int itemDisplayWidth = 65;
        public static int itemDisplayHeight = 40;

        //The size of the ingame map
        public static int gameWidth = 100;
        public static int gameHeight = 100;

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
        public static int depth { get; set; } = 0;

        public static Tile[,] tiles = new Tile[gameWidth, gameHeight];
        public static Entity[,] sfx = new Entity[gameWidth, gameHeight];
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
            //Settings.f

            rootConsole = new RootConsole(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);

            mapConsole = new TitleConsole("< Map >", mapWidth, mapHeight) { Position = new Point(0, 0) };
            logConsole = new TitleConsole("< Message Log >", messageWidth, messageHeight) { Position = new Point(0, mapHeight) };
            playerConsole = new TitleConsole("< The Rogue @ >", rogueWidth, rogueHeight) { Position = new Point(mapWidth, 0) };
            inventoryConsole = new TitleConsole("< Inventory >", inventoryWidth, inventoryHeight) { Position = new Point(0, 0) };
            targetConsole = new TitleConsole("< Targeting >", targetWidth, targetHeight) { Position = new Point(mapWidth, 0) };
            lookConsole = new TitleConsole("< Looking >", lookWidth, lookHeight) { Position = new Point(mapWidth, 0) };
            interactionConsole = new TitleConsole("< Interaction >", interactionWidth, interactionHeight) { Position = new Point(interactionWidth - (screenWidth / 2), interactionHeight - (int)(screenHeight / 1.5f)) };
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
            player.AddComponent(new Attributes(20, 1f, 1, 1, 10, 10, 10));
            player.AddComponent(new TurnComponent());
            player.AddComponent(new Movement(new List<int> { 1, 2 }));
            player.AddComponent(new InventoryComponent());
            player.AddComponent(new Actor());
            player.AddComponent(new PlayerController());
            player.AddComponent(new Faction("Player"));
            player.AddComponent(new Harmable());

            GenerateNewFloor();

            UpdateNewPlayer();

            TurnManager.AddActor(player.GetComponent<TurnComponent>());
            player.GetComponent<TurnComponent>().StartTurn();

            Entity testMagicMap = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Cyan, Color.Black, '?'),
                new Description("Scroll of Mapping", "An ancient poem that tells the story of a great cartographer."),
                new Usable(0),
                new Consumable(),
                new MagicMap(),
            });

            Entity healingPotion = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Red, Color.Black, '!'),
                new Description("Potion of Healthy Habits", "A thick ochre brew that feels hot to the touch."),
                new Usable(0),
                new Consumable(),
                new Heal(10),
            });

            Entity strengthPotion = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Orange, Color.Black, '!'),
                new Description("Potion of Mighty Strength", "Less of a brew and more of a thick stew is this orange concoction."),
                new Usable(0),
                new Consumable(),
                new IncreaseAttribute(1, "Strength"),
            });

            Entity smartPotion = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Blue, Color.Black, '!'),
                new Description("Potion of Bookish Quality", "A vial of blue fluid, just smelling it makes you feel smarter."),
                new Usable(0),
                new Consumable(),
                new IncreaseAttribute(1, "Intelligence"),
            });

            Entity speedPotion = new Entity(new List<Component>()
            {
                new Item(),
                new Vector(0, 0),
                new Draw(Color.Yellow, Color.Black, '!'),
                new Description("Potion of Quick Movements", "A bubbling brew of pale yellow fluid."),
                new Usable(0),
                new Consumable(),
                new IncreaseAttribute(1, "Speed"),
            });

            JsonDataManager.SaveEntity(testMagicMap, "Scroll of Mapping");
            JsonDataManager.SaveEntity(healingPotion, "Potion of Healthy Habits");
            JsonDataManager.SaveEntity(strengthPotion, "Potion of Mighty Strength");
            JsonDataManager.SaveEntity(smartPotion, "Potion of Bookish Quality");
            JsonDataManager.SaveEntity(speedPotion, "Potion of Quick Movements");

            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Scroll of Mapping"), player);
            }
            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Potion of Healthy Habits"), player);
            }
            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Potion of Mighty Strength"), player);
            }
            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Potion of Bookish Quality"), player);
            }
            for (int i = 0; i < 2; i++)
            {
                InventoryManager.AddToInventory(JsonDataManager.ReturnEntity("Potion of Quick Movements"), player);
            }

            isGameActive = true;
        }
        public static void GenerateNewFloor(Random seed = null)
        {
            depth++;
            
            switch (depth)
            {
                case 1:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.LightGray, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 2:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.LightBlue, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 3:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.SteelBlue, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 4:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.Blue, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 5:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.BlueViolet, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 6:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.DarkViolet, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 7:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.Violet, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
                case 8:
                    {
                        dungeonGenerator = new DungeonGenerator(new Draw[] { new Draw(Color.Brown, Color.Black, '.') }, new Description("Stone Floor", "A simple stone floor."), new Draw[] { new Draw(Color.PaleVioletRed, Color.Black, (char)177) }, new Description("Stone Wall", "A simple stone wall."), seed);
                        dungeonGenerator.GenerateDungeon();
                        break;
                    }
            }

            dungeonGenerator.PlacePlayer();

            //ParticleEffects.RevealNewFloor();
        }
        public static void PlayerDeath(Entity killingEntity)
        {
            foreach (TurnComponent component in TurnManager.entities)
            {
                component.isAlive = false;
                component.isTurnActive = false;
            }
            TurnManager.entities.Clear();
            isGameActive = false;

            depth = 0;
            ShadowcastFOV.RevealAll();
            ParticleEffects.FloorFadeAway();

            Particle particle = CreateSFX(player.GetComponent<Vector>(), new Draw[] { new Draw(Color.Black, Color.Black, (char)0) }, 35, "None", 3);

            onParticleEmpty += CreateDeathMessage;
        }
        public static void CreateDeathMessage()
        {
            string deathMessage = " < You have died. > ";
            int baseLength = deathMessage.Length + 2;

            mapConsole.DrawBox(new Rectangle(3, 4, mapConsole.Width - 6, mapConsole.Height - 7),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            mapConsole.DrawBox(new Rectangle((mapConsole.Width / 2) - (baseLength / 2), (mapConsole.Height / 3) - 3, baseLength, mapConsole.Height / 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black, 177)));

            int startY = mapConsole.Height / 2;
            mapConsole.Print((mapConsole.Width / 2) - ($"{deathMessage}".Length / 2), startY, $"{deathMessage}", Color.Yellow, Color.Black);

            CreateConsoleBorder(mapConsole);

            onParticleEmpty -= CreateDeathMessage;
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

                    CreateSFX(new Vector(chosenTile, 0), new Draw[] { new Draw(Color.Gray, Color.Black, (char)177) }, 100, "WanderNorth", 2);
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
        public static Particle CreateSFX(Vector position, Draw[] draw, int life, string direction, int threshold, Vector target = null)
        {
            Particle particle = new Particle(life, direction, threshold, draw, target);
            particle.AddComponent(new Vector(position.x, position.y));
            particle.AddComponent(draw[0]);

            rootConsole.particles.Add(particle);
            return particle;
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
        public List<Particle> particles = new List<Particle>();
        public override void Update(TimeSpan delta)
        {
            base.Update(delta);

            if (particles.Count > 0)
            {
                Program.ClearSFX();

                for (int i = 0; i < particles.Count; i++)
                {
                    Particle particle = particles[i];
                    particle?.Progress();
                }

                if (particles.Count == 0)
                {
                    Program.ClearSFX();
                    Program.DrawMap();
                    Program.onParticleEmpty?.Invoke();
                }
                else
                {
                    Program.DrawMap();
                }
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
    public class Particle : Entity
    {
        public int life { get; set; }
        public string direction { get; set; }
        public int threshold { get; set; }
        public int currentThreshold = 0;
        public Draw[] particles { get; set; }
        public int currentParticle = 0;
        public bool aboveSight { get; set; } = false;
        public Vector trackedTarget = null;
        public Action<Vector> onParticleDeath;
        public void Progress()
        {
            currentThreshold--;

            Vector position = GetComponent<Vector>();

            if (currentThreshold <= 0)
            {
                //The kind of movement a particle will display.
                switch (direction)
                {
                    case "Attached":
                        {
                            if (trackedTarget != null)
                            {
                                GetComponent<Vector>().x = trackedTarget.x;
                                GetComponent<Vector>().y = trackedTarget.y;
                            }
                            break;
                        }
                    case "Target":
                        {
                            Vector newPosition = DijkstraMap.PathFromMap(this, "ParticlePath");
                            GetComponent<Vector>().x = newPosition.x;
                            GetComponent<Vector>().y = newPosition.y;
                            break;
                        }
                    case "Wander":
                        {
                            position.x += Program.random.Next(-1, 2);
                            position.y += Program.random.Next(-1, 2);
                            break;
                        }
                    case "None": { break; }
                    case "North":
                        {
                            position.y--;
                            break;
                        }
                    case "NorthEast":
                        {
                            position.x--;
                            position.y--;
                            break;
                        }
                    case "East":
                        {
                            position.x--;
                            break;
                        }
                    case "SouthEast":
                        {
                            position.x--;
                            position.y++;
                            break;
                        }
                    case "South":
                        {
                            position.y++;
                            break;
                        }
                    case "SouthWest":
                        {
                            position.x++;
                            position.y++;
                            break;
                        }
                    case "West":
                        {
                            position.x++;
                            break;
                        }
                    case "NorthWest":
                        {
                            position.x++;
                            position.y--;
                            break;
                        }
                    case "WanderNorth":
                        {
                            position.x += Program.random.Next(-1, 2);
                            position.y += Program.random.Next(-1, 0);
                            break;
                        }
                    case "WanderNorthEast":
                        {
                            position.x += Program.random.Next(-1, 0);
                            position.y += Program.random.Next(-1, 0);
                            break;
                        }
                    case "WanderEast":
                        {
                            position.x += Program.random.Next(-1, 0);
                            position.y += Program.random.Next(-1, 2);
                            break;
                        }
                    case "WanderSouthEast":
                        {
                            position.x += Program.random.Next(-1, 0);
                            position.y += Program.random.Next(0, 2);
                            break;
                        }
                    case "WanderSouth":
                        {
                            position.x += Program.random.Next(-1, 2);
                            position.y += Program.random.Next(0, 2);
                            break;
                        }
                    case "WanderSouthWest":
                        {
                            position.x += Program.random.Next(0, 2);
                            position.y += Program.random.Next(0, 2);
                            break;
                        }
                    case "WanderWest":
                        {
                            position.x += Program.random.Next(0, 2);
                            position.y += Program.random.Next(-1, 2);
                            break;
                        }
                    case "WanderNorthWest":
                        {
                            position.x += Program.random.Next(0, 2);
                            position.y += Program.random.Next(-1, 0);
                            break;
                        }
                }
            }

            if (currentThreshold <= 0)
            {
                currentThreshold = threshold;

                if (currentParticle < particles.Length - 1) { currentParticle++; }
                else { currentParticle = 0; }

                Draw draw = GetComponent<Draw>();
                draw.character = particles[currentParticle].character;
                draw.fColor = particles[currentParticle].fColor;
                draw.bColor = particles[currentParticle].bColor;

                life--;
                if (life <= 0)
                {
                    KillParticle();
                    return;
                }
            }

            if (Math.CheckBounds(position.x, position.y))
            {
                Program.sfx[position.x, position.y] = this;
            }
        }
        public void KillParticle()
        {
            Program.rootConsole.particles.Remove(this);
            onParticleDeath?.Invoke(GetComponent<Vector>());
        }
        public Particle(int life, string direction, int threshold, Draw[] particles, Vector target = null, bool aboveSight = false)
        {
            this.life = life;
            this.direction = direction;
            this.threshold = threshold;
            this.particles = particles;
            this.aboveSight = aboveSight;

            if (target != null)
            {
                if (direction == "Target")
                {
                    DijkstraMap.CreateMap(new List<Vector>() { target }, "ParticlePath");
                }
                else if (direction == "Attached")
                {
                    trackedTarget = target;
                }
            }
        }
    }
}