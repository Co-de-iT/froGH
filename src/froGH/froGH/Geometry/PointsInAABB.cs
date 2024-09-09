using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class PointsInAABB : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointsInAABB class.
        /// </summary>
        public PointsInAABB()
          : base("Points In AABB", "f_PtsAABB",
              "Detects Points within a Bounding Box\nUse this in case of dynamic sets to search\nFor fixed sets, use the RTree version",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Bounding Box", "B", "The World XYZ aligned Bounding Box for Search", GH_ParamAccess.item);
            pManager.AddPointParameter("Cloud to Search", "C", "Cloud of Points to search", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "The Points within the sphere radius", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Points indices", "i", "Indices of the points in the original list", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Box box = Box.Unset;
            if (!DA.GetData(0, ref box)) return;

            List<Point3d> points = new List<Point3d>();
            DA.GetDataList(1, points);
            if (points == null || points.Count == 0) return;

            List<Point3d> pointsIn = new List<Point3d>();
            List<int> indexesIn = new List<int>();

            RTree tree = RTree.CreateFromPointArray(points);

            // perform search
            tree.Search(box.BoundingBox, (sender, e) =>
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
                return Resources.PointsInAABB_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("805CA143-D955-43FE-930B-799A1B46EF07"); }
        }
    }
}