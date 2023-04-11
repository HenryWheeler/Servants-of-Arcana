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

namespace Servants_of_Arcana
{
    public class PlayerController : Controller
    {
        public bool autoPathing = false;
        public bool pathingToTarget = false;
        public bool confirming = false;
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
        public event Action keyboardEvent;
        public void Confirm()
        {
            Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);

            confirming = false;
            if (Program.player != null)
            {
                Program.player.GetComponent<PlayerController>().confirming = false;
            }
            keyboardEvent.Invoke();

            ClearEvent();
        }
        public void Deny()
        {
            Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);

            confirming = false;
            if (Program.player != null)
            {
                Program.player.GetComponent<PlayerController>().confirming = false;
            }

            if (InteractionManager.popupActive)
            {
                InteractionManager.CreateItemDisplay(InventoryManager.selectedItem);
            }

            ClearEvent();
        }
        public void ClearEvent()
        {
            keyboardEvent = null;
        }
        public override void ProcessKeyboard(IScreenObject host, Keyboard info, out bool handled)
        {
            if (confirming)
            {
                if (info.IsKeyPressed(Keys.Y))
                {
                    Confirm();
                }
                else if (info.IsKeyPressed(Keys.Enter))
                {
                    Confirm();
                }
                else if (info.IsKeyPressed(Keys.N))
                {
                    Deny();
                }
                else if (info.IsKeyPressed(Keys.Escape))
                {
                    Deny();
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
                            Program.player.GetComponent<PlayerController>().confirming = true;
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
                                    if (tile != null && tile.terrainType == 1 && !tile.GetComponent<Visibility>().explored)
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
                            Program.player.GetComponent<PlayerController>().confirming = true;
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
                        else if (info.IsKeyPressed(Keys.NumPad5)) { Program.player.GetComponent<TurnComponent>().EndTurn(); }
                        else if (info.IsKeyDown(Keys.LeftShift) && info.IsKeyPressed(Keys.OemComma))
                        {
                            Vector vector2 = Program.player.GetComponent<Vector>();
                            if (Program.tiles[vector2.x, vector2.y].GetComponent<Draw>().character == '<')
                            {
                                confirming = true;
                                Program.player.GetComponent<PlayerController>().confirming = true;
                                keyboardEvent += DescendFloor;

                                InteractionManager.CreateConfirmationPrompt($"Ascend to a higher floor?");
                            }
                            else if (Program.tiles[DungeonGenerator.stairSpot.x, DungeonGenerator.stairSpot.y].GetComponent<Visibility>().explored)
                            {
                                confirming = true;
                                Program.player.GetComponent<PlayerController>().confirming = true;
                                keyboardEvent += PathToStairs;

                                InteractionManager.CreateConfirmationPrompt($"Begin pathing to stairs?");
                            }
                        }
                        else if (info.IsKeyPressed(Keys.OemPeriod)) { Program.player.GetComponent<TurnComponent>().EndTurn(); }
                        else if (info.IsKeyPressed(Keys.L)) { Look.StartLooking(); }
                        else if (info.IsKeyPressed(Keys.G)) { InventoryManager.GetItem(Program.player); }
                        else if (info.IsKeyPressed(Keys.Q)) 
                        {
                            confirming = true;
                            Program.player.GetComponent<PlayerController>().confirming = true;
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
                            Program.player.GetComponent<PlayerController>().confirming = true;
                            keyboardEvent += BeginAutoPath;

                            InteractionManager.CreateConfirmationPrompt($"Begin automatic pathing?");
                        }
                        else if (info.IsKeyDown(Keys.LeftShift) && info.IsKeyPressed(Keys.OemQuestion))
                        {
                            ManualManager.OpenManual();
                        }
                        else if (info.IsKeyPressed(Keys.D1))
                        {
                            Program.inventoryConsole.SelectItem(0);
                        }
                        else if (info.IsKeyPressed(Keys.D2))
                        {
                            Program.inventoryConsole.SelectItem(1);
                        }
                        else if (info.IsKeyPressed(Keys.D3))
                        {
                            Program.inventoryConsole.SelectItem(2);
                        }
                        else if (info.IsKeyPressed(Keys.D4))
                        {
                            Program.inventoryConsole.SelectItem(3);
                        }
                        else if (info.IsKeyPressed(Keys.D5))
                        {
                            Program.inventoryConsole.SelectItem(4);
                        }
                        else if (info.IsKeyPressed(Keys.D6))
                        {
                            Program.inventoryConsole.SelectItem(5);
                        }
                        else if (info.IsKeyPressed(Keys.D7))
                        {
                            Program.inventoryConsole.SelectItem(6);
                        }
                        else if (info.IsKeyPressed(Keys.D8))
                        {
                            Program.inventoryConsole.SelectItem(7);
                        }
                        else if (info.IsKeyPressed(Keys.D9))
                        {
                            Program.inventoryConsole.SelectItem(8);
                        }
                        else if (info.IsKeyPressed(Keys.D0))
                        {
                            Program.inventoryConsole.SelectItem(9);
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
                    if (info.IsKeyPressed(Keys.Escape)) { InventoryManager.CloseInventoryDisplay(); }
                    else if (info.IsKeyPressed(Keys.E)) 
                    {
                        Equip();
                    }
                    else if (info.IsKeyPressed(Keys.D))
                    {
                        Drop();
                    }
                    else if (info.IsKeyPressed(Keys.U))
                    {
                        Use();
                    }
                    else if (info.IsKeyPressed(Keys.D1))
                    {
                        Program.inventoryConsole.SelectItem(0);
                    }
                    else if (info.IsKeyPressed(Keys.D2))
                    {
                        Program.inventoryConsole.SelectItem(1);
                    }
                    else if (info.IsKeyPressed(Keys.D3))
                    {
                        Program.inventoryConsole.SelectItem(2);
                    }
                    else if (info.IsKeyPressed(Keys.D4))
                    {
                        Program.inventoryConsole.SelectItem(3);
                    }
                    else if (info.IsKeyPressed(Keys.D5))
                    {
                        Program.inventoryConsole.SelectItem(4);
                    }
                    else if (info.IsKeyPressed(Keys.D6))
                    {
                        Program.inventoryConsole.SelectItem(5);
                    }
                    else if (info.IsKeyPressed(Keys.D7))
                    {
                        Program.inventoryConsole.SelectItem(6);
                    }
                    else if (info.IsKeyPressed(Keys.D8))
                    {
                        Program.inventoryConsole.SelectItem(7);
                    }
                    else if (info.IsKeyPressed(Keys.D9))
                    {
                        Program.inventoryConsole.SelectItem(8);
                    }
                    else if (info.IsKeyPressed(Keys.D0))
                    {
                        Program.inventoryConsole.SelectItem(9);
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
                else if (TargetingSystem.isTargeting)
                {
                    if (info.IsKeyPressed(Keys.Escape))
                    {
                        confirming = true;
                        Program.player.GetComponent<PlayerController>().confirming = true;
                        keyboardEvent += StopTargeting;

                        InteractionManager.CreateConfirmationPrompt($"Stop targeting and return to inventory?");
                    }
                    if (info.IsKeyPressed(Keys.Enter))
                    {
                        if (TargetingSystem.isTargetValid)
                        {
                            if (TargetingSystem.requiredTargetsMet)
                            {
                                confirming = true;
                                Program.player.GetComponent<PlayerController>().confirming = true;
                                keyboardEvent += TargetingSystemUseItem;

                                InteractionManager.CreateConfirmationPrompt($"Use the {TargetingSystem.currentUsedItem.entity.GetComponent<Description>().name}?");
                            }
                            else
                            {
                                Log.Add($"You cannot use this item as it requires {TargetingSystem.currentUsedItem.requiredTargets} targets.");
                            }
                        }
                        else
                        {
                            Log.Add($"You are unable to target this tile with this item.");
                        }
                    }
                    if (info.IsKeyPressed(Keys.Up)) { TargetingSystem.MoveReticle(new Vector(0, -1)); }
                    else if (info.IsKeyPressed(Keys.Down)) { TargetingSystem.MoveReticle(new Vector(0, 1)); }
                    else if (info.IsKeyPressed(Keys.Left)) { TargetingSystem.MoveReticle(new Vector(-1, 0)); }
                    else if (info.IsKeyPressed(Keys.Right)) { TargetingSystem.MoveReticle(new Vector(1, 0)); }
                    else if (info.IsKeyPressed(Keys.NumPad8)) { TargetingSystem.MoveReticle(new Vector(0, -1)); }
                    else if (info.IsKeyPressed(Keys.NumPad9)) { TargetingSystem.MoveReticle(new Vector(1, -1)); }
                    else if (info.IsKeyPressed(Keys.NumPad6)) { TargetingSystem.MoveReticle(new Vector(1, 0)); }
                    else if (info.IsKeyPressed(Keys.NumPad3)) { TargetingSystem.MoveReticle(new Vector(1, 1)); }
                    else if (info.IsKeyPressed(Keys.NumPad2)) { TargetingSystem.MoveReticle(new Vector(0, 1)); }
                    else if (info.IsKeyPressed(Keys.NumPad1)) { TargetingSystem.MoveReticle(new Vector(-1, 1)); }
                    else if (info.IsKeyPressed(Keys.NumPad4)) { TargetingSystem.MoveReticle(new Vector(-1, 0)); }
                    else if (info.IsKeyPressed(Keys.NumPad7)) { TargetingSystem.MoveReticle(new Vector(-1, -1)); }
                }
            }
            else if (Program.isPlayerCreatingCharacter)
            {
                if (info.IsKeyPressed(Keys.Enter))
                {
                    if (Program.playerName.Length > 0)
                    {
                        confirming = true;
                        keyboardEvent += Program.StartNewGame;

                        InteractionManager.CreateConfirmationPrompt($"Is this your new name?");
                    }
                }
                else if (info.IsKeyPressed(Keys.Back))
                {
                    if (Program.playerName.Length > 0)
                    {
                        Program.playerName = Program.playerName.Remove(Program.playerName.Length - 1);
                    }
                    InteractionManager.CharacterCreationDisplay();
                }
                else if (Program.playerName.Length <= 19)
                {
                    if (info.IsKeyPressed(Keys.Space))
                    {
                        if (Program.playerName.Length > 0)
                        {
                            Program.playerName += " ";
                        }
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.OemMinus))
                    {
                        Program.playerName += "-";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.OemComma))
                    {
                        Program.playerName += ",";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    if (info.IsKeyDown(Keys.LeftShift))
                    {
                        if (info.IsKeyPressed(Keys.A))
                        {
                            Program.playerName += "A";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.B))
                        {
                            Program.playerName += "B";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.C))
                        {
                            Program.playerName += "C";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.D))
                        {
                            Program.playerName += "D";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.E))
                        {
                            Program.playerName += "E";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.F))
                        {
                            Program.playerName += "F";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.G))
                        {
                            Program.playerName += "G";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.H))
                        {
                            Program.playerName += "H";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.I))
                        {
                            Program.playerName += "I";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.J))
                        {
                            Program.playerName += "J";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.K))
                        {
                            Program.playerName += "K";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.L))
                        {
                            Program.playerName += "L";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.M))
                        {
                            Program.playerName += "M";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.N))
                        {
                            Program.playerName += "N";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.O))
                        {
                            Program.playerName += "O";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.P))
                        {
                            Program.playerName += "P";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.Q))
                        {
                            Program.playerName += "Q";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.R))
                        {
                            Program.playerName += "R";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.S))
                        {
                            Program.playerName += "S";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.T))
                        {
                            Program.playerName += "T";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.U))
                        {
                            Program.playerName += "U";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.V))
                        {
                            Program.playerName += "V";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.W))
                        {
                            Program.playerName += "W";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.X))
                        {
                            Program.playerName += "X";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.Y))
                        {
                            Program.playerName += "Y";
                            InteractionManager.CharacterCreationDisplay();
                        }
                        else if (info.IsKeyPressed(Keys.Z))
                        {
                            Program.playerName += "Z";
                            InteractionManager.CharacterCreationDisplay();
                        }
                    }
                    else if (info.IsKeyPressed(Keys.A))
                    {
                        Program.playerName += "a";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.B))
                    {
                        Program.playerName += "b";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.C))
                    {
                        Program.playerName += "c";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.D))
                    {
                        Program.playerName += "d";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.E))
                    {
                        Program.playerName += "e";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.F))
                    {
                        Program.playerName += "f";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.G))
                    {
                        Program.playerName += "g";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.H))
                    {
                        Program.playerName += "h";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.I))
                    {
                        Program.playerName += "i";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.J))
                    {
                        Program.playerName += "j";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.K))
                    {
                        Program.playerName += "k";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.L))
                    {
                        Program.playerName += "l";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.M))
                    {
                        Program.playerName += "m";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.N))
                    {
                        Program.playerName += "n";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.O))
                    {
                        Program.playerName += "o";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.P))
                    {
                        Program.playerName += "p";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.Q))
                    {
                        Program.playerName += "q";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.R))
                    {
                        Program.playerName += "r";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.S))
                    {
                        Program.playerName += "s";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.T))
                    {
                        Program.playerName += "t";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.U))
                    {
                        Program.playerName += "u";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.V))
                    {
                        Program.playerName += "v";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.W))
                    {
                        Program.playerName += "w";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.X))
                    {
                        Program.playerName += "x";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.Y))
                    {
                        Program.playerName += "y";
                        InteractionManager.CharacterCreationDisplay();
                    }
                    else if (info.IsKeyPressed(Keys.Z))
                    {
                        Program.playerName += "z";
                        InteractionManager.CharacterCreationDisplay();
                    }
                }
            }
            else
            {
                if (info.IsKeyPressed(Keys.N)) 
                {
                    confirming = true;
                    keyboardEvent += Program.StartCharacterCreation;

                    InteractionManager.CreateConfirmationPrompt($"Start a new game?");
                }
                else if (info.IsKeyPressed(Keys.Q))
                {
                    confirming = true;
                    keyboardEvent += QuitGame;

                    InteractionManager.CreateConfirmationPrompt($"Quit game?");
                }
            }
            handled = true;
        }
        public void Equip()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

            if (inventory.items.Count > 0)
            {
                if (InventoryManager.selectedItem.GetComponent<Equipable>() != null)
                {
                    Equipable equipable = InventoryManager.selectedItem.GetComponent<Equipable>();

                    if (equipable.equipped)
                    {
                        if (!equipable.removable)
                        {
                            Log.Add($"You cannot unequip the {equipable.entity.GetComponent<Description>().name}.");
                        }
                        else
                        {
                            confirming = true;
                            Program.player.GetComponent<PlayerController>().confirming = true;
                            keyboardEvent += UnequipItem;

                            InteractionManager.CreateConfirmationPrompt($"Unequip the {equipable.entity.GetComponent<Description>().name}?");
                        }
                    }
                    else
                    {
                        if (inventory.ReturnSlot(equipable.slot).item == null)
                        {
                            confirming = true;
                            Program.player.GetComponent<PlayerController>().confirming = true;
                            keyboardEvent += EquipItem;

                            InteractionManager.CreateConfirmationPrompt($"Equip the {equipable.entity.GetComponent<Description>().name}?");
                        }
                        else
                        {
                            if (!inventory.ReturnSlot(equipable.slot).item.GetComponent<Equipable>().removable)
                            {
                                Log.Add($"You cannot equip the {InventoryManager.selectedItem.GetComponent<Description>().name} because the " +
                                    $"{inventory.ReturnSlot(equipable.slot).item.GetComponent<Description>().name} is unequipable.");
                            }
                            else
                            {
                                confirming = true;
                                Program.player.GetComponent<PlayerController>().confirming = true;
                                keyboardEvent += EquipItem;

                                InteractionManager.CreateConfirmationPrompt(new List<string>() { $"The {inventory.ReturnSlot(equipable.slot).item.GetComponent<Description>().name} is already equipped.",
                                                $"Equip the {equipable.entity.GetComponent<Description>().name} anyways?" });
                            }
                        }
                    }
                }
                else
                {
                    Log.Add($"You cannot equip the {InventoryManager.selectedItem.GetComponent<Description>().name}.");
                }
            }
        }
        public void Use()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

            if (inventory.items.Count > 0)
            {
                if (InventoryManager.selectedItem.GetComponent<Usable>() != null)
                {
                    confirming = true;
                    Program.player.GetComponent<PlayerController>().confirming = true;
                    keyboardEvent += UseItem;

                    InteractionManager.CreateConfirmationPrompt($"{InventoryManager.selectedItem.GetComponent<Usable>().action} the {InventoryManager.selectedItem.GetComponent<Description>().name}?");
                }
                else
                {
                    Log.Add($"You cannot use the {InventoryManager.selectedItem.GetComponent<Description>().name}.");
                }
            }
        }
        public void Drop()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

            if (inventory.items.Count > 0)
            {
                if (InventoryManager.selectedItem.GetComponent<Equipable>() != null &&
                        InventoryManager.selectedItem.GetComponent<Equipable>().equipped)
                {
                    confirming = true;
                    Program.player.GetComponent<PlayerController>().confirming = true;
                    keyboardEvent += DropItem;

                    InteractionManager.CreateConfirmationPrompt(new List<string>() { $"The {InventoryManager.selectedItem.GetComponent<Description>().name} is equipped.", "Drop anyways?" });
                }
                else
                {
                    confirming = true;
                    Program.player.GetComponent<PlayerController>().confirming = true;
                    keyboardEvent += DropItem;

                    InteractionManager.CreateConfirmationPrompt($"Drop the {InventoryManager.selectedItem.GetComponent<Description>().name}?");
                }
            }
        }
        public void EquipItem()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            InventoryManager.EquipItem(Program.player, InventoryManager.selectedItem);
        }
        public void UnequipItem()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            InventoryManager.UnequipItem(Program.player, InventoryManager.selectedItem);
        }
        public void DropItem()
        {
            InventoryManager.DropItem(Program.player);
        }
        public void UseItem()
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            InventoryManager.UseItem(Program.player, InventoryManager.selectedItem, Program.player.GetComponent<Vector>());
        }
        public void TargetingSystemUseItem()
        {
            TargetingSystem.UseSelectedItem();
        }
        public void StopTargeting()
        {
            TargetingSystem.StopTargeting(true);
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
