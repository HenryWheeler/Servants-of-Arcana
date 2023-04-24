using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;
using System.Collections.Specialized;
using Servants_of_Arcana.Systems;

namespace Servants_of_Arcana
{
    public class Look
    {
        public static bool looking { get; set; } = false;
        private static Vector reticlePosition { get;set; }
        public static void StartLooking()
        {
            reticlePosition = new Vector();
            reticlePosition.x = Program.player.GetComponent<Vector>().x;
            reticlePosition.y = Program.player.GetComponent<Vector>().y;

            Program.player.GetComponent<TurnComponent>().isTurnActive = false;
            looking = true;

            Program.playerConsole.Clear();
            Program.rootConsole.Children.MoveToTop(Program.lookConsole);
            Program.rootConsole.Children.MoveToBottom(Program.playerConsole);
            Program.rootConsole.Children.MoveToBottom(Program.inventoryConsole);

            Program.ClearUISFX();
            MoveReticle(new Vector(0, 0));
        }
        public static void StopLooking()
        {
            Program.player.GetComponent<TurnComponent>().isTurnActive = true;
            looking = false;

            Program.lookConsole.Clear();
            Program.rootConsole.Children.MoveToTop(Program.playerConsole);
            Program.rootConsole.Children.MoveToTop(Program.inventoryConsole);

            AttributeManager.UpdateAttributes(Program.player);

            Program.ClearUISFX();
            Program.MoveCamera(Program.player.GetComponent<Vector>());
            //Program.DrawMap();
        }
        public static void MoveReticle(Vector direction)
        {
            if (Math.CheckBounds(reticlePosition.x + direction.x, reticlePosition.y + direction.y))
            {
                Program.ClearUISFX();
                Program.lookConsole.Clear();

                reticlePosition.x += direction.x;
                reticlePosition.y += direction.y;

                bool itemUnknown = false;


                Description description = null;
                if (!Program.tiles[reticlePosition.x, reticlePosition.y].GetComponent<Visibility>().visible)
                {
                    Program.lookConsole.DrawBox(new Rectangle(1, 1, Program.lookConsole.Width - 2, Program.lookConsole.Height - 2), 
                    ShapeParameters.CreateStyledBoxFilled(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Gray, Color.Black), new ColoredGlyph(Color.AntiqueWhite, Color.Black, 177)));
                    Program.lookConsole.Print(4, Program.lookConsole.Height / 2 - 3, " You cannot look at ".Align(HorizontalAlignment.Center, Program.lookConsole.Width - 8, (char)177), Color.AntiqueWhite);
                    Program.lookConsole.Print(4, Program.lookConsole.Height / 2 - 1, " what you cannot see. ".Align(HorizontalAlignment.Center, Program.lookConsole.Width - 8, (char)177), Color.AntiqueWhite);
                }
                else if (Program.tiles[reticlePosition.x, reticlePosition.y].actor != null)
                {
                    description = Program.tiles[reticlePosition.x, reticlePosition.y].actor.GetComponent<Description>();
                }
                else if (Program.tiles[reticlePosition.x, reticlePosition.y].item != null)
                {
                    if (!ItemIdentityManager.IsItemIdentified(Program.tiles[reticlePosition.x, reticlePosition.y].item.GetComponent<Description>().name))
                    {
                        itemUnknown = true;
                    }
                    description = Program.tiles[reticlePosition.x, reticlePosition.y].item.GetComponent<Description>();
                }
                else
                {
                    description = Program.tiles[reticlePosition.x, reticlePosition.y].GetComponent<Description>();
                }

                if (description != null)
                {
                    if (itemUnknown)
                    {
                        Math.DisplayToConsole(Program.lookConsole, $"Unknown Item", 1, 1, 0, 3, false);
                        Math.DisplayToConsole(Program.lookConsole, "You have no idea what this item could be. Only through use could its identity be discovered.", 1, 1, 0, 7, false);
                    }
                    else
                    {
                        Math.DisplayToConsole(Program.lookConsole, $"{description.name}", 1, 1, 0, 3, false);
                        Math.DisplayToConsole(Program.lookConsole, $"{description.description}", 1, 1, 0, 7, false);
                    }
                    Program.lookConsole.DrawLine(new Point(0, 5), new Point(Program.lookConsole.Width, 5), (char)196, Color.AntiqueWhite, Color.Black);
                }

                Program.CreateConsoleBorder(Program.lookConsole);
                Program.MoveCamera(reticlePosition);
                CreateReticle();
                //Program.DrawMap();
            }
        }
        private static void CreateReticle()
        {
            for (int x = reticlePosition.x - 1; x  <= reticlePosition.x + 1; x++) 
            {
                for (int y = reticlePosition.y - 1; y <= reticlePosition.y + 1; y++)
                {
                    if (!Math.CheckBounds(x, y))
                    {
                        return;
                    }
                }
            }

            Draw draw;

            draw = new Draw(Color.Yellow, Color.Black, (char)196);
            Program.uiSfx[reticlePosition.x, reticlePosition.y + 1] = draw;
            Program.uiSfx[reticlePosition.x, reticlePosition.y - 1] = draw;


            draw = new Draw(Color.Yellow, Color.Black, (char)179);
            Program.uiSfx[reticlePosition.x + 1, reticlePosition.y] = draw;
            Program.uiSfx[reticlePosition.x - 1, reticlePosition.y] = draw;


            draw = new Draw(Color.Yellow, Color.Black, (char)217);
            Program.uiSfx[reticlePosition.x + 1, reticlePosition.y + 1] = draw;


            draw = new Draw(Color.Yellow, Color.Black, (char)192);
            Program.uiSfx[reticlePosition.x - 1, reticlePosition.y + 1] = draw;


            draw = new Draw(Color.Yellow, Color.Black, (char)191);
            Program.uiSfx[reticlePosition.x + 1, reticlePosition.y - 1] = draw;


            draw = new Draw(Color.Yellow, Color.Black, (char)218);
            Program.uiSfx[reticlePosition.x - 1, reticlePosition.y - 1] = draw;
        }
    }
}
