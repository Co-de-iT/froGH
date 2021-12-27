using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using froGH.Properties;

namespace froGH
{
    public class MeshWiresDisplay : GH_Component
    {
        private BoundingBox _clip;
        private List<Mesh> _meshList = new List<Mesh>();
        private Color _color;
        private int _width;

        /// <summary>
        /// Initializes a new instance of the MeshWiresDisplay class.
        /// </summary>
        public MeshWiresDisplay()
          : base("Custom Mesh Wires Display", "f_CMWD",
              "Render-compatible custom Mesh wires display",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Meshes for edges display", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "C", "Edges color", GH_ParamAccess.item, Color.Black);
            pManager.AddIntegerParameter("Width", "W", "Edges width (in pixels)", GH_ParamAccess.item, 1);

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
            List<Mesh> M = new List<Mesh>();
            if (!DA.GetDataList(0, M)) return;

            for (int i = M.Count-1; i >= 0; i--)
                if (!M[i].IsValid || M[i] == null)
                    M.RemoveAt(i);

            Color C = Color.Black;
            int W = 1;
            DA.GetData(1, ref C);
            DA.GetData(2, ref W);

            foreach (Mesh mesh in M)
                _clip = BoundingBox.Union(_clip, mesh.GetBoundingBox(false));

            _meshList = M;
            _color = C;
            _width = W;
        }


        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _meshList.Clear();
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        //Draw all wires and points in this method.
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            foreach (Mesh _m in _meshList)
                args.Display.DrawMeshWires(_m, _color, _width);
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
                return Resources.Mesh_Wires_Display_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("28f0a5e3-45b7-4555-a267-0250d867b7ea"); }
        }
    }
}