using Newtonsoft.Json.Linq;
using SadConsole.Components;
using SadConsole.Entities;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Input;
using SadRogue.Primitives;
using System.Numerics;
using Servants_of_Arcana.Systems;
using System.Security.AccessControl;

namespace Servants_of_Arcana
{
    public class PlayerController : Controller
    {
        public bool autoPathing = false;
        public bool pathingToTarget = false;
        public Vector lastPosition { get; set; }
        public override void Execute()
        {
            if (Program.player != entity)
            {
                Program.player = entity;
                Program.UpdateNewPlayer();
            }
        }
        public override void SetDelegates() { }
        public PlayerController() { }
    }
    class KeyboardComponent : KeyboardConsoleComponent
    {
        public bool confirming = false;
        private event Action keyboardEvent;
        public override void ProcessKeyboard(IScreenObject host, Keyboard info, out bool handled)
        {
            if (confirming)
            {
                if (info.IsKeyPressed(Keys.Y))
                {
                    Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);

                    confirming = false;
                    keyboardEvent.Invoke();
                    keyboardEvent = null;
                }
                else if (info.IsKeyPressed(Keys.Enter))
                {
                    Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);

                    confirming = false;
                    keyboardEvent.Invoke();
                    keyboardEvent = null;
                }
                else if (info.IsKeyPressed(Keys.N))
                {
                    Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);

                    confirming = false;
                    keyboardEvent = null;
                }
                else if (info.IsKeyPressed(Keys.Escape))
                {
                    Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);

                    confirming = false;
                    keyboardEvent = null;
                }
            }
            else if (Program.isGameActive)
            {
                if (Program.player.GetComponent<TurnComponent>().isTurnActive)
                {
                    PlayerController controller = Program.player.GetComponent<PlayerController>();
                    if (controller.autoPathing)
                    {
                        if (info.IsKeyPressed(Keys.Escape)) 
                        {
                            confirming = true;
                            keyboardEvent += CancelPathing;
                            InteractionManager.CreateConfirmationPrompt("Cancel Pathing?");
                        }
                        else
                        {
                            List<Vector> unexploredTiles = new List<Vector>();

                            for (int x = 2; x < Program.gameWidth - 4; x++)
                            {
                                for (int y = 2; y < Program.gameHeight - 4; y++)
                                {
                                    Tile tile = Program.tiles[x, y];
                                    if (tile != null && !tile.GetComponent<Visibility>().explored)
                                    {
                                        Vector vector = tile.GetComponent<Vector>();
                                        unexploredTiles.Add(vector);
                                    }
                                }
                            }

                            DijkstraMap.CreateMap(unexploredTiles, "Player-Autopath");

                            Vector newPosition = DijkstraMap.PathFromMap(Program.player, "Player-Autopath");
                            Program.player.GetComponent<Movement>().Move(newPosition);

                            if (controller.lastPosition == newPosition)
                            {
                                controller.autoPathing = false;
                            }
                            controller.lastPosition = newPosition;
                        }
                    }
                    else if (controller.pathingToTarget)
                    {
                        if (info.IsKeyPressed(Keys.Escape)) 
                        {
                            confirming = true;
                            keyboardEvent += CancelPathing;
                            InteractionManager.CreateConfirmationPrompt("Cancel Pathing?");
                        }
                        else
                        {
                            Vector newPosition = DijkstraMap.PathFromMap(Program.player, "Player-Autopath");
                            Program.player.GetComponent<Movement>().Move(newPosition);

                            if (controller.lastPosition == newPosition)
                            {
                                controller.pathingToTarget = false;
                            }
                            controller.lastPosition = newPosition;
                        }
                    }
                    else
                    {
                        if (info.IsKeyPressed(Keys.Up)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(0, -1))); }
                        else if (info.IsKeyPressed(Keys.Down)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(0, 1))); }
                        else if (info.IsKeyPressed(Keys.Left)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(-1, 0))); }
                        else if (info.IsKeyPressed(Keys.Right)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(1, 0))); }
                        else if (info.IsKeyPressed(Keys.NumPad8)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(0, -1))); }
                        else if (info.IsKeyPressed(Keys.NumPad9)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(1, -1))); }
                        else if (info.IsKeyPressed(Keys.NumPad6)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(1, 0))); }
                        else if (info.IsKeyPressed(Keys.NumPad3)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(1, 1))); }
                        else if (info.IsKeyPressed(Keys.NumPad2)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(0, 1))); }
                        else if (info.IsKeyPressed(Keys.NumPad1)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(-1, 1))); }
                        else if (info.IsKeyPressed(Keys.NumPad4)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(-1, 0))); }
                        else if (info.IsKeyPressed(Keys.NumPad7)) { Program.player.GetComponent<Movement>().Move(new Vector(Program.player.GetComponent<Vector>(), new Vector(-1, -1))); }
                        else if (info.IsKeyDown(Keys.LeftShift) && info.IsKeyPressed(Keys.OemPeriod))
                        {
                            Vector vector2 = Program.player.GetComponent<Vector>();
                            if (Program.tiles[vector2.x, vector2.y].GetComponent<Draw>().character == '>')
                            {
                                confirming = true;
                                keyboardEvent += DescendFloor;

                                InteractionManager.CreateConfirmationPrompt($"Descend to a lower floor?");
                            }
                            else if (Program.tiles[DungeonGenerator.stairSpot.x, DungeonGenerator.stairSpot.y].GetComponent<Visibility>().explored)
                            {
                                confirming = true;
                                keyboardEvent += PathToStairs;

                                InteractionManager.CreateConfirmationPrompt($"Begin pathing to stairs?");
                            }
                        }
                        else if (info.IsKeyPressed(Keys.OemPeriod)) { Program.player.GetComponent<TurnComponent>().EndTurn(); }
                        else if (info.IsKeyPressed(Keys.L)) { Look.StartLooking(); }
                        else if (info.IsKeyPressed(Keys.I)) { InventoryManager.OpenInventory(); }
                        //else if (info.IsKeyPressed(Keys.E)) { InventoryManager.OpenEquipment(); }
                        else if (info.IsKeyPressed(Keys.G)) { InventoryManager.GetItem(Program.player); }
                        else if (info.IsKeyPressed(Keys.Q)) 
                        {
                            confirming = true;
                            keyboardEvent += QuitGame;

                            InteractionManager.CreateConfirmationPrompt($"Quit game?");
                        }
                        else if (info.IsKeyPressed(Keys.V))
                        {
                            ShadowcastFOV.RevealAll();
                        }
                        else if (info.IsKeyPressed(Keys.B))
                        {
                            ShadowcastFOV.HideAll();
                        }
                        else if (info.IsKeyPressed(Keys.A))
                        {
                            confirming = true;
                            keyboardEvent += BeginAutoPath;

                            InteractionManager.CreateConfirmationPrompt($"Begin automatic pathing?");
                        }
                        else if (info.IsKeyPressed(Keys.OemQuestion))
                        {
                            ManualManager.OpenManual();
                        }
                    }
                }
                else if (Look.looking)
                {
                    if (info.IsKeyPressed(Keys.Up)) { Look.MoveReticle(new Vector(0, -1)); }
                    else if (info.IsKeyPressed(Keys.Down)) { Look.MoveReticle(new Vector(0, 1)); }
                    else if (info.IsKeyPressed(Keys.Left)) { Look.MoveReticle(new Vector(-1, 0)); }
                    else if (info.IsKeyPressed(Keys.Right)) { Look.MoveReticle(new Vector(1, 0)); }
                    else if (info.IsKeyPressed(Keys.NumPad8)) { Look.MoveReticle(new Vector(0, -1)); }
                    else if (info.IsKeyPressed(Keys.NumPad9)) { Look.MoveReticle(new Vector(1, -1)); }
                    else if (info.IsKeyPressed(Keys.NumPad6)) { Look.MoveReticle(new Vector(1, 0)); }
                    else if (info.IsKeyPressed(Keys.NumPad3)) { Look.MoveReticle(new Vector(1, 1)); }
                    else if (info.IsKeyPressed(Keys.NumPad2)) { Look.MoveReticle(new Vector(0, 1)); }
                    else if (info.IsKeyPressed(Keys.NumPad1)) { Look.MoveReticle(new Vector(-1, 1)); }
                    else if (info.IsKeyPressed(Keys.NumPad4)) { Look.MoveReticle(new Vector(-1, 0)); }
                    else if (info.IsKeyPressed(Keys.NumPad7)) { Look.MoveReticle(new Vector(-1, -1)); }
                    else if (info.IsKeyPressed(Keys.L)) { Look.StopLooking(); }
                    else if (info.IsKeyPressed(Keys.Escape)) { Look.StopLooking(); }
                }
                else if (InventoryManager.isInventoryOpen)
                {
                    if (info.IsKeyPressed(Keys.I)) { InventoryManager.CloseInventory(); }
                    else if (info.IsKeyPressed(Keys.Escape)) { InventoryManager.CloseInventory(); }
                    else if (info.IsKeyPressed(Keys.Up)) { InventoryManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.Down)) { InventoryManager.MoveSelection(false); ; }
                    else if (info.IsKeyPressed(Keys.Left)) { InventoryManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.Right)) { InventoryManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad8)) { InventoryManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.NumPad9)) { InventoryManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad6)) { InventoryManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad3)) { InventoryManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad2)) { InventoryManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad1)) { InventoryManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.NumPad4)) { InventoryManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.NumPad7)) { InventoryManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.E)) 
                    {
                        InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

                        if (inventory.items.Count > 0 && InventoryManager.selectedItem < inventory.items.Count) 
                        {
                            if (inventory.items[InventoryManager.selectedItem].GetComponent<Equipable>() != null)
                            {
                                Equipable equipable = inventory.items[InventoryManager.selectedItem].GetComponent<Equipable>();

                                if (equipable.equipped)
                                {
                                    if (!equipable.unequipable)
                                    {
                                        Log.Add($"You cannot unequip the {equipable.entity.GetComponent<Description>().name}.");
                                    }
                                    else
                                    {
                                        confirming = true;
                                        keyboardEvent += UnequipItem;

                                        InteractionManager.CreateConfirmationPrompt($"Unequip the {equipable.entity.GetComponent<Description>().name}?");
                                    }
                                }
                                else
                                {
                                    if (inventory.ReturnSlot(equipable.slot).item == null)
                                    {
                                        confirming = true;
                                        keyboardEvent += EquipItem;

                                        InteractionManager.CreateConfirmationPrompt($"Equip the {equipable.entity.GetComponent<Description>().name}?");
                                    }
                                    else
                                    {
                                        if (!inventory.ReturnSlot(equipable.slot).item.GetComponent<Equipable>().unequipable)
                                        {
                                            Log.Add($"You cannot equip the {inventory.items[InventoryManager.selectedItem].GetComponent<Description>().name} because the " +
                                                $"{inventory.ReturnSlot(equipable.slot).item.GetComponent<Description>().name} is unequipable.");
                                        }
                                        else
                                        {
                                            confirming = true;
                                            keyboardEvent += EquipItem;

                                            InteractionManager.CreateConfirmationPrompt(new List<string>() { $"The {inventory.ReturnSlot(equipable.slot).item.GetComponent<Description>().name} is already equipped.", 
                                                $"Equip the {equipable.entity.GetComponent<Description>().name} anyways?" } );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Add($"You cannot equip the {inventory.items[InventoryManager.selectedItem].GetComponent<Description>().name}.");
                            }
                        }
                    }
                    else if (info.IsKeyPressed(Keys.D))
                    {
                        InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

                        if (inventory.items.Count > 0 && InventoryManager.selectedItem < inventory.items.Count)
                        {
                            if (inventory.items[InventoryManager.selectedItem].GetComponent<Equipable>() != null &&
                                inventory.items[InventoryManager.selectedItem].GetComponent<Equipable>().equipped)
                            {
                                confirming = true;
                                keyboardEvent += DropItem;

                                InteractionManager.CreateConfirmationPrompt(new List<string>() { $"The {inventory.items[InventoryManager.selectedItem].GetComponent<Description>().name} is equipped.", "Drop anyways?" });
                            }
                            else
                            {
                                confirming = true;
                                keyboardEvent += DropItem;

                                InteractionManager.CreateConfirmationPrompt($"Drop the {inventory.items[InventoryManager.selectedItem].GetComponent<Description>().name}?");
                            }
                        }
                    }
                    else if (info.IsKeyPressed(Keys.U))
                    {
                        InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

                        if (inventory.items.Count > 0 && InventoryManager.selectedItem < inventory.items.Count)
                        {
                            if (inventory.items[InventoryManager.selectedItem].GetComponent<Usable>() != null)
                            {
                                confirming = true;
                                keyboardEvent += UseItem;

                                InteractionManager.CreateConfirmationPrompt($"Use the {inventory.items[InventoryManager.selectedItem].GetComponent<Description>().name}?");
                            }
                            else
                            {
                                Log.Add($"You cannot use the {inventory.items[InventoryManager.selectedItem].GetComponent<Description>().name}.");
                            }
                        }
                    }
                }
                else if (ManualManager.isManualOpen)
                {
                    if (info.IsKeyPressed(Keys.Escape)) { ManualManager.CloseManual(); }
                    else if (info.IsKeyPressed(Keys.OemQuestion)) { ManualManager.CloseManual(); }
                    else if (info.IsKeyPressed(Keys.Up)) { ManualManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.Down)) { ManualManager.MoveSelection(false); ; }
                    else if (info.IsKeyPressed(Keys.Left)) { ManualManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.Right)) { ManualManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad8)) { ManualManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.NumPad9)) { ManualManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad6)) { ManualManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad3)) { ManualManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad2)) { ManualManager.MoveSelection(false); }
                    else if (info.IsKeyPressed(Keys.NumPad1)) { ManualManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.NumPad4)) { ManualManager.MoveSelection(true); }
                    else if (info.IsKeyPressed(Keys.NumPad7)) { ManualManager.MoveSelection(true); }
                }
            }
            else
            {
                if (info.IsKeyPressed(Keys.N)) 
                {
                    confirming = true;
                    keyboardEvent += Program.StartNewGame;

                    InteractionManager.CreateConfirmationPrompt($"Start a new game?");
                }
                else if (info.IsKeyPressed(Keys.Q))
                {
                    confirming = true;
                    keyboardEvent += QuitGame;

                    InteractionManager.CreateConfirmationPrompt($"Quit game?");
                }
                //else if (info.IsKeyPressed(Keys.L) && SaveDataManager.savePresent) { SaveDataManager.LoadSave(); }
                //else if (info.IsKeyPressed(Keys.Q)) { Program.ExitProgram(); }
            }
            handled = true;
        }
        public void EquipItem()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            InventoryManager.EquipItem(Program.player, inventory.items[InventoryManager.selectedItem]);
        }
        public void UnequipItem()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            InventoryManager.UnequipItem(Program.player, inventory.items[InventoryManager.selectedItem]);
        }
        public void DropItem()
        {
            InventoryManager.DropItem(Program.player);
        }
        public void UseItem()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            InventoryManager.UseItem(Program.player, inventory.items[InventoryManager.selectedItem], Program.player.GetComponent<Vector>());
        }
        public void CancelPathing()
        {
            PlayerController controller = Program.player.GetComponent<PlayerController>();
            controller.autoPathing = false;
            controller.pathingToTarget = false;
        }
        public void BeginAutoPath()
        {
            PlayerController controller = Program.player.GetComponent<PlayerController>();
            controller.autoPathing = true;
        }
        public void PathToStairs()
        {
            PlayerController controller = Program.player.GetComponent<PlayerController>();
            DijkstraMap.CreateMap(new List<Vector>() { DungeonGenerator.stairSpot }, "Player-Autopath");
            controller.pathingToTarget = true;
        }
        public void DescendFloor()
        {
            Program.GenerateNewFloor();
        }
        public void QuitGame()
        {
            Program.isGameActive = false;
            System.Environment.Exit(1);
        }
    }
}
