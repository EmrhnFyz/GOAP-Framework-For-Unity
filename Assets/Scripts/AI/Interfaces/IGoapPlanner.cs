using System.Collections.Generic;

namespace AI.GOAP
{
    public interface IGoapPlanner
    {
        ActionPlan Plan(GoapAgent agent, HashSet<Goal> goals, Goal mostRecentGoal = null);
    }
}
