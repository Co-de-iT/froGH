using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using System.Linq;
using froGH.Properties;

namespace froGH
{
    public class PointCloudDisplay : GH_Component
    {
        private PointCloud _cloud;
        private BoundingBox _clip;
        private int _size;
        /// <summary>
        /// Initializes a new instance of the PointCloudDisplay class.
        /// </summary>
        public PointCloudDisplay()
          : base("PointCloud Display", "f_PCD",
              "Render-compatible Point Cloud display\n(it comes only in black)",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to Display", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Size", "S", "Display size in pixels (one value for all points)", GH_ParamAccess.item, 1);

            pManager[1].Optional = true;
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

            List<Point3d> P = new List<Point3d>();
            if (!DA.GetDataList(0, P)) return;

            if (P == null || P.Count == 0) return;

            int s = 1;
            DA.GetData(1, ref s);

            if (s < 1) return;

            _size = s;
            _cloud = new PointCloud(P);
            _clip = _cloud.GetBoundingBox(false);
        }

        /// <summary>
        /// This method will be called once every solution, before any calls to RunScript.
        /// </summary>
        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _cloud = null;
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        //Draw all wires and points in this method.
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cloud == null)
                return;
            args.Display.DrawPointCloud(_cloud, _size);
        }

        /// <summary>
        /// Exposure override for position in the SUbcategory (options primary to septenary)
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
                return Resources.Custom_PointCloud_Display_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fad51a67-3cac-4fc7-8312-68ce08b0311e"); }
        }
    }
}