using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class CameraReport : GH_Component
    {

        Rhino.Display.RhinoViewport vp; //use to get and set rhino camera properties
        /// <summary>
        /// Initializes a new instance of the CameraReport class.
        /// </summary>
        public CameraReport()
          : base("Camera Report", "f_CamRep",
              "Gets information about Rhino active camera",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Run the report\nUse a toggle for a one-shot update, a toggle + timer for continuous data", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "C", "Camera location", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", "T", "Target location", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lens", "L", "Lens", GH_ParamAccess.item);
            pManager.AddVectorParameter("Up Vector", "u", "Up Vector", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Near Clipping Plane", "nCP", "Camera Frustum near clipping plane", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Far Clipping Plane", "fCP", "Camera Frustum far clipping plane", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            if (!DA.GetData(0, ref run)) return;

            if (run)
            {
                //Get camera
                //Get current viewport
                vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;

                //save camera location and camera target
                DA.SetData(0, vp.CameraLocation);
                DA.SetData(1, vp.CameraTarget);
                DA.SetData(2, vp.Camera35mmLensLength);
                DA.SetData(3, vp.CameraUp);
                Plane fp, np;
                vp.GetFrustumNearPlane(out np);
                vp.GetFrustumFarPlane(out fp);
                DA.SetData(4, np);
                DA.SetData(5, fp);
            }
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
                return Resources.Camera_report_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e7f6295f-86f7-44b3-84cd-7683970637a0"); }
        }
    }
}