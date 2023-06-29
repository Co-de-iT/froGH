using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    [Obsolete]
    public class L_MeshPointOrientation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshPointOrientation class.
        /// </summary>
        public L_MeshPointOrientation()
          : base("Mesh Point Orientation", "f_MPOrient",
              "Tells on which side of an orientable mesh a given point is\nin case of a closed mesh, True corresponds to the point\nbeing inside of the mesh",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "The points to check for orientation", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Orientation", "O", "True if point is on the side of the backfaces, False otherwise", GH_ParamAccess.list);
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

            List<Point3d> P = new List<Point3d>();
            if (!DA.GetDataList(1, P)) return;
            if (P.Count == 0 || P == null) return;

            M.RebuildNormals();
            MeshPoint mP;
            Vector3d[] faceNormals = ComputeFaceNormals(M);
            bool[] inside = new bool[P.Count];
            for (int i = 0; i < P.Count; i++)
            {
                mP = M.ClosestMeshPoint(P[i], 1000);
                inside[i] = Vector3d.VectorAngle(faceNormals[mP.FaceIndex], P[i] - mP.Point) > 1; // Huh... this should be  >= (Math.Pi * 0.5)
            }

            DA.SetDataList(0, inside);
        }

        Vector3d[] ComputeFaceNormals(Mesh M)
        {
            Vector3d[] fN = new Vector3d[M.Faces.Count];

            // parallelize above 50000 faces
            if (M.Faces.Count > 50000)
            {
                Parallel.For(0, M.Faces.Count, i =>
                {
                    FaceNormal(M, i);
                });
            }
            else
            {
                //Vector3d vFn;
                //int count;
                for (int i = 0; i < M.Faces.Count; i++)
                {
                    FaceNormal(M, i);
                }
            }

            return fN;
        }

        private Vector3d FaceNormal(Mesh M, int i)
        {
            Vector3d vFn = Vector3d.Zero;
            int count = 3;

            vFn += M.Normals[M.Faces[i].A];
            vFn += M.Normals[M.Faces[i].B];
            vFn += M.Normals[M.Faces[i].C];
            if (M.Faces[i].IsQuad)
            {
                vFn += M.Normals[M.Faces[i].D];
                count++;
            }

            vFn /= count;

            return vFn;
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
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
                return Resources.Mesh_side_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("535cc46f-a09e-4eb3-b1aa-32278698d0b8"); }
        }
    }
}