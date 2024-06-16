using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class PlaneFromLPolyline : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PlaneFromLPolyline class.
        /// </summary>
        public PlaneFromLPolyline()
          : base("Plane From L Polyline", "f_PLP",
              "Generates a plane from an L shaped polyline (X-O-Y order)",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "The L-shaped Polyline", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "The corresponding Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Polyline P = null;
            Curve C = null;
            if (!DA.GetData(0, ref C)) return;
            C.TryGetPolyline(out P);
            if (P == null || P.Count != 3) return;

            Plane Pl = new Plane(P[1], P[0], P[2]);

            DA.SetData(0, Pl);
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
                return Resources.PlaneFromLPolyline_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("928ca0d9-e8e0-408f-a9c4-f20e3fa856fe"); }
        }
    }
}