using Rhino.Collections;
using Rhino.Geometry;
using System.Collections.Generic;

namespace froGH.Utils
{
    public class FroGHRTree
    {
        public RTree Tree
        { get; set; }

        public Point3dList Points
        { get; set; }

        public EnumRTreeType Source
        { get; set; }

        public FroGHRTree()
        { }

        public FroGHRTree(List<Point3d> points)
        {
            Points = new Point3dList();
            Source = EnumRTreeType.Points;
            Tree = new RTree();
            for (int i = 0; i < points.Count; i++)
            {
                Points.Add(points[i]);
                Tree.Insert(points[i], i);
            }
        }

        public FroGHRTree(PointCloud cloud)
        {
            Points = new Point3dList(cloud.GetPoints());
            Source = EnumRTreeType.PointCloud;
            Tree = RTree.CreatePointCloudTree(cloud);

        }

        public FroGHRTree(Mesh mesh)
        {
            Point3d[] vertices = mesh.Vertices.ToPoint3dArray();
            Points = new Point3dList(vertices);
            Source = EnumRTreeType.Mesh;
            Tree = RTree.CreateFromPointArray(vertices);

        }

        public int ClosestPointIndex(Point3d searchPoint)
        {
            ClosestPointSearchData closestPointSearchData = new ClosestPointSearchData(Points);
            Tree.Search(new Sphere(searchPoint, searchPoint.DistanceTo(Points[0] * 1.1)), ClosestPointCallback, closestPointSearchData);
            return closestPointSearchData.Index;
        }

        private static void ClosestPointCallback(object sender, RTreeEventArgs e)
        {
            ClosestPointSearchData closestPointSearchData = e.Tag as ClosestPointSearchData;
            if (closestPointSearchData != null)
            {
                Point3d samplePoint = closestPointSearchData.Points[e.Id];
                Sphere searchSphere = e.SearchSphere;
                double distToCenter = samplePoint.DistanceTo(searchSphere.Center);
                if (closestPointSearchData.Index == -1 || distToCenter < closestPointSearchData.Distance)
                {
                    searchSphere = e.SearchSphere;
                    e.SearchSphere = new Sphere(searchSphere.Center, distToCenter);
                    closestPointSearchData.Distance = distToCenter;
                    closestPointSearchData.Index = e.Id;
                }
            }
        }

        public List<int> IndicesInSphere(Point3d center, double radius)
        {
            RTSearchData distanceSearchData = new RTSearchData();
            Tree.Search(new Sphere(center, radius), SearchCallback, distanceSearchData);
            return distanceSearchData.Ids;
        }

        public List<int> IndicesInAABB(BoundingBox bb)
        {
            RTSearchData distanceSearchData = new RTSearchData();
            Tree.Search(bb, SearchCallback, distanceSearchData);
            return distanceSearchData.Ids;
        }

        private static void SearchCallback(object sender, RTreeEventArgs e)
        {
            RTSearchData distanceSearchData = e.Tag as RTSearchData;
            distanceSearchData.Ids.Add(e.Id);
        }

    }
}
