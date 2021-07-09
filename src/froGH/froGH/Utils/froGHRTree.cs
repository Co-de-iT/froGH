using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace froGH.Utils
{
    public class froGHRTree
    {
        public RTree Tree
        { get; set; }

        public Point3dList Points
        { get; set; }

        public EnumRTreeType Source
        { get; set; }

        public froGHRTree()
        { }

        public froGHRTree(List<Point3d> points)
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

        public froGHRTree(PointCloud cloud)
        {
            Points = new Point3dList(cloud.GetPoints());
            Source = EnumRTreeType.PointCloud;
            Tree = RTree.CreatePointCloudTree(cloud);

        }

        public froGHRTree(Mesh mesh)
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
                Point3d val = closestPointSearchData.Points[e.Id];
                Sphere searchSphere = e.SearchSphere;
                double num = val.DistanceTo(searchSphere.Center);
                if (closestPointSearchData.Index == -1 || num < closestPointSearchData.Distance)
                {
                    searchSphere = e.SearchSphere;
                    e.SearchSphere = new Sphere(searchSphere.Center, num);
                    closestPointSearchData.Distance = num;
                    closestPointSearchData.Index = e.Id;
                }
            }
        }

        public List<int> IndicesInSphere(Point3d center, double radius)
        {
            SphereSearchData distanceSearchData = new SphereSearchData();
            Tree.Search(new Sphere(center, radius), SphereCallback, distanceSearchData);
            return distanceSearchData.Ids;
        }

        private static void SphereCallback(object sender, RTreeEventArgs e)
        {
            SphereSearchData distanceSearchData = e.Tag as SphereSearchData;
            distanceSearchData.Ids.Add(e.Id);
        }

    }
}
