
using System;

namespace BehaviourTree
{
    public abstract class Composite : TreeNode
    {
        protected TreeNode[] children = null;

        public Composite(TreeNode[] inChildren)
        {
            children = inChildren;
        }
    }


    /** Process children, fails if any child fails */
    public class Sequence : Composite
    {
        int index = 0;
        int[] childIndices = null;
        bool randomise = false;

        public Sequence(TreeNode[] inChildren, bool inRandomise = false) : base(inChildren)
        {
            randomise = inRandomise;
            Reset();
        }

        public override NodeStatus Process(TreeState state)
        {
            if (children.Length == 0) return NodeStatus.Success;

            if (index < children.Length)
            {
                NodeStatus result = children[childIndices[index]].Process(state);
                if (result != NodeStatus.Success)
                {
                    return result;
                }
            }

            for(int i = 0; i < children.Length; i++)
            {
                NodeStatus result = children[i].Process(state);
                if(result != NodeStatus.Success)
                {
                    return result;
                }
            }
            return NodeStatus.Success;
        }

        public override void Reset()
        {
            index = 0;
        }
    }

    /** Process children until one succeeds */
    public class Selector : TreeNode
    {
        TreeNode[] children = null;
        bool randomise = false;

        Selector(TreeNode[] inChildren, bool inRandomise = false)
        {
            children = inChildren;
            randomise = inRandomise;
        }

        public override NodeStatus Process(TreeState state)
        {
            for (int i = 0; i < children.Length; i++)
            {
                NodeStatus result = children[i].Process(state);
                if (result != NodeStatus.Fail)
                {
                    return result;
                }
            }
            return NodeStatus.Fail;
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }

}