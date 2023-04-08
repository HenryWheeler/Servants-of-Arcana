using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;
using Newtonsoft.Json.Bson;

namespace Servants_of_Arcana
{
    public class InteractionManager
    {
        public static bool popupActive = false;
        public static void CreateConfirmationPrompt(string prompt)
        {
            CreateConfirmationPrompt(new List<string> { prompt });
        }
        public static void CreateConfirmationPrompt(List<string> prompts)
        {
            Program.interactionConsole.Position = new Point((Program.screenWidth / 2) - (Program.interactionWidth / 2), (Program.screenHeight / 2) - (Program.interactionHeight / 2));
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);

            Program.interactionConsole.Fill(Color.Black, Color.Black);

            Program.interactionConsole.DrawBox(new Rectangle(1, 1, Program.interactionConsole.Width - 2, Program.interactionConsole.Height - 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            int baseLength = 11;

            foreach (string prompt in prompts) 
            {
                if (prompt.Length > baseLength - 6)
                {
                    baseLength = prompt.Length + 6;
                }
            }

            Program.interactionConsole.DrawBox(new Rectangle((Program.interactionConsole.Width / 2) - (baseLength / 2), (Program.interactionConsole.Height / 3) - 3, baseLength, Program.interactionConsole.Height / 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black, 177)));

            int startY = Program.interactionConsole.Height / 3;
            foreach (string prompt in prompts)
            {
                Program.interactionConsole.Print((Program.interactionConsole.Width / 2) - ($"< {prompt} >".Length / 2), startY, $"< {prompt} >", Color.Yellow, Color.Black);
                startY += 2;
            }

            Program.interactionConsole.Print((Program.interactionConsole.Width / 2) - ("< Y / N >".Length / 2), (int)(Program.interactionConsole.Height / 1.5f), "< Y / N >", Color.Yellow, Color.Black);
        }
        public static void CreatePopup(string popup)
        {
            CreatePopup(new List<string> { popup });
        }
        public static void CreatePopup(List<string> popups)
        {
            Program.interactionConsole.Position = new Point((Program.screenWidth / 2) - (Program.interactionWidth / 2), (Program.screenHeight / 2) - (Program.interactionHeight / 2));
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);

            Program.interactionConsole.Fill(Color.Black, Color.Black);

            Program.interactionConsole.DrawBox(new Rectangle(1, 1, Program.interactionConsole.Width - 2, Program.interactionConsole.Height - 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            int baseLength = 11;

            foreach (string prompt in popups)
            {
                if (prompt.Length > baseLength - 6)
                {
                    baseLength = prompt.Length + 6;
                }
            }

            Program.interactionConsole.DrawBox(new Rectangle((Program.interactionConsole.Width / 2) - (baseLength / 2), (Program.interactionConsole.Height / 3) - 3, baseLength, Program.interactionConsole.Height / 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black, 177)));

            int startY = Program.interactionConsole.Height / popups.Count + 1;
            foreach (string prompt in popups)
            {
                Program.interactionConsole.Print((Program.interactionConsole.Width / 2) - ($"< {prompt} >".Length / 2), startY, $"< {prompt} >", Color.Yellow, Color.Black);
                startY += 2;
            }
        }
        public static void CreateItemDisplay(Entity item)
        {
            popupActive = true;
            Program.interactionConsole.Position = new Point(((int)(Program.mapWidth * 1.5f) / 2) - (Program.interactionWidth / 2), (int)((Program.mapHeight * 1.5f) / 2) - (Program.interactionHeight / 2));
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);
            Program.interactionConsole.Fill(Color.Black, Color.Black);

            string description = item.GetComponent<Description>().description;

            if (item.GetComponent<Equipable>() != null)
            {
                description += $" + + This item is equipped in your {item.GetComponent<Equipable>().slot}.";
                description += $" + Press Yellow*E to equip it.";
                if (!item.GetComponent<Equipable>().removable)
                {
                    description += " + It cannot be unequipped.";
                }
            }

            if (item.GetComponent<Usable>() != null)
            {
                description += $" + + This item can be used.";
                description += $" + Press Yellow*U to use it.";
            }

            Math.DisplayToConsole(Program.interactionConsole, $"{description}", 2, 1, 0, 2, false);
        }
        public static void ClosePopup()
        {
            popupActive = false;
            Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);
            Program.interactionConsole.Position = new Point((Program.screenWidth / 2) - (Program.interactionWidth / 2), (Program.screenHeight / 2) - (Program.interactionHeight / 2));
        }
    }
}
