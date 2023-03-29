using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
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
        public static void Explosion(Entity creator, Vector origin, int strength, string name, bool fireball)
        {
            List<Vector> affectedTiles = AreaOfEffectModels.ReturnSphere(creator.GetComponent<Vector>(), origin, strength);
            List<Particle> particles = new List<Particle>();

            foreach (Vector vector in affectedTiles) 
            {
                Tile tile = Program.tiles[vector.x, vector.y];

                if (tile != null) 
                {
                    if (tile.actor != null)
                    {
                        CombatManager.SpecialAttack(creator, tile.actor, strength, strength, strength, strength, name);
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

                        if (fireball)
                        {
                            particles.Add(particle);
                        }
                        else
                        {
                            ParticleManager.AddParticleToSystem(particle);
                        }
                    }
                }
            }

            if (fireball)
            {
                Vector userLocation = creator.GetComponent<Vector>();

                //int life = 100;

                List<Particle> fireBallParticles = new List<Particle>();

                Particle initialFireball1 = ParticleManager.CreateParticle(false, new Vector(0, 0), 5, 5, "Wander",
                    new Draw(Color.DarkOrange, Color.Black, (char)176), null, true, false);
                fireBallParticles.Add(initialFireball1);
                fireBallParticles.Add(initialFireball1);

                Particle initialFireball2 = ParticleManager.CreateParticle(false, new Vector(0, 0), 5, 5, "Wander",
                    new Draw(Color.Red, Color.Black, (char)177), null, true, false);
                fireBallParticles.Add(initialFireball2);
                fireBallParticles.Add(initialFireball2);


                ParticleManager.CreateParticle(true, userLocation, (int)Math.Distance(userLocation.x, userLocation.y, origin.x, origin.y), 5, "Target",
                    new Draw(Color.Red, Color.Black, (char)177), origin, false, false, particles, false, true);
            }
        }
        public static void Lightning(Entity creator, Vector destination, int strength, int range, string name)
        {
            List<Vector> affectedTiles = AreaOfEffectModels.ReturnLine(creator.GetComponent<Vector>(), destination, range);

            foreach (Vector vector in affectedTiles)
            {
                Tile tile = Program.tiles[vector.x, vector.y];

                if (tile != null)
                {
                    if (tile.actor != null)
                    {
                        CombatManager.SpecialAttack(creator, tile.actor, strength, strength, strength, strength, name);
                    }
                    else
                    {
                        Vector origin = creator.GetComponent<Vector>();
                        int life = (int)((Math.Distance(origin.x, origin.y, vector.x, vector.y) + 1) / 5);

                        Particle deathParticle4 = ParticleManager.CreateParticle(false, vector, 5, 5, "None",
                            new Draw(Color.Yellow, Color.Black, (char)176), null, false, false, null, true);

                        Particle deathParticle3 = ParticleManager.CreateParticle(false, vector, 5, 5, "None",
                            new Draw(Color.Yellow, Color.Black, (char)176), null, true, false, new List<Particle>() { deathParticle4 }, true);

                        Particle deathParticle2 = ParticleManager.CreateParticle(false, vector, 10 - life, 5, "None",
                            new Draw(Color.Yellow, Color.Black, (char)176), null, true, false, new List<Particle>() { deathParticle3 }, true);

                        Particle explosionParticle = ParticleManager.CreateParticle(true, vector, life, 5, "None",
                            new Draw(Color.Black, Color.Black, (char)177), null, false, false, new List<Particle>() { deathParticle2 }, true);
                    }
                }
            }
        }
        public static List<Entity> SummonMinions(Entity summoner, Vector origin, List<string> minionName, int amountToSummon)
        {
            List<Entity> minions = new List<Entity>();

            for (int i = 0; i < amountToSummon; i++) 
            {
                foreach (string name in minionName)
                {
                    Entity entity = JsonDataManager.ReturnEntity(name);

                    entity.GetComponent<Vector>().x = origin.x;
                    entity.GetComponent<Vector>().y = origin.y;

                    Program.tiles[origin.x, origin.y].actor = entity;


                    if (entity.GetComponent<MinionAI>() != null)
                    {
                        entity.GetComponent<MinionAI>().SetMaster(summoner);
                    }

                    TurnManager.AddActor(entity.GetComponent<TurnComponent>());
                    Math.SetTransitions(entity);

                    minions.Add(entity);
                }
            }

            return minions;
        } 
        public static void Transpose(Entity user, Vector victim)
        {
            Entity target = Program.tiles[victim.x, victim.y].actor;

            Vector vector = target.GetComponent<Vector>();
            Vector origin = user.GetComponent<Vector>();
            Vector tempSave = new Vector(vector.x, vector.y);

            vector.x = origin.x;
            vector.y = origin.y;

            origin.x = tempSave.x;
            origin.y = tempSave.y;

            Program.tiles[vector.x, vector.y].actor = target;
            Program.tiles[origin.x, origin.y].actor = user;

            List<Vector> affectedTiles = AreaOfEffectModels.ReturnSphere(user.GetComponent<Vector>(), user.GetComponent<Vector>(), 3);

            foreach (Vector vector2 in affectedTiles)
            {
                Tile tile = Program.tiles[vector2.x, vector2.y];

                if (tile != null)
                {
                    int life = (int)(Math.Distance(origin.x, origin.y, vector2.x, vector2.y) + 1);

                    Particle deathParticle3 = ParticleManager.CreateParticle(false, vector2, life - 3, 4, "None",
                        new Draw(Color.MediumPurple, Color.Black, (char)176), null, true, false);

                    Particle deathParticle2 = ParticleManager.CreateParticle(false, vector2, life - 2, 4, "None",
                        new Draw(Color.Purple, Color.Black, (char)177), null, true, false, new List<Particle>() { deathParticle3 });

                    Particle deathParticle = ParticleManager.CreateParticle(false, vector2, life - 1, 3, "None",
                        new Draw(Color.MediumPurple, Color.Black, (char)176), null, true, false, new List<Particle>() { deathParticle2 });

                    Particle explosionParticle = ParticleManager.CreateParticle(true, vector2, life, 2, "None",
                        new Draw(Color.Purple, Color.Black, (char)177), null, true, false, new List<Particle>() { deathParticle });
                }
            }

            affectedTiles = AreaOfEffectModels.ReturnSphere(target.GetComponent<Vector>(), target.GetComponent<Vector>(), 3);

            foreach (Vector vector3 in affectedTiles)
            {
                Tile tile = Program.tiles[vector3.x, vector3.y];

                if (tile != null)
                {
                    int life = (int)(Math.Distance(origin.x, origin.y, vector3.x, vector3.y) + 1);

                    Particle deathParticle3 = ParticleManager.CreateParticle(false, vector3, life - 3, 4, "None",
                        new Draw(Color.MediumPurple, Color.Black, (char)176), null, true, false);

                    Particle deathParticle2 = ParticleManager.CreateParticle(false, vector3, life - 2, 4, "None",
                        new Draw(Color.Purple, Color.Black, (char)177), null, true, false, new List<Particle>() { deathParticle3 });

                    Particle deathParticle = ParticleManager.CreateParticle(false, vector3, life - 1, 3, "None",
                        new Draw(Color.MediumPurple, Color.Black, (char)176), null, true, false, new List<Particle>() { deathParticle2 });

                    Particle explosionParticle = ParticleManager.CreateParticle(true, vector3, life, 2, "None",
                        new Draw(Color.Purple, Color.Black, (char)177), null, true, false, new List<Particle>() { deathParticle });
                }
            }

            if (user.GetComponent<PlayerController>() != null && user == target)
            {
                Log.Add($"You have swapped places with yourself.");
            }
            else if (user.GetComponent<PlayerController>() != null) 
            {
                Log.Add($"{user.GetComponent<Description>().name} have swapped places with the {target.GetComponent<Description>().name}.");
            }
            else if (target.GetComponent<PlayerController>() != null)
            {
                Log.Add($"{user.GetComponent<Description>().name} has swapped places with you.");
            }
            else if (user == target)
            {
                Log.Add($"{user.GetComponent<Description>().name} has swapped places with itself.");
            }
            else
            {
                Log.Add($"{user.GetComponent<Description>().name} has swapped places with the {target.GetComponent<Description>().name}.");
            }
        }
    }
    public class AreaOfEffectModels
    {
        private static List<Vector> result = new List<Vector>();
        public static List<Vector> ReturnSphere(Vector origin, Vector destination, int range)
        {
            for (uint octant = 0; octant < 8; octant++)
            {
                ComputeOctant(octant, destination.x, destination.y, range, 1, new Slope(1, 1), new Slope(0, 1));
            }

            List<Vector> resultList = new List<Vector>();

            foreach (Vector v in result)
            {
                resultList.Add(v);
            }

            resultList.Add(destination);

            result.Clear();
            return resultList;
        }
        public static List<Vector> ReturnTwoPoints(Vector origin, Vector target, int range)
        {
            return new List<Vector>() { origin, target };
        }
        public static List<Vector> ReturnLine(Vector origin, Vector destination, int range)
        {
            List<Vector> coordinates = new List<Vector>();

            int t;
            int x = origin.x; int y = origin.y;
            int delta_x = destination.x - origin.x; int delta_y = destination.y - origin.y;
            int abs_delta_x = (int)MathF.Abs(delta_x); int abs_delta_y = (int)MathF.Abs(delta_y);
            int sign_x = MathF.Sign(delta_x); int sign_y = MathF.Sign(delta_y);
            bool hasConnected = false;

            if (abs_delta_x > abs_delta_y)
            {
                t = abs_delta_y * 2 - abs_delta_x;
                do
                {
                    if (t >= 0) { y += sign_y; t -= abs_delta_x * 2; }
                    x += sign_x;
                    t += abs_delta_y * 2;
                    coordinates.Add(new Vector(x, y));
                    if (x == destination.x && y == destination.y) { hasConnected = true; }
                }
                while (!hasConnected);
            }
            else
            {
                t = abs_delta_x * 2 - abs_delta_y;
                do
                {
                    if (t >= 0) { x += sign_x; t -= abs_delta_y * 2; }
                    y += sign_y;
                    t += abs_delta_x * 2;
                    coordinates.Add(new Vector(x, y));
                    if (x == destination.x && y == destination.y) { hasConnected = true; }
                }
                while (!hasConnected);
            }

            return coordinates;
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
