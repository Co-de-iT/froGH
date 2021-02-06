using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class CameraControl : GH_Component
    {
        //To get and set camera properties
        Rhino.Display.RhinoViewport vp;

        /// <summary>
        /// Initializes a new instance of the CameraControl class.
        /// </summary>
        public CameraControl()
          : base("Camera Control", "f_CamCon",
              "Controls Rhino camera from Grasshopper",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Location", "C", "Camera location", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", "T", "Target location", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lens", "L", "Lens", GH_ParamAccess.item, 35);
            pManager.AddVectorParameter("Up Vector", "u", "Up Vector", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager.AddBooleanParameter("Activate", "a", "Take control or release it back to Rhino\nuse a Toggle", GH_ParamAccess.item, false);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d location = new Point3d();
            if (!DA.GetData(0, ref location)) return;

            Point3d target = new Point3d();
            if (!DA.GetData(1, ref target)) return;

            double lens = 35;
            DA.GetData(2, ref lens);

            if (lens == 0) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Lens cannot be zero");

            Vector3d up =  Vector3d.ZAxis;
            DA.GetData(3, ref up);

            bool activate = false;
            DA.GetData(4, ref activate);

            if (!activate) Message = "";
            else
            {
                Message = "active";
                //Get current viewport
                vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
                //Set new camera
                vp.SetCameraLocations(target, location);
                vp.CameraUp = up;
                vp.ChangeToPerspectiveProjection(true, lens);
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
                return Resources.control_camera_3_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("32ff86d8-69e9-46d9-9275-0a817639fa1d"); }
        }
    }
}