using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using System.Threading.Tasks;

namespace froGH
{
    public class MeshClosestVertexIndex : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshClosestVertexIndex class.
        /// </summary>
        public MeshClosestVertexIndex()
          : base("Mesh Closest Vertex Index", "f_MCVi",
              "Closest vertex index in a mesh (absolute and face-relative) for a given list of points",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "The Points to sample", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to sample against", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Vertex index", "Vi", "Index of closest vertices in the Mesh for each given point", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Vertex face index", "Vfi", "Relative Index (related to the mesh face) of closest vertices in the Mesh for each given point", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Face index", "Fi", "Closest Face Index for each given point", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> P = new List<Point3d>();
            if (!DA.GetDataList(0, P)) return;

            if (P.Count == 0 || P == null) return;

            Mesh M = new Mesh();
            if (!DA.GetData(1, ref M)) return;
            if (!M.IsValid) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh is invalid");

            Point3d[] pts = P.Select(p => p).ToArray();

            int[] vIndices = new int[pts.Length];
            int[] vFIndices = new int[pts.Length];
            int[] fIndices = new int[pts.Length];

            Parallel.For(0, pts.Length, i =>
            {
                MeshPoint Mp = M.ClosestMeshPoint(pts[i], 50);
                fIndices[i] = Mp.FaceIndex;
                double[] bary = Mp.T;
                double maxBar = bary[0];
                vIndices[i] = 0;

                for (int j = 1; j < bary.Length; j++)
                    if (bary[j] > maxBar)
                    {
                        maxBar = bary[j];
                        vIndices[i] = j;
                    }
                vFIndices[i] = vIndices[i];
                switch (vIndices[i])
                {
                    case 0:
                        vIndices[i] = M.Faces[Mp.FaceIndex].A;
                        break;
                    case 1:
                        vIndices[i] = M.Faces[Mp.FaceIndex].B;
                        break;
                    case 2:
                        vIndices[i] = M.Faces[Mp.FaceIndex].C;
                        break;
                    case 3:
                        vIndices[i] = M.Faces[Mp.FaceIndex].D;
                        break;
                }
            });

            DA.SetDataList(0, vIndices);
            DA.SetDataList(1, vFIndices);
            DA.SetDataList(2, fIndices);
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
                return Resources.MeshClosestVertexIndex_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("18c76626-5d2e-49a9-8df5-9b9614863a53"); }
        }
    }
}