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
        public static void CreateConfirmationPrompt(string prompt)
        {
            CreateConfirmationPrompt(new List<string> { prompt });
        }
        public static void CreateConfirmationPrompt(List<string> prompts)
        {
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);

            Program.interactionConsole.Fill(Color.Black, Color.Black);

            Program.interactionConsole.DrawBox(new Rectangle(3, 4, Program.interactionConsole.Width - 6, Program.interactionConsole.Height - 7),
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

            Program.CreateConsoleBorder(Program.interactionConsole);
        }
        public static void CreatePopup(string popup)
        {
            CreatePopup(new List<string> { popup });
        }
        public static void CreatePopup(List<string> popups)
        {
            Program.rootConsole.Children.MoveToTop(Program.interactionConsole);

            Program.interactionConsole.Fill(Color.Black, Color.Black);

            Program.interactionConsole.DrawBox(new Rectangle(3, 4, Program.interactionConsole.Width - 6, Program.interactionConsole.Height - 7),
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

            Program.CreateConsoleBorder(Program.interactionConsole);
        }
    }
}
