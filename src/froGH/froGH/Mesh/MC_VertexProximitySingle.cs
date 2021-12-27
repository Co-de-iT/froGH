using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;

namespace froGH
{
    public class MC_VertexProximitySingle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_VertexProximitySingle class.
        /// </summary>
        public MC_VertexProximitySingle()
          : base("Mesh Connectivity - Vertex Proximity Single", "f_MC-VPS",
              "Mesh Topological connectivity for a single vertex",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Vertex Index", "i", "The vertex index", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connected vertices", "vV", "Indexes of connected vertices to this vertex", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Connected edges", "vE", "Indexes of connected edges to this vertex", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Connected faces", "vF", "Indexes of connected faces to this vertex", GH_ParamAccess.list);
            //pManager.AddBooleanParameter("Faces-edges directions match", "fD", "true if direction of face mateches the connected edge", GH_ParamAccess.tree);
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

            int i = 0;
            DA.GetData(1, ref i);

            // prevent index overshoots
            i = i % M.Vertices.Count;

            int[] vertices;
            int[] edges;
            int[] faces;

            Utilities.GetVertexNeighbours(M, i, out vertices, out edges, out faces);

            ////topology structure groups coincident vertices into a single "TopologyVertex"
            //// topology vertex index
            //int tvIndex = M.TopologyVertices.TopologyVertexIndex(i);

            //// find connected edges in radial order
            //// SortEdges puts edges in radial order
            //M.TopologyVertices.SortEdges(tvIndex);
            //edges = M.TopologyVertices.ConnectedEdges(tvIndex);
            //if (edges == null || edges.Length == 0) return;

            //// find neighbour vertices in the edges order
            //vertices = new int[edges.Length];
            //for (int j = 0; j < edges.Length; j++)
            //{
            //    IndexPair edgeTips = M.TopologyEdges.GetTopologyVertices(edges[j]);
            //    vertices[j] = edgeTips[0] == tvIndex ? edgeTips[1] : edgeTips[0];
            //}

            //faces = null;
            //// look for faces only if there is more than 1 edge
            //if (edges.Length > 1)
            //{
            //    // find neighbour faces in edges order
            //    // try this: pick first face index as face shared from first and second edge
            //    // for the following edges pick the index (of the two) that is different from the previous iteration
            //    faces = new int[edges.Length];

            //    // first edge/face index
            //    int[] cFaces;
            //    // find connected faces indexes to first edge
            //    cFaces = M.TopologyEdges.GetConnectedFaces(edges[0]);
            //    if (cFaces.Length != 0)
            //    {
            //        for (int j = 0; j < cFaces.Length; j++)
            //        {
            //            // find edges for j-th connected face
            //            int[] fEdges = M.TopologyEdges.GetEdgesForFace(cFaces[j]);
            //            // if face contains also index of second edge then we found our first face
            //            if (fEdges.Contains(edges[1]))
            //            {
            //                faces[0] = cFaces[j];
            //                break;
            //            }
            //        }
            //    }
            //    // all other edges
            //    for (int j = 1; j < edges.Length; j++)
            //    {
            //        cFaces = M.TopologyEdges.GetConnectedFaces(edges[j]);
            //        if (cFaces.Length != 0) faces[j] = cFaces[0] != faces[j - 1] ? cFaces[0] : cFaces[1];
            //    }
            //}

            DA.SetDataList(0, vertices);
            DA.SetDataList(1, edges);
            DA.SetDataList(2, faces);
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
                return Resources.Mesh_Vertex_Proximity_Single_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ecbed6d1-b66f-41a1-8852-d54b8e01f503"); }
        }
    }
}