using System.Collections.Generic;
using System.Linq;

namespace froGH.Utils
{
    // implemented from:
    // https://www.youtube.com/watch?v=LwDDcHt_pmM&t=2051s
    // https://github.com/ChenChihYuan/KD_Tree_Intro
    // EXPLANATION: https://www.baeldung.com/cs/k-d-trees
    // IMPLEMENTATIONS:
    // range query, see following links:
    // https://www.cs.umd.edu/class/fall2019/cmsc420-0201/Lects/lect14-kd-query.pdf
    // https://stackoverflow.com/questions/59426224/how-to-implement-range-search-in-kd-tree
    // check the codeandcats example on GitHub: https://github.com/codeandcats/KdTree
    public class KDTree
    {
        private int k;
        private KDNode root_ = null;
        private KDNode best_ = null;
        private double bestDistance_ = 0;
        private int visited_ = 0;

        public KDTree(int dimension, List<KDNode> nodes)
        {
            k = dimension;
            root_ = MakeTree(nodes, 0);
        }

        private KDNode MakeTree(List<KDNode> nodes, int depth)
        {
            if (nodes.Count <= 0) return null;

            // 1. sort points along an axis
            int axis = depth % k;

            List<KDNode> sorted_nodes = new List<KDNode>();
            if (axis == 0)
                sorted_nodes = nodes.OrderBy(node1 => node1.pt.X).ToList();
            else if (axis == 1)
                sorted_nodes = nodes.OrderBy(node1 => node1.pt.Y).ToList();
            else if (axis == 2)
                sorted_nodes = nodes.OrderBy(node1 => node1.pt.Z).ToList();


            // 2. find the median point

            // 0 1 2 3   -> Even  Count == 4
            // 0 1 2 3 4 -> Odd   Count == 5
            //     #

            KDNode node = sorted_nodes[nodes.Count / 2];

            // pts.Add(sorted_nodes[nodes.Count / 2].pt);
            // [)

            // 3. divide the list into left/right subtrees
            List<KDNode> left = sorted_nodes.Skip(0).Take(nodes.Count / 2).ToList();
            List<KDNode> right = sorted_nodes.Skip(nodes.Count / 2 + 1).Take(nodes.Count - 1).ToList();
            node.left = MakeTree(left, depth + 1);
            node.right = MakeTree(right, depth + 1);

            return node;
        }

        public KDNode FindNearest(KDNode target)
        {
            if (root_ == null) return null;
            best_ = null;
            bestDistance_ = 0;

            Nearest(root_, target, 0);
            return best_;
        }

        private void Nearest(KDNode root, KDNode target, int index)
        {
            if (root == null) return;

            double d = root.pt.DistanceToSquared(target.pt);

            if (best_ == null || d < bestDistance_)
            {
                bestDistance_ = d;
                best_ = root;
            }
            if (bestDistance_ == 0) return;

            double dx = 0;
            if (index == 0)
                dx = root.pt.X - target.pt.X;
            else if (index == 1)
                dx = root.pt.Y - target.pt.Y;
            else if (index == 2)
                dx = root.pt.Z - target.pt.Z;

            index = (index + 1) % k;

            // if dx > 0 : target.pt is on the LEFT of the root.pt in X direction case
            // left is the negative axis and right the positive axis with respect to the root point (which is the median)
            Nearest(dx > 0 ? root.left : root.right, target, index);
            if (dx * dx >= bestDistance_) return;
            Nearest(dx > 0 ? root.right : root.left, target, index); // for the special cases
        }

        public override string ToString()
        {
            return "KD Tree";
        }
    }
}
