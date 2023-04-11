using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;
using SadConsole.Entities;

namespace Servants_of_Arcana
{
    public class InventoryManager
    {
        public static int maxInventorySize = 10;
        public static bool isInventoryOpen = false;
        public static Entity selectedItem = null;
        public static void OpenInventoryDisplay(Entity item)
        {
            Program.player.GetComponent<TurnComponent>().isTurnActive = false;
            InteractionManager.CreateItemDisplay(item);
            selectedItem = item;
            isInventoryOpen = true;
        }
        public static void CloseInventoryDisplay()
        {
            InteractionManager.ClosePopup();
            AttributeManager.UpdateAttributes(Program.player);
            Program.player.GetComponent<TurnComponent>().isTurnActive = true;
            selectedItem = null;
            isInventoryOpen = false;
        }
        public static void AddToInventory(Entity item, Entity actor)
        {
            actor.GetComponent<InventoryComponent>().items.Add(item);
            AttributeManager.UpdateAttributes(actor);
        }
        public static void GetItem(Entity actor)
        {
            Vector location = actor.GetComponent<Vector>();
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();

            if (Program.tiles[location.x, location.y].item != null && 
                Program.tiles[location.x, location.y].item.GetComponent<Item>() != null)
            {
                if (inventory.items.Count < maxInventorySize)
                {
                    inventory.items.Add(Program.tiles[location.x, location.y].item);

                    if (actor.GetComponent<PlayerController>() != null)
                    {
                        Log.Add($"{actor.GetComponent<Description>().name} picks up the {Program.tiles[location.x, location.y].item.GetComponent<Description>().name}.");
                    }

                    Program.tiles[location.x, location.y].item = null;
                    actor.GetComponent<TurnComponent>().EndTurn();
                }
                else if (actor.GetComponent<PlayerController>() != null)
                {
                    Log.Add($"{actor.GetComponent<Description>().name} is holding too much to pick this up.");
                }
            }
            else if (actor.GetComponent<PlayerController>() != null)
            {
                Log.Add($"There is nothing for {actor.GetComponent<Description>().name} to pick up.");
            }

            AttributeManager.UpdateAttributes(actor);
        }
        public static void DropItem(Entity actor, Entity item = null)
        {
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
            Vector vector = actor.GetComponent<Vector>();
            if (inventory.items.Count > 0)
            {
                if (item == null)
                {
                    item = selectedItem;
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
                        CloseInventoryDisplay();
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
                    Log.Add($"{actor.GetComponent<Description>().name} has no items to drop.");
                }
                else
                {
                    actor.GetComponent<TurnComponent>().EndTurn();
                }
            }

            AttributeManager.UpdateAttributes(actor);
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
            equipable.onEquip?.Invoke(actor, true);


            if (actor.GetComponent<PlayerController>() != null)
            {
                CloseInventoryDisplay();
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

            AttributeManager.UpdateAttributes(actor);
        }
        public static void UnequipItem(Entity actor, Entity item, bool endTurn = true) 
        {
            Equipable equipable = item.GetComponent<Equipable>();
            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
            EquipmentSlot slot = inventory.ReturnSlot(equipable.slot);

            slot.item = null;
            equipable.equipped = false;
            equipable.onEquip?.Invoke(actor, false);

            if (endTurn) 
            {
                if (actor.GetComponent<PlayerController>() != null)
                {
                    CloseInventoryDisplay();
                }

                Log.Add($"{actor.GetComponent<Description>().name} unequips the {item.GetComponent<Description>().name}.");
                actor.GetComponent<TurnComponent>().EndTurn();
            }

            AttributeManager.UpdateAttributes(actor);
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

                        if (item.GetComponent<Charges>() != null)
                        {
                            item.GetComponent<Charges>().chargesRemaining--;
                            if (item.GetComponent<Charges>().chargesRemaining <= 0)
                            {
                                if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                                {
                                    UnequipItem(actor, item, false);
                                }
                                actor.GetComponent<InventoryComponent>().items.Remove(item);
                                Log.Add($"The {item.GetComponent<Description>().name} is spent!");
                            }
                        }

                        CloseInventoryDisplay();
                    }
                    else
                    {
                        CloseInventoryDisplay();
                        TargetingSystem.BeginTargeting(use);
                        return;
                    }
                }
                else
                {
                    //For now default to actor position
                    use.Use(actor, position);

                    if (item.GetComponent<Charges>() != null)
                    {
                        item.GetComponent<Charges>().chargesRemaining--;
                        if (item.GetComponent<Charges>().chargesRemaining <= 0)
                        {
                            if (item.GetComponent<Equipable>() != null && item.GetComponent<Equipable>().equipped)
                            {
                                UnequipItem(actor, item, false);
                            }

                            actor.GetComponent<InventoryComponent>().items.Remove(item);
                            Log.Add($"The {item.GetComponent<Description>().name} is spent!");
                        }
                    }
                }

                actor.GetComponent<TurnComponent>().EndTurn();
            }
            else if (actor.GetComponent<PlayerController>() == null) 
            {
                actor.GetComponent<TurnComponent>().EndTurn();
            }

            AttributeManager.UpdateAttributes(actor);
        }
    }
}
