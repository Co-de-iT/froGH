using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    [Obsolete]
    public class LL_ZoomToObject : GH_Component
    {
        Rhino.Display.RhinoViewport vp;

        /// <summary>
        /// Initializes a new instance of the ZoomToObject class.
        /// </summary>
        public LL_ZoomToObject()
          : base("Zoom To Object", "f_ZToObj",
              "Zoom Rhino camera to frame selected object(s)",
              "froGH", "View/Display")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to frame", GH_ParamAccess.list);
            pManager.AddNumberParameter("Zoom factor", "z", "Zoom factor\n< 0 to zoom out, > 0 to zoom in", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("Activate", "a", "Activate Zoom", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
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
            List<GeometryBase> G = new List<GeometryBase>();
            if (!DA.GetDataList(0, G)) return;
            double zoomFactor = 1.0;
            DA.GetData(1, ref zoomFactor);
            bool active = false;
            DA.GetData(2, ref active);

            if (!active || G == null) return;

            //Get current viewport
            vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;

            BoundingBox bb, bTemp;
            bb = G[0].GetBoundingBox(false);
            for (int i = 1; i < G.Count; i++)
            {
                bTemp = G[i].GetBoundingBox(false);
                bb.Union(bTemp);
            }
            if (zoomFactor < 0.0)
                bb.Inflate(bb.Diagonal.Length * -zoomFactor);

            vp.ZoomBoundingBox(bb);
            if (zoomFactor > 0.0)
                vp.Magnify(zoomFactor + 1.0, false);
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
                return Resources.Zoom_to_Object_3_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("77760196-57FD-4690-A0E3-F2B3A2B591A7"); }
        }
    }
}