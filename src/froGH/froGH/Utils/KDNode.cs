using Rhino.Geometry;

namespace froGH.Utils
{
    public class KDNode
    {
        public Point3d point;
        public KDNode left, right;

        public KDNode(Point3d point)
        {
            this.point = point;
        }
    }
}
