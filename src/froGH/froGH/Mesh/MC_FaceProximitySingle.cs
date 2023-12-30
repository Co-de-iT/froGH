using System;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MC_FaceProximitySingle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MC_FaceProximitySingle class.
        /// </summary>
        public MC_FaceProximitySingle()
          : base("Mesh Connectivity - Face Proximity Single", "f_MC-FPS",
              "Mesh Topological connectivity for a single face",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Face Index", "i", "The face index", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connected vertices", "fV", "Indexes of the face's vertices", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Connected edges", "fE", "Indexes of the face's edges", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Connected faces", "fF", "Indexes of connected faces to this face", GH_ParamAccess.list);
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

            i = i % M.Faces.Count;

            int[] vertices;// = new DataTree<GH_Integer>();
            int[] edges;// = new DataTree<GH_Integer>();
            int[] faces;// = new DataTree<GH_Integer>();

            // fill faces array
            faces = M.Faces.AdjacentFaces(i);


            // fill edges array
            edges = M.TopologyEdges.GetEdgesForFace(i);


            // fill vertices array
            if (M.Faces[i].IsQuad)
            {
                vertices = new int[4];
                vertices[0] = M.Faces[i].A;
                vertices[1] = M.Faces[i].B;
                vertices[2] = M.Faces[i].C;
                vertices[3] = M.Faces[i].D;
            }
            else
            {
                vertices = new int[3];
                vertices[0] = M.Faces[i].A;
                vertices[1] = M.Faces[i].B;
                vertices[2] = M.Faces[i].C;
            }


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
                return Resources.Mesh_Face_Proximity_Single_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("96a46c02-030a-4f0c-bb33-258d6b42e704"); }
        }
    }
}