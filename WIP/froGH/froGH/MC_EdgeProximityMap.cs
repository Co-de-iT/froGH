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
    public class MC_EdgeProximityMap : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_EdgeProximityMap class.
        /// </summary>
        public MC_EdgeProximityMap()
          : base("Mesh Connectivity - Edge Proximity Map", "f_MC-EPM",
              "Mesh edge-centered Topological connectivity map",
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
            pManager.AddIntegerParameter("Connected vertices", "eV", "Indexes of endpoint vertices of each edge", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected edges", "eE", "Indexes of connected edges to each edge", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected faces", "eF", "Indexes of faces shared by each edge", GH_ParamAccess.tree);
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

            int[][] vertices = new int[M.TopologyEdges.Count][];
            int[][][] edges = new int[M.TopologyEdges.Count][][];
            int[][] faces = new int[M.TopologyEdges.Count][];

            Parallel.For(0, M.TopologyEdges.Count, i =>
            //for (int i = 0; i < M.TopologyEdges.Count; i++)
            {
                int[] edgeEndPts = new int[2];

                // fill faces array
                faces[i] = M.TopologyEdges.GetConnectedFaces(i);


                // fill edges array

                // topology edge > index of vertices at endpoints
                edgeEndPts[0] = M.TopologyEdges.GetTopologyVertices(i).I;
                edgeEndPts[1] = M.TopologyEdges.GetTopologyVertices(i).J;

                edges[i] = new int[2][];

                // get connected edges at each endpoint
                for (int j = 0; j < edgeEndPts.Length; j++)
                {
                    HashSet<int> connV = new HashSet<int>();
                    int[] coV = M.TopologyVertices.ConnectedEdges(edgeEndPts[j]);
                    foreach (int v in coV)
                        connV.Add(v);
                    connV.Remove(i);
                    edges[i][j] = connV.ToArray();
                }

                // fill vertices array
                vertices[i] = new int[2];
                vertices[i][0] = M.TopologyVertices.MeshVertexIndices(edgeEndPts[0])[0];
                vertices[i][1] = M.TopologyVertices.MeshVertexIndices(edgeEndPts[1])[0];

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

        public DataTree<GH_Integer> ToDataTree(int[][][] array)
        {
            DataTree<GH_Integer> dt = new DataTree<GH_Integer>();
            for (int i = 0; i < array.Length; i++)
                for (int j = 0; j < array[i].Length; j++)
                    dt.AddRange(array[i][j].Select(x => new GH_Integer(x)), new GH_Path(i, j));
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
                return Resources.Mesh_Edge_Proximity_Map_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3d9f1af8-fd69-4879-91d4-c41472368678"); }
        }
    }
}