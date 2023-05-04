using Newtonsoft.Json.Bson;
using Servants_of_Arcana.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public abstract class AIController : Controller
    {
        public AIController(int baseInterest, int minDistance, int maxDistance, int abilityChance, int hate, int fear, int greed)
        {
            this.baseInterest = baseInterest;
            interest = baseInterest;
            this.minDistance = minDistance;
            this.maxDistance = maxDistance;
            this.abilityChance = abilityChance;
            this.hate = hate;
            this.fear = fear;
            this.greed = greed;
        }
        public AIController() { }
        public enum State
        {
            Asleep,
            Angry,
            Fearful,
            Curious,
            Awake,
            Bored,
            Recall
        }
        public enum Input
        {
            Noise,
            Hunger,
            Hurt,
            Recall,
            Tired,
            Hatred,
            Bored,
            Fear,
            Greed,
            None
        }
        public class StateMachine
        {
            readonly State currentState;
            readonly Input currentInput;
            public StateMachine(State _state, Input _input)
            {
                currentState = _state;
                currentInput = _input;
            }
            public override int GetHashCode()
            {
                return 17 + 31 * currentState.GetHashCode() + 31 * currentInput.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                StateMachine other = obj as StateMachine;
                return other != null && currentState == other.currentState && currentInput == other.currentInput;
            }
        }
        public Dictionary<StateMachine, State> transitions = new Dictionary<StateMachine, State>();
        public State currentState = State.Asleep;
        public Input currentInput = Input.None;
        public List<Entity> hatedEntities = new List<Entity>();
        public List<string> hatedFactions = new List<string>();
        public int dungeonSection;
        public Entity target { get; set; }
        public int interest { get; set; }
        public int baseInterest { get; set; }
        public int maxDistance { get; set; }
        public int minDistance { get; set; }
        /// <summary>
        /// The level of hate the AI has. 
        /// A higher level will lead to the AI being far more aggressive and less likely to flee.
        /// AI with a hate of zero will never fight.
        /// </summary>
        public int hate { get; set; }
        /// <summary>
        /// The level of fear the AI has. 
        /// A higher level will lead to the AI being far more likely to flee if it feels an enemy is more powerful than it. 
        /// AI with a fear of zero will never run.
        /// </summary>
        public int fear { get; set; }
        /// <summary>
        /// The level of greed the AI has. 
        /// A higher level will lead to the AI being more likely to attempt to grab items within its environment. 
        /// AI with a greed of zero will never attempt to grab items.
        /// </summary>
        public int greed { get; set; }
        /// <summary>
        ///The chance for the AI to use an ability each round
        /// </summary>
        public int abilityChance { get; set; }
        public override void Execute()
        {
            try
            {
                if (transitions.Count == 0)
                {
                    throw new Exception("Entity transition count equals zero.");
                }

                State tempRecord = currentState;

                Observe();
                StateMachine stateMachine = new StateMachine(currentState, currentInput);
                if (transitions.ContainsKey(stateMachine))
                {
                    currentState = transitions[stateMachine];
                    currentInput = Input.None;
                }

                if (tempRecord != currentState)
                {
                    ParticleManager.CreateAIStateParticle(currentState, entity.GetComponent<Vector>());
                }

                PerformAction();
            }
            catch (Exception e)
            {
                Log.Add($"{entity.GetComponent<Description>().name} gives error: {e.Message}");
                entity.GetComponent<TurnComponent>().EndTurn();
            }
        }
        public abstract void PerformAction();
        public abstract void SetTransitions();
        private void Observe()
        {
            Input input = Input.None;
            Vector startPos = entity.GetComponent<Vector>();
            int rangeLimit = entity.GetComponent<Attributes>().sight;

            for (uint octant = 0; octant < 8; octant++)
            {
                input = ComputeSight(octant, startPos.x, startPos.y, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));

                if (input != Input.None)
                {
                    break;
                }
            }

            if (input != Input.None)
            {
                currentInput = input;
                interest = baseInterest * 2;
            }
            else if (interest <= 0)
            {
                currentInput = Input.Bored;
                target = null;
            }
        }
        public Input ComputeSight(uint octant, int oX, int oY, int rangeLimit, int x, Slope top, Slope bottom)
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
                        Tile tile = Program.tiles[tx, ty];
                        if (Math.CheckBounds(tx, ty) && tile.actor != null && tile.actor != entity)
                        {
                            //Check for entities relative hatred or fear, etc, of the target
                            Input refInput = ReturnFeelings(tile.actor);
                            if (refInput != Input.None)
                            {
                                target = tile.actor;
                                return refInput;
                            }
                        }
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
                                else { ComputeSight(octant, oX, oY, rangeLimit, x + 1, top, newBottom); }
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

            return Input.None;
        }
        public Input ReturnFeelings(Entity entity)
        {
            if (entity.GetComponent<Actor>() != null)
            {
                string faction = entity.GetComponent<Faction>().faction;
                Attributes stats = entity.GetComponent<Attributes>();
                Attributes AIStats = this.entity.GetComponent<Attributes>();

                int interest = baseInterest;

                if (hatedFactions.Contains(faction) || hatedEntities.Contains(entity))
                {
                    interest -= stats.health + ((stats.strength + 1) * 3) + ((stats.intelligence + 1) * 3) + ((stats.armorValue + 1) * 3);
                    interest += AIStats.health + ((AIStats.strength + 1) * 3) + ((AIStats.intelligence + 1) * 3) + ((AIStats.armorValue + 1) * 3) + hate;

                    /*
                    foreach (string status in entity.GetComponent<Harmable>().statusEffects)
                    {
                        if (hatedEntities.Contains(status))
                        {
                            baseInterest += 25;
                        }
                        else
                        {
                            baseInterest += 5;
                        }
                    }
                    */

                    if (interest > 1 && hate > 0)
                    {
                        hatedEntities.Add(entity);
                        return Input.Hatred;
                    }
                    else if (interest - fear < 0 && fear > 0)
                    {
                        return Input.Fear;
                    }

                    return Input.None;
                }
                else
                {
                    return Input.None;
                }
            }
            else if (entity.GetComponent<Item>() != null)
            {
                Attributes stats = entity.GetComponent<Attributes>();
                if (stats != null)
                {
                    interest += (int)(stats.armorValue + stats.maxHealth + stats.intelligence + stats.strength + stats.sight + stats.maxEnergy);
                }

                //AttackFunction attackFunction = entity.GetComponent<AttackFunction>();
                //if (attackFunction != null)
                {

                }

                return Input.Greed;
            }
            else
            {
                return Input.None;
            }
        }
        private bool BlocksLight(Vector vector2)
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
