using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;
using System.Collections.Specialized;

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

            Program.ClearSFX();
            MoveReticle(new Vector(0, 0));
        }
        public static void StopLooking()
        {
            Program.player.GetComponent<TurnComponent>().isTurnActive = true;
            looking = false;

            Program.lookConsole.Clear();
            Program.rootConsole.Children.MoveToTop(Program.playerConsole);

            AttributeManager.UpdateAttributes(Program.player);

            Program.ClearSFX();
            Program.MoveCamera(Program.player.GetComponent<Vector>());
            Program.DrawMap();
        }
        public static void MoveReticle(Vector direction)
        {
            if (Math.CheckBounds(reticlePosition.x + direction.x, reticlePosition.y + direction.y))
            {
                Program.ClearSFX();
                Program.lookConsole.Clear();

                reticlePosition.x += direction.x;
                reticlePosition.y += direction.y;


                Description description = null;
                if (!Program.tiles[reticlePosition.x, reticlePosition.y].GetComponent<Visibility>().visible)
                {
                    Program.lookConsole.DrawBox(new Rectangle(3, 4, Program.lookConsole.Width - 6, Program.lookConsole.Height - 7), 
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
                    description = Program.tiles[reticlePosition.x, reticlePosition.y].item.GetComponent<Description>();
                }
                else
                {
                    description = Program.tiles[reticlePosition.x, reticlePosition.y].GetComponent<Description>();
                }

                if (description != null)
                {
                    //string health = "";

                    /*
                    if (description.entity != null && description.entity.GetComponent<PronounSet>() != null)
                    {
                        if (description.entity.GetComponent<PronounSet>().present)
                        {
                            health += $"{description.name} is: + ";
                        }
                        else
                        {
                            health += $"{description.name} are: + ";
                        }

                        if (Math.ReturnAI(description.entity) != null)
                        {
                            health += $"{Math.ReturnAI(description.entity).currentState}, ";

                            if (description.entity.GetComponent<Harmable>().statusEffects.Count == 0)
                            {
                                health += "and ";
                            }
                        }

                        Attributes stats = description.entity.GetComponent<Attributes>();

                        if (stats.hp == stats.hpCap) { health += "Green*Uninjured"; }
                        else if (stats.hp <= stats.hpCap && stats.hp >= stats.hpCap / 2) { health += "Yellow*Hurt"; }
                        else { health += "Red*Badly Red*Hurt"; }

                        if (description.entity.GetComponent<Harmable>().statusEffects.Count == 0)
                        {
                            health += ".";
                        }
                        else
                        {
                            health += ", + ";
                        }

                        for (int i = 0; i < description.entity.GetComponent<Harmable>().statusEffects.Count; i++)
                        {
                            if (i == description.entity.GetComponent<Harmable>().statusEffects.Count - 1)
                            {
                                health += $"and {description.entity.GetComponent<Harmable>().statusEffects[i]}. + ";
                            }
                            else
                            {
                                health += $"{description.entity.GetComponent<Harmable>().statusEffects[i]}, ";
                            }
                        }
                    }
                    */


                    //Create boxes to surround look menu, and display information.

                    string[] nameParts = description.name.Split(' ');
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
                    int start = (Program.lookConsole.Width / 2) - (int)System.Math.Ceiling((double)name.Length / 2);

                    start++;

                    foreach (string part in nameParts)
                    {
                        string[] temp = part.Split('*');
                        if (temp.Length == 1)
                        {
                            Program.lookConsole.Print(start, 6, temp[0] + " ", Color.White);
                            start += temp[0].Length + 1;
                        }
                        else
                        {
                            Program.lookConsole.Print(start, 6, temp[1] + " ", Log.StringToColor(temp[0]), Color.Black);
                            start += temp[1].Length + 1;
                        }
                    }

                    Program.lookConsole.DrawLine(new Point(0, 7), new Point(Program.lookConsole.Width, 7), (char)196, Color.AntiqueWhite, Color.Black);
                    Math.DisplayToConsole(Program.lookConsole, $"{description.description}", 3, 1, 0, 9, false);
                }

                Program.CreateConsoleBorder(Program.lookConsole);
                Program.MoveCamera(reticlePosition);
                CreateReticle();
                Program.DrawMap();
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

            Program.sfx[reticlePosition.x, reticlePosition.y + 1] = new Entity(new List<Component>() 
            {
                new Draw(Color.Yellow, Color.Black, (char)196),
            });
            Program.sfx[reticlePosition.x, reticlePosition.y - 1] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)196),
            });

            Program.sfx[reticlePosition.x + 1, reticlePosition.y] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)179),
            });
            Program.sfx[reticlePosition.x - 1, reticlePosition.y] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)179),
            });

            Program.sfx[reticlePosition.x + 1, reticlePosition.y + 1] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)217),
            });
            Program.sfx[reticlePosition.x - 1, reticlePosition.y + 1] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)192),
            });
            Program.sfx[reticlePosition.x + 1, reticlePosition.y - 1] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)191),
            });
            Program.sfx[reticlePosition.x - 1, reticlePosition.y - 1] = new Entity(new List<Component>()
            {
                new Draw(Color.Yellow, Color.Black, (char)218),
            });
        }
    }
}
