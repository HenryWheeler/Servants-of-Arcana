using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class InventoryManager
    {
        public static int selectedItem = 0;
        public static int maxInventorySize = 15;
        public static bool isInventoryOpen = false;
        public static void OpenInventory()
        {
            Program.player.GetComponent<TurnComponent>().isTurnActive = false;

            Program.rootConsole.Children.MoveToTop(Program.inventoryConsole);
            Program.inventoryConsole.Fill(Color.Black, Color.Black);

            selectedItem = 0;
            CreateInventoryDisplay();

            isInventoryOpen = true;
        }
        public static void CloseInventory()
        {
            Program.rootConsole.Children.MoveToTop(Program.playerConsole);
            Program.inventoryConsole.Fill(Color.Black, Color.Black);
            AttributeManager.UpdateAttributes(Program.player);

            Program.player.GetComponent<TurnComponent>().isTurnActive = true;
            isInventoryOpen = false;
        }
        public static void MoveSelection(bool up)
        {
            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();
            if (inventory.items.Count > 1)
            {
                if (!up)
                {
                    selectedItem++;

                    if (selectedItem >= inventory.items.Count) { selectedItem = 0; }
                }
                else
                {
                    selectedItem--;

                    if (selectedItem < 0) { selectedItem = inventory.items.Count - 1; }
                }

                CreateInventoryDisplay();
            }
        }
        private static void CreateInventoryDisplay()
        {
            Program.interactionConsole.Fill(Color.Black, Color.Black);

            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

            for (int y = 0; y < maxInventorySize; y++) 
            {
                if (y < inventory.items.Count && inventory.items[y] != null) 
                {
                    Program.inventoryConsole.DrawLine(new Point(0, y + 5), new Point(Program.inventoryConsole.Width, y + 5), (char)196, Color.Gray);

                    string[] nameParts = inventory.items[y].GetComponent<Description>().name.Split(' ');
                    string name = "";
                    foreach (string part in nameParts)
                    {
                        string[] temp = part.Split('*');
                        if (temp.Length == 1)
                        {
                            name += temp[0] + " ";
                        }
                        else
                        {
                            name += temp[1] + " ";
                        }
                    }
                    int start = (Program.inventoryConsole.Width / 2) - (int)System.Math.Ceiling((double)name.Length / 2);

                    start++;

                    foreach (string part in nameParts)
                    {
                        string[] temp = part.Split('*');
                        if (temp.Length == 1)
                        {
                            Program.inventoryConsole.Print(start, y + 5, temp[0] + " ", Color.White);
                            start += temp[0].Length + 1;
                        }
                        else
                        {
                            Program.inventoryConsole.Print(start, y + 5, temp[1] + " ", Log.StringToColor(temp[0]), Color.Black);
                            start += temp[1].Length + 1;
                        }
                    }
                }
                else if (y < inventory.inventorySize)
                {
                    Program.inventoryConsole.Print(3, y + 5, "< Empty >".Align(HorizontalAlignment.Center, Program.inventoryConsole.Width - 6, (char)196), Color.Gray, Color.Black);
                  
                }
                else if (y >= inventory.inventorySize)
                {
                    Program.inventoryConsole.Print(3, y + 5, "< Beyond Max Capacity >".Align(HorizontalAlignment.Center, Program.inventoryConsole.Width - 6, (char)196), Color.AntiqueWhite, Color.Black);
                }

                if (selectedItem == y)
                {
                    Program.inventoryConsole.SetGlyph(3, y + 5, new ColoredGlyph(Color.Yellow, Color.Black, '>'));
                    Program.inventoryConsole.SetGlyph(Program.inventoryConsole.Width - 4, y + 5, new ColoredGlyph(Color.Yellow, Color.Black, '<'));
                }
            }

            Program.inventoryConsole.DrawLine(new Point(0, maxInventorySize + 7), new Point(Program.inventoryConsole.Width, maxInventorySize + 7), (char)196, Color.AntiqueWhite, Color.Black);

            if (selectedItem < inventory.items.Count && inventory.items[selectedItem] != null)
            {
                string[] nameParts = inventory.items[selectedItem].GetComponent<Description>().name.Split(' ');
                string name = "";
                foreach (string part in nameParts)
                {
                    string[] temp = part.Split('*');
                    if (temp.Length == 1)
                    {
                        name += temp[0] + " ";
                    }
                    else
                    {
                        name += temp[1] + " ";
                    }
                }
                int start = (Program.inventoryConsole.Width / 2) - (int)System.Math.Ceiling((double)name.Length / 2);

                start++;

                foreach (string part in nameParts)
                {
                    string[] temp = part.Split('*');
                    if (temp.Length == 1)
                    {
                        Program.inventoryConsole.Print(start, maxInventorySize + 8, temp[0] + " ", Color.White);
                        start += temp[0].Length + 1;
                    }
                    else
                    {
                        Program.inventoryConsole.Print(start, maxInventorySize + 8, temp[1] + " ", Log.StringToColor(temp[0]), Color.Black);
                        start += temp[1].Length + 1;
                    }
                }

                Program.inventoryConsole.DrawLine(new Point(0, maxInventorySize + 9), new Point(Program.inventoryConsole.Width, maxInventorySize + 9), (char)196, Color.AntiqueWhite, Color.Black);
                Math.DisplayToConsole(Program.inventoryConsole, $"{inventory.items[selectedItem].GetComponent<Description>().description}", 3, 1, 0, maxInventorySize + 11, false);
            }
            else
            {
                Program.inventoryConsole.DrawBox(new Rectangle(0, maxInventorySize + 8, Program.inventoryConsole.Width, Program.inventoryConsole.Height), 
                    ShapeParameters.CreateFilled(new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

                Program.inventoryConsole.Print(0, ((Program.lookConsole.Height / 3) * 2) - 3, " There is no item ".Align(HorizontalAlignment.Center, Program.lookConsole.Width, (char)177), Color.AntiqueWhite);
                Program.inventoryConsole.Print(0, ((Program.lookConsole.Height / 3) * 2) - 1, " selected. ".Align(HorizontalAlignment.Center, Program.lookConsole.Width, (char)177), Color.AntiqueWhite);
            }

            Program.CreateConsoleBorder(Program.inventoryConsole);
        }
        public static void AddToInventory(Entity item, Entity actor)
        {
            actor.GetComponent<InventoryComponent>().items.Add(item);
        }
        public static void GetItem(Entity actor)
        {
            Vector location = actor.GetComponent<Vector>();
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();

            if (Program.tiles[location.x, location.y].item != null && 
                Program.tiles[location.x, location.y].item.GetComponent<Item>() != null && 
                inventory.items.Count < inventory.inventorySize)
            {
                inventory.items.Add(Program.tiles[location.x, location.y].item);

                if (actor.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"You pick up the {Program.tiles[location.x, location.y].item.GetComponent<Description>().name}.");
                }

                Program.tiles[location.x, location.y].item = null;
                actor.GetComponent<TurnComponent>().EndTurn();
            }
            else if (actor.GetComponent<PlayerController>() != null)
            {
                Log.Add($"There is nothing there for you to pick up.");
            }
        }
        public static void DropItem(Entity actor, Entity item = null)
        {
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
            Vector vector = actor.GetComponent<Vector>();
            if (inventory.items.Count > 0)
            {
                if (item == null)
                {
                    item = inventory.items[selectedItem];
                }

                if (Program.tiles[vector.x, vector.y].item == null)
                {
                    if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped && item.GetComponent<Equipable>().unequipable)
                    {
                        Log.Add($"{actor.GetComponent<Description>().name} cannot drop the {item.GetComponent<Description>().name} because it is unequipable.");
                        return;
                    }

                    inventory.items.Remove(item);
                    Program.tiles[vector.x, vector.y].item = item;

                    if (actor.GetComponent<PlayerController>() != null)
                    {
                        Log.Add($"{actor.GetComponent<Description>().name} dropped the {item.GetComponent<Description>().name}");
                        CloseInventory();
                    }

                    actor.GetComponent<TurnComponent>().EndTurn();
                }
                else
                {
                    Log.Add($"{actor.GetComponent<Description>().name} cannot drop the {item.GetComponent<Description>().name} because there is something already there.");
                }
            }
            else
            {
                if (actor.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"You have no items to drop.");
                }
            }
        }
    }
}
