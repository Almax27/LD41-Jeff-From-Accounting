using System;

namespace BehaviourTree
{
    public enum NodeStatus
    {
        Running,
        Success,
        Fail
    }

    public abstract class TreeNode
    {
        public abstract NodeStatus Process(TreeState state);
        public abstract void Reset();
    }

    public abstract class TreeState
    {
        public Random random = new Random();
    }


    public class Tree
    {
        public TreeNode RootNode = null;

        void Process(TreeState state)
        {
            RootNode.Process(state);
        }
    }
}