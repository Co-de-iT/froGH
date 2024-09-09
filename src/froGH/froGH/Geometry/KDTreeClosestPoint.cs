using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class KDTreeClosestPoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the KDNearest class.
        /// </summary>
        public KDTreeClosestPoint()
          : base("KDTree Closest Point", "f_KDTCP",
              "Find the closest point from a sample point in a KDTree structure\nsee froGH-Data to create a KDTree",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("KD Tree", "KD", "The KD Tree from input points", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Point to search from", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "P", "Closest Point in KDTree", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            KDTree kdTree = null;
            if (!DA.GetData(0, ref kdTree)) return;
            Point3d tPt = new Point3d();
            if(!DA.GetData(1, ref tPt)) return;
            KDNode nearest = kdTree.FindNearest(new KDNode(tPt));
            GH_Point cPt = new GH_Point(nearest.pt);

            DA.SetData(0, cPt);
        }

        // hide component in release
#if !DEBUG
        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

#endif

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.ZZ_placeholder_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0F70FA3C-CD75-4F83-BFF3-4DEB05DF4DA2"); }
        }
    }
}