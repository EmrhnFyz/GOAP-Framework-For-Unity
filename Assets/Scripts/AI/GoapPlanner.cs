using System.Collections.Generic;
using AI.GOAP.Helpers;
using UnityEngine.Pool;

namespace AI.GOAP
{
    public class GoapPlanner : IGoapPlanner
    {
        private void SetGoalsWithDescendingOrder(HashSet<Goal> goals, Goal mostRecentGoal)
        {
            GoapPoolManager.OrderedGoals.Clear();
            // Filter valid goals
            foreach (Goal goal in goals)
            {
                bool hasUnmetBelief = false;
                foreach (var belief in goal.DesiredEffects)
                {
                    if (!belief.Evaluate())
                    {
                        hasUnmetBelief = true;
                        break;
                    }
                }

                if (hasUnmetBelief)
                {
                    GoapPoolManager.OrderedGoals.Add(goal);
                }
            }

            GoapPoolManager.RadixSorter.Sort(GoapPoolManager.OrderedGoals, mostRecentGoal);
        }

        private float CalculateHeuristic(Node node)
        {
            // Simple heuristic: number of unsatisfied preconditions
            return node.RequiredEffects.Count;
        }

        private float CalculateFScore(Node node)
        {
            return node.Cost + CalculateHeuristic(node);
        }

        private void ReconstructPath(Node node)
        {
            var current = node;
            while (current.Parent != null)
            {
                current.Parent.Leaves.Clear(); // Clear existing leaves
                current.Parent.Leaves.Add(current);
                current = current.Parent;
            }
        }

        private void ReleaseNodeHierarchy(Node node)
        {
            if (node == null)
            {
                return;
            }

            // Get a temporary list from the pool to avoid modification issues during iteration
            var tempLeaves = GoapPoolManager.NodeListPool.Get();
            // Copy the leaves to the temporary list
            foreach (var leaf in node.Leaves)
            {
                tempLeaves.Add(leaf);
            }

            node.Leaves.Clear(); // Clear leaves before releasing children

            // Release all child nodes recursively
            foreach (var leaf in tempLeaves)
            {
                ReleaseNodeHierarchy(leaf);
            }

            GoapPoolManager.NodeListPool.Release(tempLeaves);

            // Release this node's HashSet
            if (node.RequiredEffects != null)
            {
                GoapPoolManager.BeliefHashSetPool.Release(node.RequiredEffects);
                node.RequiredEffects = null; // Prevent double-release
            }

            // Return this node to the pool
            GoapPoolManager.ReturnNodeToPool(node);
        }

        private void ReleaseNodeHierarchyHashSets(Node node)
        {
            if (node == null)
            {
                return;
            }

            // Release this node's HashSet
            if (node.RequiredEffects != null)
            {
                GoapPoolManager.BeliefHashSetPool.Release(node.RequiredEffects);
                node.RequiredEffects = null; // Prevent double-release
            }

            // Release all child nodes' HashSets
            foreach (var leaf in node.Leaves)
            {
                ReleaseNodeHierarchyHashSets(leaf);
            }
        }

        public static void ReturnActionStackToPool(Stack<AgentAction> stack)
        {
            if (stack != null)
            {
                GoapPoolManager.ActionStackPool.Release(stack);
            }
        }

        public static void ReturnActionPlanToPool(ActionPlan plan)
        {
            if (plan != null)
            {
                GoapPoolManager.ActionPlanPool.Release(plan);
            }
        }

        // Add this helper method to check if a node is part of the solution path
        private bool IsNodeInSolutionPath(Node node, Node startNode)
        {
            // If this is the start node, it's definitely in the path
            if (node == startNode)
            {
                return true;
            }
            // Check if this node is a leaf in any node in the solution path
            Node currentNode = startNode;
            while (currentNode != null && currentNode.Leaves.Count > 0)
            {
                if (currentNode.Leaves.Contains(node))
                {
                    return true;
                }
                currentNode = currentNode.Leaves[0]; // Follow the solution path
            }

            return false;
        }

        private void RemoveSatisfiedEffects(Node node)
        {
            var satisfiedEffects = GoapPoolManager.BeliefHashSetPool.Get();

            foreach (var effect in node.RequiredEffects)
            {
                if (effect.Evaluate())
                {
                    satisfiedEffects.Add(effect);
                }
            }

            foreach (var effect in satisfiedEffects)
            {
                node.RequiredEffects.Remove(effect);
            }

            GoapPoolManager.BeliefHashSetPool.Release(satisfiedEffects);
        }

        private void CleanupNodesOutsideSolutionPath(Node startNode, List<Node> createdNodes)
        {
            for (int i = 0; i < createdNodes.Count; i++)
            {
                Node node = createdNodes[i];
                // Skip nodes that are part of the solution path
                if (IsNodeInSolutionPath(node, startNode))
                {
                    continue;
                }
                if (node.RequiredEffects != null)
                {
                    GoapPoolManager.BeliefHashSetPool.Release(node.RequiredEffects);
                    node.RequiredEffects = null;
                }
                GoapPoolManager.ReturnNodeToPool(node);
            }
        }

        private void ExploreNeighborNodes(Node current, HashSet<AgentAction> availableActions, PriorityQueue<Node> openSet, HashSet<Node> closedSet, List<Node> createdNodes)
        {
            foreach (var action in availableActions)
            {
                // Skip if action doesn't help satisfy any required effects
                if (!IsActionRelevant(action, current.RequiredEffects))
                {
                    continue;
                }

                var neighbor = CreateNeighborNode(current, action, createdNodes);

                if (closedSet.Contains(neighbor))
                {
                    // Return resources if neighbor is discarded
                    DiscardNeighborNode(neighbor, createdNodes);
                    continue;
                }

                float tentativeGScore = current.Cost + action.Cost;
                float fScore = tentativeGScore + CalculateHeuristic(neighbor);

                openSet.Enqueue(neighbor, fScore);
            }
        }

        private bool IsActionRelevant(AgentAction action, HashSet<Belief> requiredEffects)
        {
            foreach (var effect in action.Effects)
            {
                if (requiredEffects.Contains(effect))
                {
                    return true;
                }
            }
            return false;
        }

        private Node CreateNeighborNode(Node current, AgentAction action, List<Node> createdNodes)
        {
            // Reuse a pooled HashSet
            var newRequiredEffects = GoapPoolManager.BeliefHashSetPool.Get();

            foreach (var effect in current.RequiredEffects)
            {
                newRequiredEffects.Add(effect);
            }
            foreach (var effect in action.Effects)
            {
                newRequiredEffects.Remove(effect);
            }
            foreach (var precondition in action.Preconditions)
            {
                newRequiredEffects.Add(precondition);
            }

            // Create neighbor node
            var neighbor = GoapPoolManager.GetNodeFromPool();
            neighbor.Initialize(current, action, newRequiredEffects, current.Cost + action.Cost, reuseEffects: true);

            // Track this new node
            createdNodes.Add(neighbor);

            return neighbor;
        }

        private void DiscardNeighborNode(Node neighbor, List<Node> createdNodes)
        {
            GoapPoolManager.BeliefHashSetPool.Release(neighbor.RequiredEffects);
            createdNodes.RemoveAt(createdNodes.Count - 1); // Remove from tracking
            GoapPoolManager.ReturnNodeToPool(neighbor);
        }

        private void CleanupAllNodes(List<Node> createdNodes, int startIndex)
        {
            for (int i = startIndex; i < createdNodes.Count; i++)
            {
                if (createdNodes[i].RequiredEffects != null)
                {
                    GoapPoolManager.BeliefHashSetPool.Release(createdNodes[i].RequiredEffects);
                    createdNodes[i].RequiredEffects = null;
                }
                GoapPoolManager.ReturnNodeToPool(createdNodes[i]);
            }
        }

        private Node CreateGoalNode(Goal goal)
        {
            var desiredEffectsPool = GoapPoolManager.BeliefHashSetPool.Get();

            foreach (var desiredEffect in goal.DesiredEffects)
            {
                desiredEffectsPool.Add(desiredEffect);
            }

            // Create the goal node using the pooled hashset (avoid internal copy)
            Node goalNode = GoapPoolManager.GetNodeFromPool();
            goalNode.Initialize(null, null, desiredEffectsPool, 0, reuseEffects: true);

            return goalNode;
        }

        private bool IsGoalViable(Node goalNode)
        {
            // If the goalNode has no leaves and no action to perform, it's not viable
            return !goalNode.IsLeafDead;
        }

        private void CleanupGoalResources(Node goalNode)
        {
            if (goalNode.RequiredEffects != null)
            {
                GoapPoolManager.BeliefHashSetPool.Release(goalNode.RequiredEffects);
            }
            GoapPoolManager.ReturnNodeToPool(goalNode);
        }

        private ActionPlan CreateActionPlan(Node goalNode, Goal goal)
        {
            var actionStack = BuildActionStack(goalNode);
            Node finalNode = goalNode; // Keep a reference to the last node

            // Create the action plan
            var plan = GoapPoolManager.ActionPlanPool.Get();
            plan.Initialize(goal, actionStack, goalNode.Cost);

            // Clean up node hierarchy now that we're done with it
            ReleaseNodeHierarchy(finalNode);

            return plan;
        }

        private Stack<AgentAction> BuildActionStack(Node goalNode)
        {
            var actionStack = GoapPoolManager.ActionStackPool.Get();
            Node currentNode = goalNode;

            while (currentNode.Leaves.Count > 0)
            {
                Node cheapestLeaf = FindCheapestLeaf(currentNode);
                currentNode = cheapestLeaf;
                actionStack.Push(currentNode.Action);
            }

            return actionStack;
        }

        private Node FindCheapestLeaf(Node node)
        {
            Node cheapestLeaf = node.Leaves[0];
            float minCost = cheapestLeaf.Cost;

            for (int i = 1; i < node.Leaves.Count; i++)
            {
                if (node.Leaves[i].Cost < minCost)
                {
                    minCost = node.Leaves[i].Cost;
                    cheapestLeaf = node.Leaves[i];
                }
            }

            return cheapestLeaf;
        }

        private bool FindPath(Node startNode, HashSet<AgentAction> availableActions)
        {
            // Get the open set from a pool.
            var openSet = PriorityQueuePool<Node>.Get();
            // Get the closed set from the generic pool.
            var closedSet = GoapPoolManager.NodeHashSetPool.Get();

            // Create a list to track all created nodes for cleanup
            var createdNodes = GoapPoolManager.NodeListPool.Get();
            createdNodes.Add(startNode);

            try
            {
                openSet.Enqueue(startNode, CalculateFScore(startNode));

                while (openSet.Count > 0)
                {
                    var current = openSet.Dequeue();
                    closedSet.Add(current);

                    // Remove satisfied effects
                    RemoveSatisfiedEffects(current);

                    // Check if we reached the goal
                    if (current.RequiredEffects.Count == 0)
                    {
                        ReconstructPath(current);
                        // Release all HashSets from nodes that aren't part of the final path
                        CleanupNodesOutsideSolutionPath(startNode, createdNodes);

                        return true;
                    }
                    ExploreNeighborNodes(current, availableActions, openSet, closedSet, createdNodes);
                }

                // No plan found - clean up all nodes except startNode
                CleanupAllNodes(createdNodes, startIndex: 1); // Skip startNode

                return false;
            }
            finally
            {
                PriorityQueuePool<Node>.Return(openSet);
                GoapPoolManager.NodeHashSetPool.Release(closedSet);
                GoapPoolManager.NodeListPool.Release(createdNodes);
            }
        }

        public ActionPlan Plan(GoapAgent agent, HashSet<Goal> goals, Goal mostRecentGoal = null)
        {
            // Order the goals by priority, descending
            SetGoalsWithDescendingOrder(goals, mostRecentGoal);

            // Try to solve each goal in order
            foreach (var goal in GoapPoolManager.OrderedGoals)
            {
                Node goalNode = CreateGoalNode(goal);

                // If we can find a path to the goal, return the plan
                if (FindPath(goalNode, agent.Actions))
                {
                    if (!IsGoalViable(goalNode))
                    {
                        CleanupGoalResources(goalNode);
                        continue;
                    }

                    var plan = CreateActionPlan(goalNode, goal);
                    return plan;
                }
                else
                {
                    // Return the pooled hashset if no plan was found
                    CleanupGoalResources(goalNode);
                }
            }
            return null;
        }

    }
}
