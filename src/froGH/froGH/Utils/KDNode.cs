using Rhino.Geometry;

namespace froGH.Utils
{
    public class KDNode
    {
        public Point3d pt;
        public KDNode left, right;

        public KDNode(Point3d point)
        {
            pt = point;
        }
    }
}
