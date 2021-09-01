using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH.Data
{
    public class CreateRTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatRTree class.
        /// </summary>
        public CreateRTree()
          : base("Create RTree", "f_CRTree",
              "Creates an RTree from Points, PointClouds or Meshes",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Source Data", "S", "Points, PointCloud or Mesh input data", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RTree", "RT", "The generated RTree", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // The idea of using a fixed RTree (via a custom class) for faster search
            // comes from the LunchBox plugin (it's actually implemented there already)
            // I already had a component with dynamic RTree search by Sphere, it only made sense
            // to me to complete the set. Plus, I had the chance to study and learn some new tricks!

            List<IGH_Goo> list = new List<IGH_Goo>();
            List<GH_froGHRTree> froGHtrees = new List<GH_froGHRTree>();
            List<Point3d> points = new List<Point3d>();
            DA.GetDataList(0, list);

            for (int i = 0; i < list.Count; i++)
                switch (list[i].TypeName)
                {
                    case "Point":
                        Point3d p = Point3d.Unset;
                        list[i].CastTo<Point3d>(out p);
                        points.Add(p);
                        break;
                    case "Mesh":
                        Mesh m = new Mesh();
                        list[i].CastTo<Mesh>(out m);
                        froGHtrees.Add(new GH_froGHRTree(new froGHRTree(m)));
                        break;
                    case "Cloud":
                        PointCloud cloud = new PointCloud();
                        list[i].CastTo<PointCloud>(out cloud);
                        froGHtrees.Add(new GH_froGHRTree(new froGHRTree(cloud)));
                        break;

                    default:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unrecognized data type - use Points, a PointCloud or a Mesh");
                        return;
                }

            if (points.Count > 0) froGHtrees.Add(new GH_froGHRTree(new froGHRTree(points)));

            DA.SetDataList(0, froGHtrees);
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
                return Resources.Create_RTree_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b4a0e221-323a-4855-a648-c01cd841033b"); }
        }
    }
}