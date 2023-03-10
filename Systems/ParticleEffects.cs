using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class ParticleEffects
    {
        public static void RevealNewFloor()
        {
            Draw lightDisplay1 = new Draw(Color.White, Color.White, (char)0);
            Draw black = new Draw(Color.Black, Color.Black, (char)3);

            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    Vector vector = Program.player.GetComponent<Vector>();
                    int distance = (int)Math.Distance(x, y, vector.x, vector.y);

                    Draw[] draw = new Draw[distance];

                    for (int i = 0; i < distance; i++) 
                    {
                        if (i < distance - 1)
                        {
                            draw[i] = black;
                        }
                    }

                    draw[distance - 1] = lightDisplay1;

                    Program.CreateSFX(new Vector(x, y), draw, distance, "None", 5);
                }
            }
        }
        public static void FloorFadeAway()
        {
            for (int x = 0; x < Program.gameWidth; x++)
            {
                for (int y = 0; y < Program.gameHeight; y++)
                {
                    if (Math.CheckBounds(x, y))
                    {
                        Draw draw1 = new Draw();
                        if (Program.tiles[x, y].actor != null)
                        {
                            draw1.fColor = Program.tiles[x, y].actor.GetComponent<Draw>().fColor;
                            draw1.bColor = Program.tiles[x, y].actor.GetComponent<Draw>().bColor;
                            draw1.character = Program.tiles[x, y].actor.GetComponent<Draw>().character;
                        }
                        else if (Program.tiles[x, y].item != null)
                        {
                            draw1.fColor = Program.tiles[x, y].item.GetComponent<Draw>().fColor;
                            draw1.bColor = Program.tiles[x, y].item.GetComponent<Draw>().bColor;
                            draw1.character = Program.tiles[x, y].item.GetComponent<Draw>().character;
                        }
                        else
                        {
                            draw1.fColor = Program.tiles[x, y].GetComponent<Draw>().fColor;
                            draw1.bColor = Program.tiles[x, y].GetComponent<Draw>().bColor;
                            draw1.character = Program.tiles[x, y].GetComponent<Draw>().character;
                        }

                        Draw[] draw = new Draw[] { draw1 };
                        Program.CreateSFX(new Vector(x, y), draw, Program.random.Next(25, 75) + y, "None", 1);

                        Program.tiles[x, y].GetComponent<Draw>().fColor = Color.Black;
                        Program.tiles[x, y].GetComponent<Draw>().bColor = Color.Black;
                    }
                }
            }

            foreach (Tile tile in Program.tiles)
            {
                if (tile != null)
                {
                    if (tile.actor != null) { tile.actor = null; }
                    if (tile.item != null) { tile.item = null; }
                }
            }
        }
    }
}
