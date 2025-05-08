
using System.Collections.Generic;

namespace AI.GOAP.Helpers
{
    public class Node
    {
        public Node Parent { get; private set; }
        public AgentAction Action { get; private set; }
        public HashSet<Belief> RequiredEffects { get; set; }
        public List<Node> Leaves { get; private set; }
        public float Cost { get; private set; }

        //  Is leaf has no children and no action
        public bool IsLeafDead => Leaves.Count == 0 && Action == null;

        // Default constructor for pool
        public Node()
        {
        }

        // Reset for pool reuse
        public void Reset()
        {
            Parent = null;
            Action = null;
            RequiredEffects = null;
            // Return the leaves list to pool if it exists
            if (Leaves != null)
            {
                GoapPoolManager.ReturnNodeLeafListToPool(Leaves);
                Leaves = null;
            }

            Cost = 0;
        }

        // Initialize for pooled instances
        public void Initialize(Node parent, AgentAction action, HashSet<Belief> effects, float cost, bool reuseEffects = false)
        {
            Parent = parent;
            Action = action;
            RequiredEffects = reuseEffects ? effects : new HashSet<Belief>(effects);
            // Get a fresh leaves list from the pool
            Leaves = GoapPoolManager.GetNodeLeafListFromPool();
            Cost = cost;
        }

    }
}
