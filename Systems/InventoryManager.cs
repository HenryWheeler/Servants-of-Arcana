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
        public static int maxInventorySize = 11;
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
            Program.rootConsole.Children.MoveToTop(Program.mapConsole);
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

                    if (selectedItem >= maxInventorySize) { selectedItem = 0; }
                }
                else
                {
                    selectedItem--;

                    if (selectedItem < 0) { selectedItem = maxInventorySize - 1; }
                }

                CreateInventoryDisplay();
            }
        }
        private static void CreateInventoryDisplay()
        {
            Program.inventoryConsole.Fill(Color.Black, Color.Black);

            InventoryComponent inventory = Program.player.GetComponent<InventoryComponent>();

            for (int y = 0; y < maxInventorySize; y++) 
            {
                int offsetY = (y * 3) + 5;

                if (y < inventory.items.Count && inventory.items[y] != null) 
                {
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

                    if (inventory.items[y].GetComponent<Equipable>() != null && inventory.items[y].GetComponent<Equipable>().equipped)
                    {
                        name += "- Equipped ";
                    }

                    if (selectedItem == y)
                    {
                        Program.inventoryConsole.Print(3, offsetY, $"< {name}>".Align(HorizontalAlignment.Center, (Program.inventoryConsole.Width / 2) - 6, (char)196), Color.Yellow, Color.Black);
                        Program.inventoryConsole.SetGlyph(3, offsetY, new ColoredGlyph(Color.Yellow, Color.Black, '>'));
                        Program.inventoryConsole.SetGlyph((Program.inventoryConsole.Width / 2) - 7, offsetY, new ColoredGlyph(Color.Yellow, Color.Black, '<'));
                    }
                    else
                    {

                        Program.inventoryConsole.Print(3, offsetY, $"< {name}>".Align(HorizontalAlignment.Center, (Program.inventoryConsole.Width / 2) - 6, (char)196), Color.Gray, Color.Black);
                    }
                }
                else if (y < maxInventorySize)
                {
                    if (selectedItem == y)
                    {
                        Program.inventoryConsole.Print(3, offsetY, "< Empty >".Align(HorizontalAlignment.Center, (Program.inventoryConsole.Width / 2) - 6, (char)196), Color.Yellow, Color.Black);
                        Program.inventoryConsole.SetGlyph(3, offsetY, new ColoredGlyph(Color.Yellow, Color.Black, '>'));
                        Program.inventoryConsole.SetGlyph((Program.inventoryConsole.Width / 2) - 7, offsetY, new ColoredGlyph(Color.Yellow, Color.Black, '<'));
                    }
                    else
                    {
                        Program.inventoryConsole.Print(3, offsetY, "< Empty >".Align(HorizontalAlignment.Center, (Program.inventoryConsole.Width / 2) - 6, (char)196), Color.Gray, Color.Black);
                    }
                }
            }

            if (selectedItem < inventory.items.Count && inventory.items[selectedItem] != null)
            {
                Math.DisplayToConsole(Program.inventoryConsole, $"{inventory.items[selectedItem].GetComponent<Description>().name}", (Program.inventoryConsole.Width / 2) + 7, 1, 0, 5, false);

                Program.inventoryConsole.DrawLine(new Point((Program.inventoryConsole.Width / 2), 7), new Point(Program.inventoryConsole.Width, 7), (char)196, Color.AntiqueWhite, Color.Black);

                string description = inventory.items[selectedItem].GetComponent<Description>().description;
                
                if (inventory.items[selectedItem].GetComponent<Equipable>() != null)
                {
                    description += $" + This item can be equipped in your {inventory.items[selectedItem].GetComponent<Equipable>().slot}.";
                    if (!inventory.items[selectedItem].GetComponent<Equipable>().removable)
                    {
                        description += "It cannot be unequipped.";
                    }
                }

                Math.DisplayToConsole(Program.inventoryConsole, $"{description}", (Program.inventoryConsole.Width / 2) + 7, 1, 0, 9, false);
            }
            else
            {
                Program.inventoryConsole.DrawBox(new Rectangle((Program.inventoryConsole.Width / 2) + 7, 4, Program.inventoryConsole.Width / 2 - 10, Program.inventoryConsole.Height - 7),
                    ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));
                Program.CreateConsoleBorder(Program.inventoryConsole);

                Program.inventoryConsole.Print((Program.inventoryConsole.Width / 2) + 10, Program.inventoryConsole.Height / 2, " There is no item selected. ".Align(HorizontalAlignment.Center, Program.lookConsole.Width, (char)177), Color.AntiqueWhite);
            }

            Program.inventoryConsole.DrawBox(new Rectangle((Program.inventoryConsole.Width / 2) - 6, 4, 13, Program.inventoryConsole.Height - 7),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));
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
                inventory.items.Count < maxInventorySize)
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
                    if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                    {
                        if (!item.GetComponent<Equipable>().removable)
                        {
                            Log.Add($"{actor.GetComponent<Description>().name} cannot drop the {item.GetComponent<Description>().name} because it is unequipable.");
                            return;
                        }
                        else
                        {
                            item.GetComponent<Equipable>().equipped = false;
                            UnequipItem(actor, item, false);
                        }
                    }

                    inventory.items.Remove(item);
                    Program.tiles[vector.x, vector.y].item = item;

                    if (actor.GetComponent<PlayerController>() != null)
                    {
                        Log.Add($"{actor.GetComponent<Description>().name} dropped the {item.GetComponent<Description>().name}.");
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
                else
                {
                    actor.GetComponent<TurnComponent>().EndTurn();
                }
            }
        }
        public static void EquipItem(Entity actor, Entity item, bool endTurn = true)
        {
            Equipable equipable = item.GetComponent<Equipable>();
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
            EquipmentSlot slot = inventory.ReturnSlot(equipable.slot);

            if (slot.item != null)
            {
                UnequipItem(actor, slot.item, false);
            }

            slot.item = item;
            equipable.equipped = true;

            if (actor.GetComponent<PlayerController>() != null)
            {
                CloseInventory();
                Log.Add($"You equipped the {slot.item.GetComponent<Description>().name}.");
            }
            else
            {
                Log.Add($"The {actor.GetComponent<Description>().name} equips the {slot.item.GetComponent<Description>().name}.");
            }

            if (endTurn)
            {
                actor.GetComponent<TurnComponent>().EndTurn();
            }
        }
        public static void UnequipItem(Entity actor, Entity item, bool endTurn = true) 
        {
            Equipable equipable = item.GetComponent<Equipable>();
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
            EquipmentSlot slot = inventory.ReturnSlot(equipable.slot);

            slot.item = null;
            equipable.equipped = false;

            if (actor.GetComponent<PlayerController>() != null)
            {
                CloseInventory();
                Log.Add($"You unequiped the {item.GetComponent<Description>().name}.");
            }
            else
            {
                Log.Add($"The {actor.GetComponent<Description>().name} unequip the {item.GetComponent<Description>().name}.");
            }

            if (endTurn) 
            {
                actor.GetComponent<TurnComponent>().EndTurn();
            }
        }
        public static void UseItem(Entity actor, Entity item, Vector position)
        {
            Usable use = item.GetComponent<Usable>();
            if (use != null)
            {
                if (actor.GetComponent<PlayerController>() != null)
                {
                    //If range is above zero enter targeting mode, otherwise automatically use the item

                    if (use.range == 0)
                    {
                        use.Use(actor, position);

                        if (item.GetComponent<Consumable>() != null)
                        {
                            actor.GetComponent<InventoryComponent>().items.Remove(item);
                        }

                        CloseInventory();

                        Log.Add($"{actor.GetComponent<Description>().name} {use.action} the {item.GetComponent<Description>().name}!");
                    }
                    else
                    {
                        CloseInventory();
                        TargetingSystem.BeginTargeting(use);
                    }
                }
                else
                {
                    //For now default to actor position
                    use.Use(actor, position);

                    if (item.GetComponent<Consumable>() != null)
                    {
                        actor.GetComponent<InventoryComponent>().items.Remove(item);
                    }

                    Log.Add($"The {actor.GetComponent<Description>().name} {use.action}s the {item.GetComponent<Description>().name}!");
                }

                actor.GetComponent<TurnComponent>().EndTurn();
            }
            else if (actor.GetComponent<PlayerController>() == null) 
            {
                actor.GetComponent<TurnComponent>().EndTurn();
            }
        }
    }
}
