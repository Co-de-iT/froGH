using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class IsPolylineClockwise : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IsPolylineClockwise class.
        /// </summary>
        public IsPolylineClockwise()
          : base("Is Polyline Clockwise", "f_PlCW",
              "Determines if a Polyline direction is clockwise in its plane",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "The Polyline to analyze", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Is clockwise", "Cw", "true if CW, false if CCW", GH_ParamAccess.item);
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
            if (P == null || P.Count < 3) return;

            DA.SetData(0, isClockWise(P));
        }

        /*
        code from https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        Implemented in VBNet by Mateusz Zwierzycki
        */

        public bool isClockWise(Polyline poly)
        {
            double sum = 0;
            foreach (Line s in poly.GetSegments())
                sum += (s.To.X - s.From.X) * (s.To.Y + s.From.Y);

            return sum > 0;
        }

        /// <summary>
        /// Exposure override for position in the SUbcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
                return Resources.Is_Polyline_Clockwise_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f25b021f-56b8-44ce-86d0-ed9965bcadf3"); }
        }
    }
}