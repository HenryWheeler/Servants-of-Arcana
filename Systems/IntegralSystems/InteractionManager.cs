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
        public static Vector confirmPromptPosition { get; set; }
        public static Vector denyPromptPosition { get; set; }
        public static bool canUse { get; set; } = false;
        public static bool canEquip { get; set; } = false;
        public static bool canUnequip { get; set; } = false;
        public static bool canDrop { get; set; } = false;
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

            confirmPromptPosition = new Vector((Program.interactionConsole.Width / 3) + ($"< {baseLength} >".Length / 2), (int)(Program.interactionConsole.Height / 1.5f) - 1);
            denyPromptPosition = new Vector((int)(Program.interactionConsole.Width / 1.5f) - ($"< {baseLength} >".Length / 2), (int)(Program.interactionConsole.Height / 1.5f) - 1);
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
        public static void CharacterCreationDisplay()
        {
            Program.titleConsole.Fill(Color.Black, Color.Black);

            Program.titleConsole.DrawBox(new Rectangle(0, 0, Program.titleConsole.Width, Program.titleConsole.Height),
                ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.AntiqueWhite, Color.Black)));
            Program.titleConsole.DrawBox(new Rectangle(1, 1, Program.titleConsole.Width - 2, Program.titleConsole.Height - 2),
                ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));

            Program.titleConsole.DrawBox(new Rectangle(25, 15, 50, 25), ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.Black, Color.Black, 177)));

            Program.titleConsole.Print(26, 18, "- Type Your New Name -".Align(HorizontalAlignment.Center, 47), Color.Yellow, Color.Black);
            if (Program.playerName.Length <= 19)
            {
                Program.titleConsole.Print(26, 30, $"{Program.playerName}{'_'}".Align(HorizontalAlignment.Center, 47), Color.Yellow, Color.Black);
            }
            else
            {
                Program.titleConsole.Print(26, 30, $"{Program.playerName}".Align(HorizontalAlignment.Center, 47), Color.Yellow, Color.Black);
            }
            Program.titleConsole.Print(26, 30, ">", Color.Yellow, Color.Black);
            Program.titleConsole.Print(73, 30, "<", Color.Yellow, Color.Black);
        }
        public static void CreateAltarDisplay(Altar terminal)
        {
            popupActive = true;
            Program.interactionConsole.Position = new Point(((int)(Program.mapWidth) / 2) - (Program.interactionWidth / 2), (int)((Program.mapHeight) / 2) - (Program.interactionHeight / 2));
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);
            Program.interactionConsole.Fill(Color.Black, Color.Black);

            Program.interactionConsole.Print(1, 3, "What will you request of your god?".Align(HorizontalAlignment.Center, Program.interactionWidth - 2, (char)0), Color.Yellow, Color.Black);
            Program.interactionConsole.DrawLine(new Point(0, 5), new Point(Program.interactionWidth, 5),
                (char)196, Color.AntiqueWhite, Color.Black);

            Program.interactionConsole.Print(1, 20, "Or".Align(HorizontalAlignment.Center, Program.interactionWidth - 2, (char)0), Color.Yellow, Color.Black);
        }
        public static void CreateItemDisplay(Entity item)
        {
            popupActive = true;
            Program.interactionConsole.Position = new Point(((int)(Program.mapWidth) / 2) - (Program.interactionWidth / 2), (int)((Program.mapHeight) / 2) - (Program.interactionHeight / 2));
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);
            Program.interactionConsole.Fill(Color.Black, Color.Black);

            string description = item.GetComponent<Description>().description;


            Program.interactionConsole.DrawLine(new Point(0, (int)(Program.interactionHeight / 1.5f) + 1), new Point(Program.interactionWidth, (int)(Program.interactionHeight / 1.5f) + 1),
                (char)196, Color.AntiqueWhite, Color.Black);

            if (item.GetComponent<Equipable>() != null)
            {
                if (item.GetComponent<Equipable>().equipped)
                {
                    canUnequip = true;
                    canEquip = false;
                }
                else
                {
                    canUnequip = false;
                    canEquip = true;
                }
            }
            else
            {
                canUnequip = false;
                canEquip = false;
            }

            if (item.GetComponent<Usable>() != null)
            {
                canUse = true;
            }
            else
            {
                canUse = false;
            }

            if (ItemIdentityManager.IsItemIdentified(item.GetComponent<Description>().name))
            {
                Math.DisplayToConsole(Program.interactionConsole, $"{description}", 1, 1, 0, 2, false);
            }
            else
            {
                Math.DisplayToConsole(Program.interactionConsole, $"You have no idea what this item could be. Only through use could its identity be discovered.", 1, 1, 0, 2, false);
            }
        }
        public static void ClosePopup()
        {
            popupActive = false;
            Program.rootConsole.Children.MoveToBottom(Program.interactionConsole);
            Program.interactionConsole.Position = new Point((Program.screenWidth / 2) - (Program.interactionWidth / 2), (Program.screenHeight / 2) - (Program.interactionHeight / 2));
        }
    }
}
