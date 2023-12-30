using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class LPolylineFromPlane : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LPolylineFromPlane class.
        /// </summary>
        public LPolylineFromPlane()
          : base("L Polyline From Plane", "f_LPP",
              "Generates an L shaped polyline (X-O-Y order) from a Plane",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Input Plane", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "l", "Length of polyline segments", GH_ParamAccess.item, 1);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "The L-shaped Polyline", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane p = new Plane();

            if (!DA.GetData(0, ref p)) return;

            double length = 1;
            DA.GetData(1, ref length);

            Polyline poly = new Polyline();

            poly.Add(p.Origin + p.XAxis * length);
            poly.Add(p.Origin);
            poly.Add(p.Origin + p.YAxis * length);

            DA.SetData(0, poly);
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
                return Resources.LPolylineFromPlane_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3631ECF7-5231-42B5-81F6-D1725750A7A4"); }
        }
    }
}