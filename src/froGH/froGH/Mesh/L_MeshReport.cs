using System;
using System.Collections.Generic;
using System.Drawing;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    [Obsolete]
    public class L_MeshReport : GH_Component
    {
        private BoundingBox _clip;
        private List<Polyline> _curve = new List<Polyline>();

        /// <summary>
        /// Initializes a new instance of the MeshReport class.
        /// </summary>
        public L_MeshReport()
          : base("Mesh Report", "f_MRep",
              "Generates a report on a Mesh geometry.\nPreviews naked edges",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to process", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Report", "R", "The Report data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            if (!DA.GetData(0, ref M)) return;
            if (!M.IsValid || M == null) return;

            Mesh mesh = M;

            string text = "";
            bool printable = true;
            text = "- Details -\n";
            if (mesh.DisjointMeshCount == 1)
                text += "Mesh is a single piece.\n";
            else
                text += "Mesh is composed of " + mesh.DisjointMeshCount.ToString() + " disjoint piece(s). \n";

            Polyline[] m_nakedEdges = mesh.GetNakedEdges();
            if (m_nakedEdges == null)
            {
                text += "Mesh has 0 naked edges.\n";
            }
            else
            {
                _curve.AddRange(m_nakedEdges);
                text += "Mesh has " + m_nakedEdges.Length.ToString() + " naked edges.\n";
                printable = false;
            }
            bool isOriented = default(bool);
            bool hasBoundary = default(bool);
            if (mesh.IsManifold(true, out isOriented, out hasBoundary))
            {
                text += "Mesh is manifold.\n";
            }
            else
            {
                text += "Mesh is non-manifold.\n";
                printable = false;
            }
            switch (mesh.SolidOrientation())
            {
                case 0:
                    text += "Mesh is not solid.\n";
                    printable = false;
                    break;
                case 1:
                    text += "Mesh is solid.\n";
                    break;
                case -1:
                    text += "Mesh is solid, but normals point inward\n";
                    break;
            }
            text = ((!printable) ? ("Mesh is VALID, but NON PRINTABLE.\n\n" + text) : ("Mesh is VALID and PRINTABLE.\n\n" + text));
            text = "- Overview -\n" + text;

            DA.SetData(0, text);
        }

        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _curve.Clear();
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }


        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            for (int i = 0; i < _curve.Count; i++)
                args.Display.DrawPolyline(_curve[i], Color.Magenta, 5);
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
                return Resources.Mesh_report_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0e14bd5d-e2c3-4dbe-a41d-2028da801f99"); }
        }

    }
}