using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Collections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace froGH
{
    public class ClusterByDistance : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ClusterByDistance class.
        /// </summary>
        public ClusterByDistance()
          : base("Clusterize Points By Distance", "f_PtsCBD",
              "Clusterize a list of points by distance",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to cluster", GH_ParamAccess.list);
            pManager.AddNumberParameter("Weights", "W", "Point Weights (optional)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Index", "i", "Index of start point for clusterization", GH_ParamAccess.item);
            pManager.AddNumberParameter("Clusterization distance", "d", "Clusterization distance (cluster radius)", GH_ParamAccess.item);

            pManager[1].Optional = true; // weights are optional
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Clustered Points", "P", "Clustered Points", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Clustered Indices", "i", "Clustered Indices", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /*code by Andrea Graziano and Alessio Erioli*/

            List<Point3d> P = new List<Point3d>();
            if (!DA.GetDataList(0, P)) return;
            if (P == null || P.Count == 0) return;

            List<double> W = new List<double>();
            DA.GetDataList(1, W);
            // if no input weights, create a list of ones
            if (W == null || W.Count == 0)
                W = P.Select(p => 1.0).ToList();

            int i = 0;
            DA.GetData(2, ref i);

            double d = 0;
            DA.GetData(3, ref d);
            if (d == 0) return;

            List<Point3d> points = P.Select(p => new Point3d(p)).ToList();
            List<int> pInd = new List<int>();
            for (int j = 0; j < points.Count; j++) pInd.Add(j);

            double distsq = (d * d);

            // initialize clusters
            List<Cluster> clusters = new List<Cluster>();
            // make sure index does not overshoot number of points
            i %= points.Count;

            Cluster currentCluster = new Cluster(points[i], i, W[i]);
            points.RemoveAt(i);
            pInd.RemoveAt(i);
            W.RemoveAt(i);

            for (int j = 1; j < P.Count; j++)
            {
                int index = Point3dList.ClosestIndexInList(points, currentCluster.centroid);

                if (!currentCluster.TryAddPoint(points[index], pInd[index], W[index], distsq))
                {
                    clusters.Add(currentCluster);
                    currentCluster = new Cluster(points[index], pInd[index], W[index]);
                }
                points.RemoveAt(index);
                pInd.RemoveAt(index);
                W.RemoveAt(index);
            }

            clusters.Add(currentCluster);

            //clustersTree = ConvertToTree(clusters);

            DA.SetDataTree(0, ConvertToTree(clusters));
            DA.SetDataTree(1, ConvertIndToTree(clusters));
        }

        //private List<double> NormalizedWeights(List<double> weights, int nPoints)
        //{
        //    List<double> normalizedWeights = new List<double>();

        //    double min = weights.Min();
        //    double max = weights.Max();
        //    double invSpan = 1.0 / (max - min);

        //    if (weights.Count < nPoints)
        //    {
        //        int nWeights = weights.Count;
        //        for (int i = 0; i < nPoints - nWeights; i++)
        //            weights.Add(weights[nWeights]);
        //    }

        //    for (int i = 0; i < nPoints; i++)
        //        normalizedWeights.Add((weights[i] - min) * invSpan);

        //    return normalizedWeights;
        //}

        public class Cluster
        {
            public List<Point3d> points;
            public List<int> indexes;
            public List<double> weights;
            public Point3d centroid;

            public Cluster()
            {
                points = new List<Point3d>();
                indexes = new List<int>();
                weights = new List<double>();
                centroid = Point3d.Unset;
            }

            public Cluster(Point3d point, int ind):this(point, ind, 1.0)
            {
            }
            public Cluster(Point3d point, int ind, double weight)
            {
                points = new List<Point3d>();
                indexes = new List<int>();
                weights = new List<double>();
                points.Add(point);
                indexes.Add(ind);
                weights.Add(weight);
                centroid = point;
            }

            private void AddPoint(Point3d p, int ind, double weight)
            {
                points.Add(p);
                indexes.Add(ind);
                weights.Add(weight);
                ComputeCentroidWeighted();
            }

            public bool TryAddPoint(Point3d p, int ind, double weight, double distsq)
            {

                double dist = centroid.DistanceToSquared(p);
                if (dist < distsq)
                {
                    AddPoint(p, ind, weight);
                }
                else return false;

                if (!CheckRange(distsq))
                {
                    points.RemoveAt(points.Count - 1);
                    indexes.RemoveAt(indexes.Count - 1);
                    weights.RemoveAt(weights.Count - 1);
                    ComputeCentroidWeighted();
                    return false;
                }
                return true;
            }

            private bool CheckRange(double distsq)
            {

                for (int i = 0; i < points.Count; i++)
                {
                    if (centroid.DistanceToSquared(points[i]) > distsq)
                        return false;
                }
                return true;
            }

            private void ComputeCentroidWeighted()
            {
                centroid = new Point3d();
                double sumWeights = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    centroid += (points[i]*weights[i]);
                    sumWeights += weights[i];
                }
                centroid /= sumWeights;
            }

        }


        DataTree<Point3d> ConvertToTree(List<Cluster> clusters)
        {
            DataTree<Point3d> clustersPointsTree = new DataTree<Point3d>();

            for (int i = 0; i < clusters.Count; i++)
                clustersPointsTree.AddRange(clusters[i].points, new GH_Path(i));

            return clustersPointsTree;
        }

        DataTree<int> ConvertIndToTree(List<Cluster> clusters)
        {
            DataTree<int> clustersIndexesTree = new DataTree<int>();

            for (int i = 0; i < clusters.Count; i++)
                clustersIndexesTree.AddRange(clusters[i].indexes, new GH_Path(i));

            return clustersIndexesTree;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.ClusterizeByDistance_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C0C36EE6-1CD3-4FEE-A4FF-498C0CAF2D34"); }
        }
    }
}