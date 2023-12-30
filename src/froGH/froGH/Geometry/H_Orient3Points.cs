using System;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class H_Orient3Points : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Orient3Points class.
        /// </summary>
        public H_Orient3Points()
          : base("Orient 3 Points_DEPRECATED", "f_C3PO",
              "Orient objects by sets of 3 points",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to orient", GH_ParamAccess.item);
            pManager.AddPointParameter("Source Origin", "O", "Source plane origin", GH_ParamAccess.item);
            pManager.AddPointParameter("Source X", "X", "Source plane point for X direction", GH_ParamAccess.item);
            pManager.AddPointParameter("Source Y", "Y", "Source plane point for Y direction", GH_ParamAccess.item);            
            pManager.AddPointParameter("Target Origin", "Ot", "Target plane origin", GH_ParamAccess.item);
            pManager.AddPointParameter("Target X", "Xt", "Target plane point for X direction", GH_ParamAccess.item);
            pManager.AddPointParameter("Target Y", "Yt", "Target plane point for Y direction", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to orient", GH_ParamAccess.item);
            pManager.AddTransformParameter("Transform", "X", "Transformation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase G = null;

            if (!DA.GetData(0, ref G)) return;
            if (G == null) return;
            Point3d O = new Point3d();
            Point3d X = new Point3d();
            Point3d Y = new Point3d();
            if (!DA.GetData(1, ref O)) return;
            if (!DA.GetData(2, ref X)) return;
            if (!DA.GetData(3, ref Y)) return;
            Point3d Ot = new Point3d();
            Point3d Xt = new Point3d();
            Point3d Yt = new Point3d();
            if (!DA.GetData(4, ref Ot)) return;
            if (!DA.GetData(5, ref Xt)) return;
            if (!DA.GetData(6, ref Yt)) return;

            Plane pA = new Plane(O, X, Y);
            Plane pB = new Plane(Ot, Xt, Yt);

            Transform x = Transform.PlaneToPlane(pA, pB);
            G.Transform(x);

            DA.SetData(0, G);
            DA.SetData(1, x);
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
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
                return Resources.orient3P_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("006b445b-5af4-4e31-b445-b0ac32a8f596"); }
        }
    }
}