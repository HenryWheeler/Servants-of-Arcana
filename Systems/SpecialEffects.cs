using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.UI.Controls;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class SpecialEffects
    {
        public static void MagicMap(Entity user, Vector origin)
        {
            if (user.GetComponent<PlayerController>() != null) 
            {
                foreach (Tile tile in Program.tiles)
                {
                    if (tile != null && tile.terrainType != 0)
                    {
                        tile.GetComponent<Visibility>().explored = true;
                        Vector vector = tile.GetComponent<Vector>();

                        int distance = (int)Math.Distance(origin.x, origin.y, vector.x, vector.y);

                        
                        Particle particle = ParticleManager.CreateParticle(true, vector, distance, 3, "None", new Draw(Color.Orange, Color.Black, (char)176), null, true, true, null, true);
                        //particle.AddComponent(new FadingParticleEmitter(new List<Particle> { particle, particle, particle }));
                        //particle.SetDelegates();
                    }
                }
            }
        }
        public static void Explosion(Entity creator, Vector origin, int strength, string name)
        {
            List<Vector> affectedTiles = AreaOfEffectModels.ReturnSphere(origin, strength, false);

            foreach (Vector vector in affectedTiles) 
            {
                Tile tile = Program.tiles[vector.x, vector.y];

                if (tile != null) 
                {
                    if (tile.actor != null)
                    {
                        CombatManager.AttackTarget(creator, tile.actor, strength, strength, strength, strength, name);
                    }
                    else
                    {
                        int life = (int)(Math.Distance(origin.x, origin.y, vector.x, vector.y) + 1);

                        Particle deathParticle3 = ParticleManager.CreateParticle(false, vector, life - 3, 4, "None",
                            new Draw(Color.DarkOrange, Color.Black, (char)176), null, true, false);

                        Particle deathParticle2 = ParticleManager.CreateParticle(false, vector, life - 2, 4, "None",
                            new Draw(Color.Red, Color.Black, (char)177), null, true, false, new List<Particle>() { deathParticle3 });

                        Particle deathParticle = ParticleManager.CreateParticle(false, vector, life - 1, 3, "None",
                            new Draw(Color.DarkOrange, Color.Black, (char)176), null, true, false, new List<Particle>() { deathParticle2 });

                        Particle particle = ParticleManager.CreateParticle(false, vector, life, 2, "None",
                            new Draw(Color.Red, Color.Black, (char)177), null, true, false, new List<Particle>() { deathParticle });

                        ParticleManager.AddParticleToSystem(particle);
                    }
                }
            }
        }
    }
    public class AreaOfEffectModels
    {
        private static List<Vector> result = new List<Vector>();
        public static List<Vector> ReturnSphere(Vector origin, int range)
        {
            for (uint octant = 0; octant < 8; octant++)
            {
                ComputeOctant(octant, origin.x, origin.y, range, 1, new Slope(1, 1), new Slope(0, 1));
            }

            List<Vector> resultList = new List<Vector>();

            foreach (Vector v in result)
            {
                resultList.Add(v);
            }

            resultList.Add(origin);

            result.Clear();
            return resultList;
        }
        public static void ComputeOctant(uint octant, int oX, int oY, int range, int x, Slope top, Slope bottom)
        {
            for (; (uint)x <= (uint)range; x++)
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

                    bool inRange = range < 0 || Math.Distance(oX, oY, tx, ty) <= range;
                    if (inRange && (y != topY || top.Y * x >= top.X * y) && (y != bottomY || bottom.Y * x <= bottom.X * y))
                    {
                        Vector vector = new Vector(tx, ty);

                        if (!result.Contains(vector))
                        {
                            result.Add(vector);
                        }
                    }

                    bool isOpaque = !inRange || BlocksModel(new Vector(tx, ty));
                    if (x != range)
                    {
                        if (isOpaque)
                        {
                            if (wasOpaque == 0)
                            {
                                Slope newBottom = new Slope(y * 2 + 1, x * 2 - 1);
                                if (!inRange || y == bottomY) { bottom = newBottom; break; }
                                else { ComputeOctant(octant, oX, oY, range, x + 1, top, newBottom); }
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
        public static bool BlocksModel(Vector vector2)
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
