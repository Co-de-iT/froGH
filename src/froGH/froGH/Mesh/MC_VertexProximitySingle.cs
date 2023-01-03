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