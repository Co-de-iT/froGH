using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class Turntable : GH_Component
    {
        //To get and set camera properties
        Rhino.Display.RhinoViewport vp;

        /// <summary>
        /// Initializes a new instance of the Turntable class.
        /// </summary>
        public Turntable()
          : base("Turntable", "f_TT",
              "Simulates a turntable animation around selected objects",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry for turntable", GH_ParamAccess.list);
            pManager.AddNumberParameter("Distance Multiplier", "d", "Zoom in/out", GH_ParamAccess.item, 1);
            pManager.AddIntervalParameter("Lens range", "L", "Lens range, ex. 21 to 120", GH_ParamAccess.item, new Interval(21, 120));
            pManager.AddNumberParameter("Lens interpolation parameter", "l", "0-1 parameter for the lens range", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Direction", "D", "0 CCW,  CW", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Adjust start point", "t", "Adjust start point in turntable trajectory\n0-1 parameter", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Camera tilt shift", "cT", "Slide camera along plane Z axis", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Target tilt shift", "tT", "Slide target along camera plane Z axis", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Animation parameter", "at", "Use a 0-1 slider to move around\nAnimate the slider to save animation frames", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Activate", "a", "Take control or release it back to Rhino\nuse a Toggle", GH_ParamAccess.item, false);

            for (int i = 1; i <= 9; i++) pManager[i].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddNumberParameter("Trigger", "T", "Trigger for View Capture To File", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> G = new List<GeometryBase>();
            if (!DA.GetDataList(0, G)) return;

            //Plane P = new Plane();
            //DA.GetData(1, ref P);

            double distMult = 0.0;
            DA.GetData(1, ref distMult);

            Interval lensRange = new Interval();
            DA.GetData(2, ref lensRange);

            double lensPar = 0.0;
            DA.GetData(3, ref lensPar);

            double lens = lensRange.ParameterAt(lensPar);

            int direction = 0;
            DA.GetData(4, ref direction);
            direction = direction % 2;

            double adjustStart = 0.0;
            DA.GetData(5, ref adjustStart);

            double slideCamZ = 0.0, slideTarZ = 0.0;

            DA.GetData(6, ref slideCamZ);
            DA.GetData(7, ref slideTarZ);

            double animT = 0.0;
            DA.GetData(8, ref animT);

            bool activate = false;
            DA.GetData(9, ref activate);
            if (!activate)
            {
                Message = "";
                return;
            }

            BoundingBox bb = new BoundingBox();
            foreach (GeometryBase gg in G)
                bb.Union(gg.GetBoundingBox(false));

            double d = Math.Max(bb.Diagonal.X, bb.Diagonal.Y) * 2;

            // compensate lens/distance
            double radius = d * distMult * (lens / ((lensRange[1] + lensRange[0]) * 0.5));
            Point3d center = bb.Center;
            Point3d target = center;

            center.Z += slideCamZ;
            target.Z += slideTarZ;

            double sign = direction * 2 - 1;
            double angle = Math.PI * 2 * (adjustStart + animT) * sign;
            Point3d camera = center + new Point3d(Math.Cos(angle) * radius, Math.Sin(angle) * radius, 0.0);

            Message = string.Format("{0:f1}", lens);
            //Get current viewport
            vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            //Set new camera
            vp.SetCameraLocations(target, camera);
            vp.CameraUp = Vector3d.ZAxis;
            vp.ChangeToPerspectiveProjection(true, lens);
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
                return Resources.Turntable_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0b1f0f13-93ec-463e-b0a8-aaae4a795e2a"); }
        }
    }
}