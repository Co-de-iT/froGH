using System;
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
    public class MC_FaceProximityMap : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_FaceProximityMap class.
        /// </summary>
        public MC_FaceProximityMap()
          : base("Mesh Connectivity - Face Proximity Map", "f_MC-FPM",
              "Mesh face-centered Topological connectivity map",
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
            pManager.AddIntegerParameter("Connected vertices", "fV", "Indexes of each face's vertices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected edges", "fE", "Indexes of each face's edges", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connected faces", "fF", "Indexes of connected faces for each face", GH_ParamAccess.tree);
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

            int[][] vertices = new int[M.Faces.Count][];
            int[][] edges = new int[M.Faces.Count][];
            int[][] faces = new int[M.Faces.Count][];

            Parallel.For(0, M.Faces.Count, i =>
            {

                // fill faces array

                faces[i] = M.Faces.AdjacentFaces(i);

                // fill edges array

                edges[i] = M.TopologyEdges.GetEdgesForFace(i);

                // fill vertices array

                if (M.Faces[i].IsQuad)
                {
                    vertices[i] = new int[4];
                    vertices[i][0] = M.Faces[i].A;
                    vertices[i][1] = M.Faces[i].B;
                    vertices[i][2] = M.Faces[i].C;
                    vertices[i][3] = M.Faces[i].D;
                }
                else
                {
                    vertices[i] = new int[3];
                    vertices[i][0] = M.Faces[i].A;
                    vertices[i][1] = M.Faces[i].B;
                    vertices[i][2] = M.Faces[i].C;
                }
            });

            DA.SetDataTree(0, ToDataTree(vertices, DA.Iteration));
            DA.SetDataTree(1, ToDataTree(edges, DA.Iteration));
            DA.SetDataTree(2, ToDataTree(faces, DA.Iteration));

        }

        public DataTree<GH_Integer> ToDataTree(int[][] array, int iter)
        {
            DataTree<GH_Integer> dt = new DataTree<GH_Integer>();
            for (int i = 0; i < array.Length; i++)
                dt.AddRange(array[i].Select(x => new GH_Integer(x)), new GH_Path(iter, i));
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
                return Resources.Mesh_Face_Proximity_Map_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("73761f6c-2fad-4acb-9cb5-b937fb66055f"); }
        }
    }
}