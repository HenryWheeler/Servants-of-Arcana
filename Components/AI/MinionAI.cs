﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Servants_of_Arcana.Components;

namespace Servants_of_Arcana
{
    public class MinionAI : AIController
    {
        public Entity master { get; set; }
        public override void PerformAction()
        {
            switch (currentState)
            {
                case State.Asleep:
                    {
                        AIActions.ActionSleep(this);
                        break;
                    }
                case State.Bored:
                    {
                        if (master != null) 
                        {
                            Vector vector = entity.GetComponent<Vector>();
                            Vector masterVector = master.GetComponent<Vector>();

                            if (Math.Distance(vector.x, vector.y, masterVector.x, masterVector.y) < 5)
                            {
                                AIActions.ActionWander(this);
                            }
                            else
                            {
                                Vector nextPosition = AStar.ReturnPath(vector, masterVector)[1].position;
                                if (nextPosition != null)
                                {
                                    entity.GetComponent<Movement>().Move(nextPosition);
                                }
                                else
                                {
                                    entity.GetComponent<TurnComponent>().EndTurn();
                                }
                            }
                        }
                        else
                        {
                            AIActions.ActionWander(this);
                        }
                        break;
                    }
                case State.Angry:
                    {
                        AIActions.ActionEngage(this);
                        break;
                    }
            }
        }
        public void SetMaster(Entity master)
        {
            this.master = master;
            string faction = master.GetComponent<Faction>().faction;
            hatedFactions.Remove(faction);
            hatedEntities.Remove(master);

            if (master.GetComponent<PlayerController>() != null) 
            {
                if (!hatedFactions.Contains("Enemy"))
                {
                    hatedFactions.Add("Enemy");
                }
                entity.GetComponent<Faction>().faction = "Player";
            }
            else
            {
                if (!hatedFactions.Contains("Player"))
                {
                    hatedFactions.Add("Player");
                }
                entity.GetComponent<Faction>().faction = "Enemy";
            }
        }
        public override void SetTransitions()
        {
            transitions = new Dictionary<StateMachine, State>
            {
                { new StateMachine(State.Asleep, Input.Noise), State.Bored },
                { new StateMachine(State.Asleep, Input.Hurt), State.Angry },
                { new StateMachine(State.Asleep, Input.Hatred), State.Angry },
                { new StateMachine(State.Angry, Input.Tired), State.Asleep },
                { new StateMachine(State.Angry, Input.Bored), State.Bored },
                { new StateMachine(State.Bored, Input.Hatred), State.Angry },
                { new StateMachine(State.Bored, Input.Hurt), State.Angry },
                { new StateMachine(State.Bored, Input.Tired), State.Asleep },
            };
            currentInput = Input.None;
        }
        public MinionAI(int baseInterest, int minDistance, int maxDistance, int abilityChance, int hate, int fear, int greed)
            : base(baseInterest, minDistance, maxDistance, abilityChance, hate, fear, greed)
        {
            SetTransitions();
            currentState = State.Bored;
        }
        public MinionAI() { }
    }
}
