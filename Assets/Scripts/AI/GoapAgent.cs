using System.Collections.Generic;
using AI.GOAP.DTO;
using AI.GOAP.Enums;
using AI.GOAP.Sensor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace AI.GOAP
{

    /// <summary>
    /// The GoapAgent class is the base class for all agents in the GOAP system.
    /// It contains the basic structure for the agent, such as the pawn, the sensors, the beliefs, the actions and the goals.
    /// We can extend this class for create other type of agents.
    /// </summary>
    public class GoapAgent : MonoBehaviour
    {
        [FormerlySerializedAs("Id")] public int id;
        [FormerlySerializedAs("Pawn")] public Pawn pawn;
        public bool IsAlive => true; // logic to determine if the agent is alive
        [SerializeField] private List<SensorData> sensors;

        private Dictionary<SensorName, SensorBase> _sensorMap = new();

        private Goal _lastGoal;
        public Goal CurrentGoal;
        public ActionPlan ActionPlan;
        public AgentAction CurrentAction;
        protected BeliefFactory BeliefFactory;

        public Dictionary<BeliefName, Belief> Beliefs;
        public HashSet<AgentAction> Actions;
        public HashSet<Goal> Goals;
        private static ObjectPool<HashSet<Goal>> _goalHashSetPool = new(
            createFunc: () => new HashSet<Goal>(16),
            actionOnRelease: set => set.Clear(),
            defaultCapacity: 2
        );
        public static HashSet<Goal> GetGoalHashSetFromPool()
        {
            return _goalHashSetPool.Get();
        }
        public static void ReturnGoalHashSetToPool(HashSet<Goal> set)
        {
            if (set != null)
            {
                _goalHashSetPool.Release(set);
            }
        }
        private IGoapPlanner _planner;

        protected virtual void Start()
        {
            SetupDictionaries();
            SetupTimers();
            SetupBeliefs();
            SetupActions();
            SetupGoals();

            _planner = new GoapPlanner();
        }

        protected void SetupDictionaries()
        {
            foreach (var sensorData in sensors)
            {
                _sensorMap[sensorData.name] = sensorData.sensor;
            }
        }
        public SensorBase GetSensor(SensorName sensorName)
        {
            return _sensorMap[sensorName];
        }

        protected virtual void SetupTimers()
        {
            // override this method to setup timers
        }

        protected virtual void SetupBeliefs()
        {
            Beliefs = new Dictionary<BeliefName, Belief>();
            BeliefFactory = new(this, Beliefs);
        }

        protected virtual void SetupActions()
        {
            Actions = new HashSet<AgentAction>();
        }
        protected virtual void SetupGoals()
        {
            Goals = new HashSet<Goal>();
        }

        /// <summary>
        /// override this method to subscribe sensor events
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// override this method to unsubscribe sensor events
        /// </summary>
        protected virtual void OnDisable()
        {
            if (ActionPlan != null)
            {
                ActionPlan.Release();
                ActionPlan = null;
            }

        }

        protected virtual void OnDestroy()
        {
            // Make sure the action plan is released when the agent is destroyed
            if (ActionPlan != null)
            {
                ActionPlan.Release();
                ActionPlan = null;
            }
        }

        protected virtual void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            CalculatePlan();
            // Update the plan and current action if there is one
            if (CurrentAction == null)
            {
                if (ActionPlan != null && ActionPlan.Actions.Count > 0)
                {
                    CurrentGoal = ActionPlan.AgentGoal;
                    CurrentAction = ActionPlan.Actions.Pop();

                    // Verify all precondition effects are true
                    bool allPreconditionsTrue = true;
                    foreach (var precondition in CurrentAction.Preconditions)
                    {
                        var preconditionResult = precondition.Evaluate();
                        if (!preconditionResult)
                        {
                            allPreconditionsTrue = false;
                            break;
                        }
                    }

                    if (allPreconditionsTrue)
                    {
                        CurrentAction.Start();
                    }
                    else
                    {
                        CurrentAction = null;
                        CurrentGoal = null;
                    }
                }
            }

            // If we have a current action, execute it
            if (ActionPlan != null && CurrentAction != null)
            {
                CurrentAction.Update(Time.deltaTime);

                if (!CurrentAction.CanPerform)
                {
                    CurrentAction?.Stop();
                    CurrentAction = null;
                    if (ActionPlan.Actions.Count == 0)
                    {
                        _lastGoal = CurrentGoal;
                        CurrentGoal = null;
                    }
                    return;
                }

                if (CurrentGoal != null && CurrentGoal != ActionPlan.AgentGoal)
                {
                    CurrentAction.Stop();
                    CurrentAction = null;
                    return;
                }

                if (CurrentAction.Complete)
                {
                    CurrentAction.Stop();
                    CurrentAction = null;

                    if (ActionPlan.Actions.Count == 0)
                    {
                        _lastGoal = CurrentGoal;
                        CurrentGoal = null;
                    }
                }
            }
        }

        protected virtual void CalculatePlan()
        {
            // If no goals exist, don't bother planning
            if (Goals.Count == 0)
            {
                return;
            }

            HashSet<Goal> goalsToCheck;
            bool usingPooledSet = false;

            // Check if we need to filter goals by priority
            if (CurrentGoal != null)
            {
                var priorityLevel = CurrentGoal.Priority;

                // First check if any goals have higher priority before allocating a set
                bool hasHigherPriorityGoals = false;
                foreach (var goal in Goals)
                {
                    if (goal.Priority > priorityLevel)
                    {
                        hasHigherPriorityGoals = true;
                        break;
                    }
                }

                // If no higher priority goals, skip planning
                if (!hasHigherPriorityGoals)
                {
                    return;
                }

                // Get a set from the pool only if needed
                goalsToCheck = GetGoalHashSetFromPool();
                usingPooledSet = true;

                // Add only higher priority goals
                foreach (var goal in Goals)
                {
                    if (goal.Priority > priorityLevel)
                    {
                        goalsToCheck.Add(goal);
                    }
                }
            }
            else
            {
                // Use the existing Goals set directly if no current goal
                goalsToCheck = Goals;
            }

            try
            {
                var potentialPlan = _planner.Plan(this, goalsToCheck, _lastGoal);

                if (potentialPlan != null)
                {
                    if (ActionPlan != null)
                    {
                        ActionPlan.Release();
                    }

                    ActionPlan = potentialPlan;
                }
            }
            finally
            {
                // Return the set to the pool if we got one
                if (usingPooledSet)
                {
                    ReturnGoalHashSetToPool(goalsToCheck);
                }
            }
        }
    }
}
