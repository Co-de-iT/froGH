using System;
using System.Collections.Generic;
using System.Drawing;
using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    // Refactor text so that report text and data are separated by a :
    // Using a spilt text it should be possible to separate data from text
    public class MeshReport : GH_Component
    {
        private BoundingBox _clip;
        private List<Line> _nakedEdges = new List<Line>();
        private List<Line> _nonManifoldEdges = new List<Line>();

        /// <summary>
        /// Initializes a new instance of the MeshReport class.
        /// </summary>
        public MeshReport()
          : base("Mesh Report", "f_MRep",
              "Generates a report on a Mesh geometry.\nPreviews naked (Magenta) & non-manifold (Blue) edges",
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
            pManager.AddTextParameter("Report text", "Rt", "The Report text", GH_ParamAccess.list);
            pManager.AddGenericParameter("Report data", "Rd", "The Report data (in the same order as the text)", GH_ParamAccess.list);
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
            List<IGH_Goo> reportData = new List<IGH_Goo>();
            List<string> text = new List<string>();

            /*
            Mesh.Check info
            x Mesh does not have any degenerate faces.
            x Mesh does not have any ngons.
            x Mesh does not have any extremely short edges.
            x Mesh does not have any non manifold edges.
            x Mesh does not have any naked edges.
            x Mesh does not have any duplicate faces.
            ??  Mesh does not have any faces with directions different from the mesh as a whole.
            x Mesh does not have any self intersecting faces.
            x Mesh does not have any disjoint pieces.
            x Mesh does not have any unused vertices.
             */

            // these are here to avoid polluting mesh data with other queries
            // if they are moved after some other operations the data is incorrect
            int disjointMeshCount = mesh.DisjointMeshCount;
            Polyline[] m_nakedEdges = mesh.GetNakedEdges();
            Mesh manifold = mesh.ExtractNonManifoldEdges(false);


            // Check for degenerate faces
            int[] whollyDegenerateFaces, partiallyDegenerateFaces;
            mesh.Faces.GetZeroAreaFaces(out whollyDegenerateFaces, out partiallyDegenerateFaces);
            int degenerateFacesCount = whollyDegenerateFaces.Length + partiallyDegenerateFaces.Length;
            text.Add($"Mesh has {degenerateFacesCount} degenerate faces");
            reportData.Add(new GH_Integer(degenerateFacesCount));


            // Check for nGons
            text.Add($"Mesh has {mesh.Ngons.Count} nGons");
            reportData.Add(new GH_Integer(mesh.Ngons.Count));


            // Check for extremely short edges
            List<int> veryShortEdges = new List<int>();
            for (int i = 0; i < mesh.TopologyEdges.Count; i++)
            {
                if (mesh.TopologyEdges.EdgeLine(i).Length < Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                    veryShortEdges.Add(i);
            }
            text.Add($"Mesh has {veryShortEdges.Count} extremely short edges");
            reportData.Add(new GH_Integer(veryShortEdges.Count));


            // Check for non manifold edges
            int manifoldEdgesCount = 0;
            if (manifold != null)
            {
                for (int i = 0; i < manifold.TopologyEdges.Count; i++)
                    if (manifold.TopologyEdges.GetConnectedFaces(i).Length > 2)
                    {
                        _nonManifoldEdges.Add(manifold.TopologyEdges.EdgeLine(i));
                        manifoldEdgesCount++;
                    }
            }
            text.Add($"Mesh has {manifoldEdgesCount} manifold edges");
            reportData.Add(new GH_Integer(manifoldEdgesCount));


            // Check for naked edges - CORRECT: explode polylines into single line segments
            int nakedEdgesCount = 0;
            List<Line> nakedEdges = new List<Line>();
            if (m_nakedEdges != null)
            {
                for (int i = 0; i < m_nakedEdges.Length; i++)
                {
                    nakedEdges.AddRange(m_nakedEdges[i].GetSegments());
                }
                nakedEdgesCount = nakedEdges.Count;
                _nakedEdges.AddRange(nakedEdges);
            }
            text.Add($"Mesh has {nakedEdgesCount} naked edges");
            reportData.Add(new GH_Integer(nakedEdgesCount));


            // Check for duplicate faces
            text.Add($"Mesh has {mesh.Faces.GetDuplicateFaces().Length} duplicate faces");
            reportData.Add(new GH_Integer(mesh.Faces.GetDuplicateFaces().Length));

            // Check for faces with directions different from the mesh as a whole
            // no idea how (TO BE IMPLEMENTED)


            // Check for self-intersecting faces
            Rhino.IndexPair[] selfIntFaces = mesh.Faces.GetClashingFacePairs(0);
            text.Add($"Mesh has {selfIntFaces.Length} self-intersecting faces");
            reportData.Add(new GH_Integer(selfIntFaces.Length));


            // Check for unused vertices
            int unusedVertsCount = mesh.Vertices.CullUnused();
            text.Add($"Mesh has {unusedVertsCount} unused vertices");
            reportData.Add(new GH_Integer(unusedVertsCount));


            // Check for Disjoint pieces
            if (disjointMeshCount == 1)
                text.Add("Mesh is a single piece");
            else
                text.Add("Mesh is composed of " + mesh.DisjointMeshCount.ToString() + " disjoint piece(s)");
            reportData.Add(new GH_Integer(disjointMeshCount));


            // Check if is manifold
            bool isOriented = default(bool);
            bool hasBoundary = default(bool);
            bool isManifold = mesh.IsManifold(true, out isOriented, out hasBoundary);

            if (isManifold)
                text.Add("Mesh is manifold");
            else
                text.Add("Mesh is non-manifold");
            reportData.Add(new GH_Boolean(isManifold));


            // Check if is closed (solid)
            int solidOrientation = mesh.SolidOrientation();
            switch (solidOrientation)
            {
                case 0:
                    text.Add("Mesh is not solid");
                    break;
                case 1:
                    text.Add("Mesh is solid");
                    break;
                case -1:
                    text.Add("Mesh is solid, with normals pointing inwards");
                    break;
            }
            reportData.Add(new GH_Integer(solidOrientation));

            // Update clip BB
            foreach (Line l in _nakedEdges) _clip.Union(l.BoundingBox);
            foreach (Line l in _nonManifoldEdges) _clip.Union(l.BoundingBox);

            DA.SetDataList(0, text);
            DA.SetDataList(1, reportData);
        }

        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _nakedEdges.Clear();
            _nonManifoldEdges.Clear();
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }


        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            for (int i = 0; i < _nakedEdges.Count; i++)
                args.Display.DrawLine(_nakedEdges[i], Color.Magenta, 5);
            for (int i = 0; i < _nonManifoldEdges.Count; i++)
                args.Display.DrawLine(_nonManifoldEdges[i], Color.Blue, 5);
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
                return Resources.MeshReport_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7F9741A4-2F8A-4C1A-973D-6A5CFE0DD4E5"); }
        }
    }
}