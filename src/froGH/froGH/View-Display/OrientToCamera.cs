using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class OrientToCamera : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the OrientToCamera class.
        /// </summary>
        public OrientToCamera()
          : base("Orient To Camera", "f_O2Cam",
              "Creates a Camera-aligned Plane centered on a given point",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Base Point", "P", "Base point for new plane", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Refresh", "R", "Refresh plane\nUse a button for manual refresh, a toggle + timer for auto-refresh", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Camera Aligned Plane", "P", "Camera Aligned Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d Pt = new Point3d();

            if (!DA.GetData(0, ref Pt)) return;
            bool refresh = false;
            DA.GetData(1, ref refresh);

            Message = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.Name;

            Plane p;

            if (Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.GetCameraFrame(out p))
                DA.SetData(0,new Plane(new Point3d(Pt.X, Pt.Y, Pt.Z), p.XAxis, p.YAxis));

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
                return Resources.Orient_to_Cam_3_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1fb86a3c-d6fd-406b-b765-11e0c05d6121"); }
        }
    }
}