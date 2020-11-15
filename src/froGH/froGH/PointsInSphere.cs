using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class PointsInSphere : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointsInSphere class.
        /// </summary>
        public PointsInSphere()
          : base("Points In Sphere", "f_PtsSph",
              "Detects Points within a Sphere of given center and radius",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Sphere Center", "C", "Center Point of the search Sphere", GH_ParamAccess.item);
            pManager.AddPointParameter("Points to Search", "P", "List of Points to search", GH_ParamAccess.list);
            pManager.AddNumberParameter("Search Radius", "R", "Search Radius", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "The Points within the sphere radius", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Points indexes", "i", "Indexes of the points in the original list", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d center = new Point3d();
            if (!DA.GetData(0, ref center)) return;

            List<Point3d> points = new List<Point3d>();
            if (!DA.GetDataList(1, points)) return;
            if (points.Count == 0) return;

            double radius = 1.0;
            DA.GetData(2, ref radius);

            List<Point3d> pointsIn = new List<Point3d>();
            List<int> indexesIn = new List<int>();

            RTree tree = new RTree();

            // populate it
            for (int i = 0; i < points.Count; i++)
                tree.Insert(points[i], i);

            // perform search
            tree.Search(new Sphere(center, radius), (sender, e) =>
            {
                pointsIn.Add(points[e.Id]);
                indexesIn.Add(e.Id);
            });

            DA.SetDataList(0, pointsIn);
            DA.SetDataList(1, indexesIn);
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
                return Resources.Points_within_sphere_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8d12577c-d4fe-4d78-8312-dd1eb17111ba"); }
        }
    }
}