using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class PrayerManager
    {
        public static bool isTerminalActive = false;
        public static Altar currentTerminal { get; set; } 
        public static List<string> terminalOptions = new List<string>()
        { "Identify truth.", "Cleanse your wounds.", "Reveal your goal.", "Reveal what harms you." };
        public static string firstOption { get; set; }
        public static string secondOption { get; set; }
        public static void PrayAtAltar(Altar terminal)
        {
            Program.player.GetComponent<TurnComponent>().isTurnActive = false;
            InteractionManager.CreateAltarDisplay(terminal);
            currentTerminal = terminal;
            isTerminalActive = true;

            firstOption = terminal.options[0];
            secondOption = terminal.options[1];
        }
        public static void UseAltar(bool first)
        {
            string option;
            if (first) 
            {
                option = firstOption;
            }
            else
            {
                option = secondOption;
            }

            Log.Add($"{Program.playerName} prays to its god!");

            switch (option)
            {
                case "Identify truth.":
                    {
                        List<string> items = new List<string>();
                        foreach (Entity item in Program.player.GetComponent<InventoryComponent>().items)
                        {
                            if (!ItemIdentityManager.IsItemIdentified(item.GetComponent<Description>().name))
                            {
                                items.Add(item.GetComponent<Description>().name);
                            }
                        }

                        if (items.Count > 0) 
                        {
                            foreach (string item in items) 
                            {
                                ItemIdentityManager.IdentifyItem(item);
                            }

                            Log.Add($"{Program.playerName} experiences a flash of insight.");
                        }
                        else
                        {
                            Log.Add($"The altar rejects {Program.playerName}'s request. {Program.playerName} does not meet the requirements for its request.");
                            CloseAltar();
                            return;
                        }
                        break;
                    }
                case "Cleanse your wounds.":
                    {
                        Attributes attributes = Program.player.GetComponent<Attributes>();
                        if (attributes.health + attributes.maxHealth / 2 > attributes.maxHealth) { attributes.health = attributes.maxHealth; }
                        else { attributes.health += attributes.maxHealth / 2; }

                        Log.Add($"{Program.playerName}'s wounds are healed and it is villed with vigor!");

                        break;
                    }
                case "Reveal your goal.":
                    {
                        Vector position = Program.player.GetComponent<Vector>();
                        List<Node> nodes;
                        if (Program.tiles[DungeonGenerator.keySpot.x, DungeonGenerator.keySpot.y].item != null)
                        {
                            nodes = AStar.ReturnPath(position, DungeonGenerator.keySpot);
                        }
                        else
                        {
                            nodes = AStar.ReturnPath(position, DungeonGenerator.stairSpot);
                        }

                        if (nodes != null)
                        {
                            foreach (Node node in nodes) 
                            {
                                Vector vector = node.position;
                                int distance = (int)Math.Distance(position.x, position.y, vector.x, vector.y);
                                ParticleManager.CreateParticle(true, vector, distance + 5, 4, "None", new Draw(Color.Yellow, Color.Black, '*'),
                                    null, true, true);
                                Program.tiles[vector.x, vector.y].GetComponent<Visibility>().explored = true;
                            }
                        }

                        Log.Add($"{Program.playerName} experiences a flash of insight.");

                        break;
                    }
                case "Reveal what harms you.":
                    {
                        for (int x = 0; x < Program.mapWidth; x++)
                        {
                            for (int y = 0; y < Program.mapHeight; y++)
                            {
                                if (Math.CheckBounds(x, y) && Program.tiles[x, y].GetComponent<Trap>() != null)
                                {
                                    Program.tiles[x, y].GetComponent<Trap>().Reveal();
                                }
                            }
                        }

                        Log.Add($"{Program.playerName} experiences a flash of insight.");

                        break;
                    }
            }

            currentTerminal.active = false;
            currentTerminal.entity.GetComponent<Draw>().fColor = Color.Gray;

            CloseAltar();
            Program.player.GetComponent<TurnComponent>().EndTurn();
        }
        public static void CloseAltar() 
        {
            InteractionManager.ClosePopup();
            AttributeManager.UpdateAttributes(Program.player);
            Program.player.GetComponent<TurnComponent>().isTurnActive = true;
            currentTerminal = null;
            isTerminalActive = false;
        }
    }
}
