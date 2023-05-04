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

            display += $"{Program.playerName}:{spacer}";
            display += $"Red*Health: {stats.health}/{stats.maxHealth}{spacer}";

            display += $"Yellow*Speed: {MathF.Round(stats.maxEnergy, 2)}{spacer}";
            display += $"Orange*Strength: {stats.strength}{spacer}";
            display += $"Cyan*Intelligence: {stats.intelligence}{spacer}";

            display += $"LightGray*Av: {stats.armorValue}{spacer}";

            display += $"Floor: {Program.floor}{spacer}";

            display += $"Status: {spacer} {stats.status?.Invoke()}";

            display += $"{spacer}Manual: Yellow*?.";

            Program.inventoryConsole.UpdateLists(entity.GetComponent<InventoryComponent>().items);

            Math.DisplayToConsole(Program.playerConsole, display, 2, 0);
            Program.rootConsole.Children.MoveToTop(Program.playerConsole);
            Program.rootConsole.Children.MoveToTop(Program.inventoryConsole);
        }
    }
}
