using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace froGH
{
    public class MC_EdgeProximitySingle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_EdgeProximitySingle class.
        /// </summary>
        public MC_EdgeProximitySingle()
          : base("Mesh Connectivity - Edge Proximity Single", "f_MC-EPS",
              "Mesh Topological connectivity for a single edge",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Edge Index", "i", "The edge index", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connected vertices", "eV", "Indexes of endpoint vertices for this edge", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Connected edges", "eE", "Indexes of connected edges to this edge", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected faces", "eF", "Indexes of faces shared by this edge", GH_ParamAccess.list);
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

            i = i % M.TopologyEdges.Count;

            int[] edgeEndPts;// = new DataTree<GH_Integer>();
            DataTree<int> edges = new DataTree<int>();
            int[] faces;// = new DataTree<GH_Integer>();

            // fill faces data tree
            faces = M.TopologyEdges.GetConnectedFaces(i);//.Select(x => new GH_Integer(x)), p);


            // fill vertices data tree
            edgeEndPts = new int[2];

            // topology edge > index of vertices at endpoints
            edgeEndPts[0] = M.TopologyEdges.GetTopologyVertices(i).I;
            edgeEndPts[1] = M.TopologyEdges.GetTopologyVertices(i).J;

            // fill edges data tree
            // get connected edges at each endpoint
            for (int j = 0; j < edgeEndPts.Length; j++)
            {
                //HashSet<int> connV = new HashSet<int>();
                M.TopologyVertices.SortEdges(edgeEndPts[j]);
                int[] coEd = M.TopologyVertices.ConnectedEdges(edgeEndPts[j]);
                //foreach (int v in coV)
                //    connV.Add(v);
                //connV.Remove(i);
                List<int> connEdges = new List<int>();
                List<int> prevConnEdges = new List<int>();
                bool edgeFound = false;
                for (int k = 0; k < coEd.Length; k++)
                {
                    if (coEd[k] == i)
                    {
                        edgeFound = true;
                        continue;
                    }
                    if (edgeFound)
                        connEdges.Add(coEd[k]);
                    else prevConnEdges.Add(coEd[k]);
                }
                connEdges.AddRange(prevConnEdges);

                edges.AddRange(connEdges, new GH_Path(i, j));
            }

            DA.SetDataList(0, edgeEndPts);
            DA.SetDataTree(1, edges);
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
                return Resources.EdgeProximitySingle_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d13ccde9-ddbe-44aa-8346-4397bdbd0d3a"); }
        }
    }
}