using System.Collections.Generic;

namespace AI.GOAP
{
    public class ActionPlan
    {
        public Goal AgentGoal { get; private set; }
        public Stack<AgentAction> Actions { get; private set; }
        public float TotalCost { get; set; }

        // Default constructor for pool
        public ActionPlan() { }

        // Constructor when used explicitly (keeping for compatibility)
        public ActionPlan(Goal goal, Stack<AgentAction> actions, float totalCost)
        {
            Initialize(goal, actions, totalCost);
        }

        // Initialize for pooled instances
        public void Initialize(Goal goal, Stack<AgentAction> actions, float totalCost)
        {
            AgentGoal = goal;
            Actions = actions;
            TotalCost = totalCost;
        }

        // Reset for pool reuse
        public void Reset()
        {
            if (Actions != null)
            {
                GoapPlanner.ReturnActionStackToPool(Actions);
                Actions = null;
            }
            AgentGoal = null;
            TotalCost = 0;
        }

        // Release back to pool
        public void Release()
        {
            GoapPlanner.ReturnActionPlanToPool(this);
        }
    }
}