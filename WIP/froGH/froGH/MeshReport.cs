using System;
using System.Collections.Generic;
using System.Drawing;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshReport : GH_Component
    {
        private BoundingBox _clip;
        private List<Polyline> _curve = new List<Polyline>();

        /// <summary>
        /// Initializes a new instance of the MeshReport class.
        /// </summary>
        public MeshReport()
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
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
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

            Mesh val = M;

            string text = "";
            bool flag = true;
            text = "- Details -\n";
            if (val.DisjointMeshCount == 1)
                text += "Mesh is a single piece. \n";
            else
                text += "Mesh is composed of " + val.DisjointMeshCount.ToString() + " disjoint piece(s). \n";

            Polyline[] m_nakedEdges = val.GetNakedEdges();
            if (m_nakedEdges == null)
            {
                text += "Mesh has 0 naked edges. \n";
            }
            else
            {
                _curve.AddRange(m_nakedEdges);
                text += "Mesh has " + m_nakedEdges.Length.ToString() + " naked edges. \n";
                flag = false;
            }
            bool flag2 = default(bool);
            bool flag3 = default(bool);
            if (val.IsManifold(true, out flag2, out flag3))
            {
                text += "Mesh is manifold. \n";
            }
            else
            {
                text += "Mesh is non-manifold. \n";
                flag = false;
            }
            if (val.SolidOrientation() == 1)
            {
                text += "Mesh is solid. \n";
            }
            else if (val.SolidOrientation() == 0)
            {
                text += "Mesh is not solid. \n";
                flag = false;
            }
            else
            {
                val.Flip(true, true, true);
                text += "Mesh is solid. (normals have been flipped) \n";
            }
            text = ((!flag) ? ("Mesh is INVALID.\n\n" + text) : ("Mesh is VALID.\n\n" + text));
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