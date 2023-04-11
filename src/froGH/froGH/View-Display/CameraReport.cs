using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class CameraReport : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the CameraReport class.
        /// </summary>
        public CameraReport()
          : base("CameraReport", "f_CamRep",
              "Gets information about Rhino view camera",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("View Name", "vN", "Enter a View Name for the report" +
                "\nleave empty to use current View" +
                "\ndouble click the component to update when using current View", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Update", "U", "Update Report\nUse a toggle for a one-shot update, a toggle + timer for continuous data", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
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
            string viewName = "";
            DA.GetData(0, ref viewName);
            bool update = false;
            DA.GetData(1, ref update);

            int viewIndex = Rhino.RhinoDoc.ActiveDoc.NamedViews.FindByName(viewName);

            CameraData cameraData = new CameraData();

            // if no named view is found use the active view
            if (viewIndex == -1) cameraData = new CameraData(Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport);
            else cameraData = new CameraData(Rhino.RhinoDoc.ActiveDoc.NamedViews[viewIndex].Viewport);

            if (update) ExpireSolution(true);

            //save camera location and camera target
            DA.SetData(0, cameraData.CameraLocation);
            DA.SetData(1, cameraData.CameraTarget);
            DA.SetData(2, cameraData.Camera35mmLensLength);
            DA.SetData(3, cameraData.CameraUp);
            DA.SetData(4, cameraData.NearPlane);
            DA.SetData(5, cameraData.FarPlane);
        }

        public override void CreateAttributes()
        {
            m_attributes = new CameraReport_Attributes(this);
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
            get { return new Guid("93ED06C0-63D6-4827-9A8F-3D37AFF4D9F4"); }
        }

        internal struct CameraData
        {
            internal Point3d CameraLocation, CameraTarget;
            internal double Camera35mmLensLength;
            internal Vector3d CameraUp;
            internal Plane NearPlane, FarPlane;
            internal CameraData(Rhino.Display.RhinoViewport vp)
            {
                CameraLocation = vp.CameraLocation;
                CameraTarget = vp.CameraTarget;
                Camera35mmLensLength = vp.Camera35mmLensLength;
                CameraUp = vp.CameraUp;
                vp.GetFrustumNearPlane(out NearPlane);
                vp.GetFrustumFarPlane(out FarPlane);
            }
            internal CameraData(Rhino.DocObjects.ViewportInfo vi)
            {
                CameraLocation = vi.CameraLocation;
                CameraTarget = vi.TargetPoint;
                Camera35mmLensLength = vi.Camera35mmLensLength;
                CameraUp = vi.CameraUp;
                NearPlane = vi.FrustumNearPlane;
                FarPlane = vi.FrustumFarPlane;
            }
        }
    }
}