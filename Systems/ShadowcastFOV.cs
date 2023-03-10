using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public struct Slope
    {
        public Slope(int y, int x) { Y = y; X = x; }
        public readonly int Y, X;
    }
    public static class ShadowcastFOV
    {
        public static List<Vector> visibleTiles = new List<Vector>();
        public static void Compute(Vector vector2, int rangeLimit)
        {
            SetVisible(vector2, true, rangeLimit, vector2.x, vector2.y);
            for (uint octant = 0; octant < 8; octant++)
            {
                Compute(octant, vector2.x, vector2.y, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));
            }
        }
        static void Compute(uint octant, int oX, int oY, int rangeLimit, int x, Slope top, Slope bottom)
        {
            for (; (uint)x <= (uint)rangeLimit; x++)
            {
                int topY = top.X == 1 ? x : ((x * 2 + 1) * top.Y + top.X - 1) / (top.X * 2);
                int bottomY = bottom.Y == 0 ? 0 : ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);

                int wasOpaque = -1;
                for (int y = topY; y >= bottomY; y--)
                {
                    int tx = oX, ty = oY;
                    switch (octant)
                    {
                        case 0: tx += x; ty -= y; break;
                        case 1: tx += y; ty -= x; break;
                        case 2: tx -= y; ty -= x; break;
                        case 3: tx -= x; ty -= y; break;
                        case 4: tx -= x; ty += y; break;
                        case 5: tx -= y; ty += x; break;
                        case 6: tx += y; ty += x; break;
                        case 7: tx += x; ty += y; break;
                    }

                    bool inRange = rangeLimit < 0 || Math.Distance(oX, oY, tx, ty) <= rangeLimit;
                    if (inRange && (y != topY || top.Y * x >= top.X * y) && (y != bottomY || bottom.Y * x <= bottom.X * y))
                    {
                        SetVisible(new Vector(tx, ty), true, rangeLimit - 3, oX, oY);
                    }

                    bool isOpaque = !inRange || BlocksLight(new Vector(tx, ty));
                    if (x != rangeLimit)
                    {
                        if (isOpaque)
                        {
                            if (wasOpaque == 0)
                            {
                                Slope newBottom = new Slope(y * 2 + 1, x * 2 - 1);
                                if (!inRange || y == bottomY) { bottom = newBottom; break; }
                                else { Compute(octant, oX, oY, rangeLimit, x + 1, top, newBottom); }
                            }
                            wasOpaque = 1;
                        }
                        else
                        {
                            if (wasOpaque > 0) top = new Slope(y * 2 + 1, x * 2 + 1);
                            wasOpaque = 0;
                        }
                    }
                }
                if (wasOpaque != 0) break;
            }
        }
        public static void ClearSight()
        {
            foreach (Vector coordinate in visibleTiles) { SetVisible(coordinate, false, 0, 0, 0); }
            visibleTiles.Clear();
        }
        public static void SetVisible(Vector vector2, bool visible, int sight, int oX, int oY, bool all = false)
        {
            if (Math.CheckBounds(vector2.x, vector2.y))
            {
                if (visible)
                {
                    if (Math.Distance(vector2.x, vector2.y, oX, oY) < sight)
                    {
                        Program.tiles[vector2.x, vector2.y].GetComponent<Visibility>().SetVisible(true);
                        if (!all)
                        {
                            visibleTiles.Add(vector2);
                        }
                    }
                    else
                    {
                        Program.tiles[vector2.x, vector2.y].GetComponent<Visibility>().explored = true;
                    }
                }
                else
                {
                    Program.tiles[vector2.x, vector2.y].GetComponent<Visibility>().SetVisible(false);
                }
            }
        }
        public static void RevealAll()
        {
            foreach (Tile tile in Program.tiles)
            {
                if (tile != null)
                {
                    Vector vector2 = tile.GetComponent<Vector>();
                    SetVisible(vector2, true, 1000, vector2.x, vector2.y, true);
                }
            }
            Program.DrawMap();
        }
        public static void HideAll()
        {
            foreach (Tile tile in Program.tiles)
            {
                if (tile != null)
                {
                    Vector vector2 = tile.GetComponent<Vector>();
                    SetVisible(vector2, false, 1000, vector2.x, vector2.y, true);
                }
            }
            Program.DrawMap();
        }
        public static bool BlocksLight(Vector vector2)
        {
            if (Math.CheckBounds(vector2.x, vector2.y))
            {
                Tile traversable = Program.tiles[vector2.x, vector2.y];
                if (traversable.GetComponent<Visibility>().opaque)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
