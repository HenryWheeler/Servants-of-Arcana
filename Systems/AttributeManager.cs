using SadConsole.Entities;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Servants_of_Arcana
{
    public class AttributeManager
    {
        private static string spacer { get; set; }
        public AttributeManager()
        {
            spacer = " + ";
        }
        public static void UpdateAttributes(Entity entity)
        {
            Program.playerConsole.Clear();

            Attributes stats = entity.GetComponent<Attributes>();

            string display = "";

            display += $"Red*Health: {stats.health}/{stats.maxHealth}{spacer}";

            display += $"Yellow*Speed: {stats.maxEnergy}{spacer}";
            display += $"Orange*Strength: {stats.strength}{spacer}";
            display += $"Cyan*Intelligence: {stats.intelligence}{spacer}";

            display += $"LightGray*Av: {stats.armorValue}{spacer}";
            display += $"LightGray*Dv: {stats.dodgeValue}{spacer}";

            display += $"Sight: {stats.sight}{spacer}";

            display += $"Depth: {Program.depth}{spacer}";

            Vector vector = entity.GetComponent<Vector>();
            if (Program.tiles[vector.x, vector.y].item != null)
            {
                display += $"{spacer}{spacer}{spacer}You are standing on a {Program.tiles[vector.x, vector.y].item.GetComponent<Description>().name}";
            }

            Math.DisplayToConsole(Program.playerConsole, display, 2, 2, 1, 5);
            Program.CreateConsoleBorder(Program.playerConsole, true);
        }
    }
}
