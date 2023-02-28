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

namespace Servants_of_Arcana
{
    public class PlayerController : Controller
    {
        public override void Execute()
        {
            if (Program.player != entity)
            {
                Program.player = entity;
            }
        }
        public PlayerController() { }
    }
    class KeyboardComponent : KeyboardConsoleComponent
    {
        public override void ProcessKeyboard(IScreenObject host, Keyboard info, out bool handled)
        {
            if (Program.isGameActive)
            {
                if (Program.player.GetComponent<TurnComponent>().isTurnActive)
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
                    else if (info.IsKeyPressed(Keys.OemPlus))
                    {
                        Vector vector2 = Program.player.GetComponent<Vector>();
                        if (Program.tiles[vector2.x, vector2.y].GetComponent<Draw>().character == '>') { Program.GenerateNewFloor(); }
                    }
                    else if (info.IsKeyPressed(Keys.LeftShift) && info.IsKeyPressed(Keys.OemPeriod))
                    {
                        Vector vector2 = Program.player.GetComponent<Vector>();
                        if (Program.tiles[vector2.x, vector2.y].GetComponent<Draw>().character == '<') { Program.GenerateNewFloor(); }
                    }
                    else if (info.IsKeyPressed(Keys.OemPeriod)) { Program.player.GetComponent<TurnComponent>().EndTurn(); }
                    else if (info.IsKeyPressed(Keys.L)) { Look.StartLooking(); }
                    else if (info.IsKeyPressed(Keys.I)) { InventoryManager.OpenInventory(); }
                    //else if (info.IsKeyPressed(Keys.E)) { InventoryManager.OpenEquipment(); }
                    else if (info.IsKeyPressed(Keys.G)) { InventoryManager.GetItem(Program.player); }
                    //else if (info.IsKeyPressed(Keys.J)) { SaveDataManager.CreateSave(); Program.ExitProgram(); }
                    else if (info.IsKeyPressed(Keys.V))
                    {
                        ShadowcastFOV.RevealAll();
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
                    else if (info.IsKeyPressed(Keys.D)) { InventoryManager.DropItem(Program.player); }
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
                }
            }
            else
            {
                if (info.IsKeyPressed(Keys.N)) { Program.StartNewGame(); }
                //else if (info.IsKeyPressed(Keys.L) && SaveDataManager.savePresent) { SaveDataManager.LoadSave(); }
                //else if (info.IsKeyPressed(Keys.Q)) { Program.ExitProgram(); }
            }
            handled = true;
        }
    }
}
