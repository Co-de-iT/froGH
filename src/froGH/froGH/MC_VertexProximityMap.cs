using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    public class MC_VertexProximityMap : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_VertexProximityMap class.
        /// </summary>
        public MC_VertexProximityMap()
          : base("Mesh Connectivity - Vertex Proximity Map", "f_MC-VPM",
              "Mesh vertex-centered Topological connectivity map",
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
            pManager.AddIntegerParameter("Connected vertices", "vV", "Indexes of connected vertices to each vertex", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected edges", "vE", "Indexes of connected edges to each vertex", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected faces", "vF", "Indexes of faces shared by each vertex", GH_ParamAccess.tree);
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

            int[][] vertices = new int[M.Vertices.Count][];
            int[][] edges = new int[M.Vertices.Count][];
            int[][] faces = new int[M.Vertices.Count][];

            Parallel.For(0, M.Vertices.Count, i =>
            {

                // fill faces array
                faces[i] = M.Vertices.GetVertexFaces(i);

                // fill edges array
                edges[i] = M.TopologyVertices.ConnectedEdges(M.TopologyVertices.TopologyVertexIndex(i));


                // fill vertices array
                vertices[i] = M.Vertices.GetConnectedVertices(i);

            });

            DA.SetDataTree(0, ToDataTree(vertices));
            DA.SetDataTree(1, ToDataTree(edges));
            DA.SetDataTree(2, ToDataTree(faces));
        }

        public DataTree<GH_Integer> ToDataTree(int[][] array)
        {
            DataTree<GH_Integer> dt = new DataTree<GH_Integer>();
            for (int i = 0; i < array.Length; i++)
                dt.AddRange(array[i].Select(x => new GH_Integer(x)), new GH_Path(i));
            return dt;
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
                return Resources.Mesh_Vertex_Proximity_Map_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("77fba14a-b0fb-468e-ae6f-1deeb71d2de6"); }
        }
    }
}