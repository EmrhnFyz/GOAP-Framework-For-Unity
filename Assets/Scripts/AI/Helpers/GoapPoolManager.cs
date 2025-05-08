using System.Collections.Generic;
using AI.GOAP.Helpers;
using UnityEngine.Pool;

namespace AI.GOAP
{
    public static class GoapPoolManager
    {
        public static readonly PriorityBasedRadixSorter<Goal> RadixSorter = new();
        public static List<Goal> OrderedGoals = new(64);

        public static ObjectPool<HashSet<Belief>> BeliefHashSetPool = new(
            createFunc: () => new HashSet<Belief>(128),
            actionOnRelease: set => set.Clear(),
            defaultCapacity: 128
        );

        public static ObjectPool<HashSet<Node>> NodeHashSetPool = new(
            createFunc: () => new HashSet<Node>(128),
            actionOnRelease: set => set.Clear(),
            defaultCapacity: 128
        );

        public static ObjectPool<List<Node>> NodeListPool = new(
        createFunc: () => new List<Node>(64),
        actionOnRelease: list => list.Clear(),
        defaultCapacity: 32
        );

        public static ObjectPool<Stack<AgentAction>> ActionStackPool = new(
            createFunc: () => new Stack<AgentAction>(8),
            actionOnRelease: stack => stack.Clear(),
            defaultCapacity: 16
        );

        public static ObjectPool<ActionPlan> ActionPlanPool = new(
            createFunc: () => new ActionPlan(),
            actionOnRelease: plan => plan.Reset(),
            defaultCapacity: 16
        );

        private static ObjectPool<Node> _nodePool = new(
            createFunc: () => new Node(),
            actionOnRelease: node => node.Reset(),
            defaultCapacity: 64
        );

        private static ObjectPool<List<Node>> _nodeLeafListPool = new(
            createFunc: () => new List<Node>(32),
            actionOnRelease: list => list.Clear(),
            defaultCapacity: 128
        );

        public static List<Node> GetNodeLeafListFromPool()
        {
            return _nodeLeafListPool.Get();
        }
        public static void ReturnNodeLeafListToPool(List<Node> list)
        {
            if (list != null)
            {
                _nodeLeafListPool.Release(list);
            }
        }

        public static Node GetNodeFromPool()
        {
            return _nodePool.Get();
        }

        public static void ReturnNodeToPool(Node node)
        {
            if (node != null)
            {
                _nodePool.Release(node);
            }
        }
    }
}