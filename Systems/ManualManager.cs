using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana.Systems
{
    public class ManualManager
    {
        public static int selection = 0;
        public static List<ManualEntry> entries = new List<ManualEntry>();
        public static bool isManualOpen = false;
        public ManualManager() 
        {
            entries.Add(new ManualEntry("< Basic Controls >", 
                "Move: [Arrow Keys/Numpad] + " +
                "Open Inventory: [I] + " +
                "Start Looking: [L] + " +
                "Open Manual: [?] + " +
                "Ascend Floor/Path to Staircase: [<] + " +
                "Explore Map Automatically: [A] + " +
                "Cancel Automatic Pathing: [Escape] + " +
                "Get Item at Position: [G] + " +
                "Wait One Turn: [.] +" +
                "Quit Game: [Q] + " +
                "Answer Yes to Prompt: [Y/Enter] + " +
                "Answer No to Prompt: [N/Escape]"));
            entries.Add(new ManualEntry("< Inventory Controls >", 
                "Change Selected Item: [Arrow Keys/Numpad] + " +
                "Drop Selected Item: [D] + " +
                "Equip Selected Item: [E] + " +
                "Use Selected Item: [U] + " +
                "Close Inventory: [Escape/I] + "));
            entries.Add(new ManualEntry("< Look Menu >",
                "This menu allows the player to observe their environment by looking at it. While in this menu a reticle will appear on the screen. " +
                "The menu will display information about whatever object is on the tile the reticle is positioned on. " +
                "Creatures will display over items, and items display over tiles. This order also controls what you look at when the reticle is one a tile. " +
                "For example if the reticle is over a tile which contains a creature and an item, the menu will display the creature first. + + " +
                "Controls: + " +
                "Move Reticle: [Arrow Keys/Numpad] + " +
                "Cancel Look: [Escape/L]"));
            entries.Add(new ManualEntry("< Targeting Menu >",
                "This menu allows the player to select a location when using an item that requires a target. " +
                "Tiles highlighted with Blue*blue show what tiles will be affected by the item selected. " + 
                "Tiles highlighted with Yellow*yellow show tiles that have a creature on them, whether that be an NPC, Enemy, or the Player. " +
                "If any tile is highlighted with Red*red the selection is invalid and you will be unable to confirm it. " +
                "This also applies if the targeted tile is represented with a Red*red Red*X, if this is the case the targeted tile is not visible and it will not allow you to confirm. " +
                "All creatures highlighted with Yellow*yellow will be listed with their map character. " +
                "When confirming a prompt will appear asking to confirm your selection. + + " +
                "Controls: + " +
                "Move Target: [Arrow Keys/Numpad] + " +
                "Close Targeting: [Escape] + " +
                "Confirm Selection and Use Item: [Enter]"));
            entries.Add(new ManualEntry("< Manual Controls >",
                "Move Selected Entry: [Arrow Keys/Numpad] + " +
                "Close Manual: [Escape/?]"));
        }
        public static void OpenManual()
        {
            selection = 0;
            Program.rootConsole.Children.MoveToTop(Program.manualConsole);

            isManualOpen = true;
            Program.player.GetComponent<TurnComponent>().isTurnActive = false;
            CreateManualDisplay();
        }
        public static void CloseManual()
        {
            Program.rootConsole.Children.MoveToBottom(Program.manualConsole);
            Program.manualConsole.Fill(Color.Black, Color.Black);

            isManualOpen = false;
            Program.player.GetComponent<TurnComponent>().isTurnActive = true;
        }
        public static void MoveSelection(bool up)
        {
            if (entries.Count > 1)
            {
                if (!up)
                {
                    selection++;

                    if (selection >= entries.Count) { selection = 0; }
                }
                else
                {
                    selection--;

                    if (selection < 0) { selection = entries.Count - 1; }
                }

                CreateManualDisplay();
            }
        }
        private static void CreateManualDisplay()
        {
            Program.manualConsole.Fill(Color.Black, Color.Black);

            for (int y = 0; y < entries.Count; y++)
            {
                int offsetX = 3;
                int offsetY = (y * 3) + 5;
                if (selection == y)
                {
                    Program.manualConsole.Print(offsetX, offsetY, entries[y].name.Align(HorizontalAlignment.Center, (Program.manualConsole.Width / 2) - 8, '-'), Color.Yellow, Color.Black);

                    Program.manualConsole.SetGlyph(offsetX, offsetY, new ColoredGlyph(Color.Yellow, Color.Black, '>'));
                    Program.manualConsole.SetGlyph((Program.manualConsole.Width / 2) - offsetX * 2, offsetY, new ColoredGlyph(Color.Yellow, Color.Black, '<'));
                }
                else
                {
                    Program.manualConsole.Print(offsetX, offsetY, entries[y].name.Align(HorizontalAlignment.Center, (Program.manualConsole.Width / 2) - 8, '-'), Color.Gray, Color.Black);
                }
            }

            Math.DisplayToConsole(Program.manualConsole, entries[selection].entry, (Program.manualConsole.Width / 2) + 5, 1, 0, 5, false);

            Program.manualConsole.DrawBox(new Rectangle((Program.manualConsole.Width / 2) - 5, 4, 10, Program.manualConsole.Height - 7),
            ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            Program.CreateConsoleBorder(Program.manualConsole);
        }
    }
    public class ManualEntry
    {
        public string name { get; set; }
        public string entry { get; set; }
        public ManualEntry(string name, string entry)
        {
            this.name = name;
            this.entry = entry;
        }
    }
}
