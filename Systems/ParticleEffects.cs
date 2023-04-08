using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SadConsole;
using SadConsole.Ansi;
using SadConsole.Entities;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class ParticleManager
    {
        public static Particle CreateParticle(bool addToSystem, Vector position, int life, int speed, string movement, Draw draw, Vector target = null, bool fade = false, bool alwaysVisible = false, List<Particle> deathParticles = null, bool randomizeCharacter = false, bool showOverActors = false, List<Particle> moveParticles = null)
        {
            Particle particle = new Particle(life, speed, movement, target, alwaysVisible, showOverActors);
            particle.AddComponent(new Vector(position.x, position.y));
            particle.AddComponent(draw);

            if (fade)
            {
                AddFade(particle);
            }

            if (deathParticles != null)
            {
                particle.AddComponent(new CreateParticleOnDeath(deathParticles));
            }

            if (randomizeCharacter)
            {
                particle.AddComponent(new RandomizeCharacter());
            }

            if (moveParticles != null)
            {
                particle.AddComponent(new CreateParticleOnMove(deathParticles));
            }


            if (addToSystem)
            {
                AddParticleToSystem(particle);
            }
            particle.SetDelegates();
            return particle;
        }
        public static void AddParticleToSystem(Particle particle)
        {
            Program.rootConsole.particles.Add(particle);
        }
        public static void AddFade(Particle particle)
        {
            particle.AddComponent(new Fade());
            particle.SetDelegates();
        }
        public static void CreateMovementParticle(Entity entity)
        {
            Vector vector = entity.GetComponent<Vector>();
            CreateParticle(true, vector, 50, 5, "None", new Draw(Color.BlueViolet, Color.Black, (char)178), null, true, false);
        }
        public static void CreateAIStateParticle(AIController.State state, Vector vector)
        {
            switch(state) 
            {
                case AIController.State.Angry: 
                    {
                        //Particle particle = CreateParticle(false, vector, 5, 5, "Attached", new Draw(Color.Red, Color.Black, (char)19), vector, false, false, null, false, true);
                        CreateParticle(true, vector, 5, 53, "Attached", new Draw(Color.Red, Color.Black, (char)19), vector, false, false, null, false, true);
                        break;
                    }
            }
        }
        public static void CreateBloodStain(Vector origin)
        {
            List<Vector> possibleLocations = new List<Vector>();

            for (int x = origin.x - 1; x < origin.x + 1; x++)
            {
                for (int y = origin.y - 1; y < origin.y + 1; y++)
                {
                    Vector vector = new Vector(x, y);

                    if (Math.CheckBounds(x, y) && !possibleLocations.Contains(vector) && Program.tiles[x, y].GetComponent<Draw>().character != '<')
                    {
                        possibleLocations.Add(vector);
                    }
                }
            }

            Vector chosenLocation = possibleLocations[Program.random.Next(possibleLocations.Count)];
            
            if (Program.tiles[chosenLocation.x, chosenLocation.y].terrainType == 0)
            {
                Program.tiles[chosenLocation.x, chosenLocation.y].GetComponent<Draw>().fColor = Color.DarkRed;
            }
            else if (Program.tiles[chosenLocation.x, chosenLocation.y].terrainType == 1)
            {
                Program.tiles[chosenLocation.x, chosenLocation.y].GetComponent<Draw>().fColor = Color.DarkRed;
                Program.tiles[chosenLocation.x, chosenLocation.y].GetComponent<Draw>().character = (char)176;
            }
        }
    }
    public class Fade : Component
    {
        Color foreground { get; set; }
        Color background { get; set; }
        public int initialOffset { get; set; }
        public override void SetDelegates()
        {
            if (entity.GetType() == typeof(Particle))
            {
                Particle entity = (Particle)this.entity;
                entity.onParticleDraw += FadeParticle;

                initialOffset = entity.life * 51;

                Draw draw = entity.GetComponent<Draw>();
                foreground = draw.fColor; 
                background = draw.bColor;
            }
        }
        public void FadeParticle()
        {
            Particle particle = (Particle)entity;

            float offset = initialOffset / (particle.life);
            Draw draw = entity.GetComponent<Draw>();

            draw.fColor = new Color((int)(foreground.R - offset), (int)(foreground.G - offset), (int)(foreground.B - offset));

            draw.bColor = new Color((int)(background.R - offset), (int)(background.G - offset), (int)(background.B - offset));

            if (draw.fColor.R < 0 && draw.fColor.G < 0 && draw.fColor.B < 0 && draw.bColor.R < 0 && draw.bColor.G < 0 && draw.bColor.B < 0)
            {
                Particle entity = (Particle)this.entity;
                entity.KillParticle(entity.GetComponent<Vector>());
            }
        }
        public Fade() { }
    }
    public class RandomizeCharacter : Component
    {
        public override void SetDelegates()
        {
            if (entity.GetType() == typeof(Particle))
            {
                Particle entity = (Particle)this.entity;
                entity.onParticleDraw += RandomizeParticleCharacter;
            }
        }
        public void RandomizeParticleCharacter()
        {
            entity.GetComponent<Draw>().character = (char)Program.random.Next(255);
        }
        public RandomizeCharacter() { }
    }
    public class CreateParticleOnDeath : Component
    {
        public List<Particle> particles = new List<Particle>();
        public override void SetDelegates()
        {
            if (entity.GetType() == typeof(Particle))
            {
                Particle entity = (Particle)this.entity;
                entity.onParticleDeath += CreateParticle;
            }
        }
        public void CreateParticle(Vector position)
        {
            foreach (Particle particle in particles)
            {
                ParticleManager.AddParticleToSystem(particle);
            }
        }
        public CreateParticleOnDeath(List<Particle> particles)
        {
            this.particles = particles;
        }
    }
    public abstract class Emitter : Component
    {
        public List<Particle> particles = new List<Particle>();
        public abstract override void SetDelegates();
        public Emitter(List<Particle> particles) { this.particles = particles; }
    }
    public class CreateParticleOnMove : Emitter
    {
        public override void SetDelegates()
        {
            if (entity.GetType() == typeof(Particle))
            {
                Particle entity = (Particle)this.entity;
                entity.onParticleMove += CreateParticle;
            }
        }
        public void CreateParticle(Vector position)
        {
            foreach (Particle particle in particles)
            {
                ParticleManager.AddParticleToSystem(particle);
            }
        }
        public CreateParticleOnMove(List<Particle> particles)
        :base(particles) { }
    }
    public class Particle : Entity
    {
        public int life { get; set; }
        public int speed { get; set; }
        public string movement { get; set; }
        public bool alwaysVisible { get; set; }
        public bool showOverActors { get; set; }

        public Action<Vector> onParticleDeath;
        public Action<Vector> onParticleMove;
        public Action onParticleDraw;
        public Vector target { get; set; }
        public void Progress()
        {
            Vector position = GetComponent<Vector>();

            switch (movement)
            {
                case "Attached":
                    {
                        if (target != null)
                        {
                            GetComponent<Vector>().x = target.x;
                            GetComponent<Vector>().y = target.y;
                        }
                        break;
                    }
                case "Target":
                    {
                        Vector newPosition = DijkstraMap.PathFromMap(this, "ParticlePath");
                        GetComponent<Vector>().x = newPosition.x;
                        GetComponent<Vector>().y = newPosition.y;
                        break;
                    }
                case "Wander":
                    {
                        position.x += Program.random.Next(-1, 2);
                        position.y += Program.random.Next(-1, 2);
                        break;
                    }
                case "None": { break; }
                case "North":
                    {
                        position.y--;
                        break;
                    }
                case "NorthEast":
                    {
                        position.x--;
                        position.y--;
                        break;
                    }
                case "East":
                    {
                        position.x--;
                        break;
                    }
                case "SouthEast":
                    {
                        position.x--;
                        position.y++;
                        break;
                    }
                case "South":
                    {
                        position.y++;
                        break;
                    }
                case "SouthWest":
                    {
                        position.x++;
                        position.y++;
                        break;
                    }
                case "West":
                    {
                        position.x++;
                        break;
                    }
                case "NorthWest":
                    {
                        position.x++;
                        position.y--;
                        break;
                    }
                case "WanderNorth":
                    {
                        position.x += Program.random.Next(-1, 2);
                        position.y += Program.random.Next(-1, 0);
                        break;
                    }
                case "WanderNorthEast":
                    {
                        position.x += Program.random.Next(-1, 0);
                        position.y += Program.random.Next(-1, 0);
                        break;
                    }
                case "WanderEast":
                    {
                        position.x += Program.random.Next(-1, 0);
                        position.y += Program.random.Next(-1, 2);
                        break;
                    }
                case "WanderSouthEast":
                    {
                        position.x += Program.random.Next(-1, 0);
                        position.y += Program.random.Next(0, 2);
                        break;
                    }
                case "WanderSouth":
                    {
                        position.x += Program.random.Next(-1, 2);
                        position.y += Program.random.Next(0, 2);
                        break;
                    }
                case "WanderSouthWest":
                    {
                        position.x += Program.random.Next(0, 2);
                        position.y += Program.random.Next(0, 2);
                        break;
                    }
                case "WanderWest":
                    {
                        position.x += Program.random.Next(0, 2);
                        position.y += Program.random.Next(-1, 2);
                        break;
                    }
                case "WanderNorthWest":
                    {
                        position.x += Program.random.Next(0, 2);
                        position.y += Program.random.Next(-1, 0);
                        break;
                    }
            }

            onParticleMove?.Invoke(position);

            life--;
            if (life <= 0)
            {
                KillParticle(position);
                return;
            }
            else
            {
                SetParticleInPlace(position);
            }
        }
        public void SetParticleInPlace(Vector position)
        {
            if (Math.CheckBounds(position.x, position.y))
            {
                Program.sfx[position.x, position.y] = this;
            }
        }
        public void Draw(Vector position)
        {
            if (life != 0)
            {
                onParticleDraw?.Invoke();
                GetComponent<Draw>().DrawToScreen(Program.mapConsole, position.x, position.y);
            }
            else
            {
                KillParticle(position);
            }
        }
        public void KillParticle(Vector position)
        {
            Program.rootConsole.particles.Remove(this);
            onParticleDeath?.Invoke(position);
        }
        public Particle(int life, int speed, string movement, Vector target = null, bool alwaysVisible = false, bool showOverActors = false)
        {
            this.life = life;
            this.speed = speed;
            this.movement = movement;
            this.alwaysVisible = alwaysVisible;
            this.showOverActors = showOverActors;

            if (target != null)
            {
                if (movement == "Target")
                {
                    AddComponent(new Movement(new List<int>() { 1, 2, 3 }));
                    DijkstraMap.CreateMap(new List<Vector>() { target }, "ParticlePath");
                }
                else if (movement == "Attached")
                {
                    this.target = target;
                }
            }

            SetDelegates();
        }
        public Particle(Particle copy)
        {
            this.life = copy.life;
            this.speed = copy.speed;
            this.movement = copy.movement;
            this.alwaysVisible = copy.alwaysVisible;
            if (copy.target != null) 
            {
                if (copy.movement == "Target")
                {
                    AddComponent(new Movement(new List<int>() { 1, 2, 3 }));
                    DijkstraMap.CreateMap(new List<Vector>() { copy.target }, "ParticlePath");
                }
                else if (copy.movement == "Attached")
                {
                    this.target = target;
                }
            }

            Draw draw = copy.GetComponent<Draw>();
            Draw newDraw = new Draw(draw);
            AddComponent(newDraw);


            foreach (Component component in copy.components)
            {
                if (component != null && component.GetType() != typeof(Draw))
                {
                    Component copiedComponent = component as Component;
                    AddComponent(copiedComponent);
                }
            }
        }
    }
}
