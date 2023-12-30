using System;
using System.Threading.Tasks;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshPseudoCurvature : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshPseudoCurvature class.
        /// </summary>
        public MeshPseudoCurvature()
          : base("Mesh Pseudo-Curvature", "f_MPC",
              "A quick and dirty method to evaluate something like curvature on triangulate meshes\nThis one seems to have a penchant for peaks and valleys",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Triangle Mesh for Pseudo-Curvature evaluation", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Pseudo Curvature", "pC", "Pseudo-Curvature values, per vertex", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            if (!DA.GetData(0, ref M)) return;
            if (!(M.IsValid) || M == null) return;

            // error if mesh is not triangular
            if (M.Faces.TriangleCount != M.Faces.Count)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh must have triangular faces only");

            double[] avgC = new double[M.Vertices.Count];

            double[] faceAreas = new double[M.Faces.Count];
            Point3d[] faceCenters = new Point3d[M.Faces.Count];

            M.RebuildNormals();
            // recompute normals if necessary
            //if (M.Normals.Count != M.Vertices.Count) M.Normals.ComputeNormals();

            // pre-calculate face areas and centers
            var chunks = System.Collections.Concurrent.Partitioner.Create(0, M.Faces.Count);
            Parallel.ForEach(chunks, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double fA = FaceArea(M, i);
                    faceAreas[i] = fA;
                    faceCenters[i] = M.Faces.GetFaceCenter(i);
                }
            });

            chunks = System.Collections.Concurrent.Partitioner.Create(0, M.Vertices.Count);

            Parallel.ForEach(chunks, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    // p - current vertex
                    Point3d p = M.Vertices[i];
                    // get connected faces
                    int[] cF = M.Vertices.GetVertexFaces(i);
                    double areaSum = 0, wAvgCurv = 0, slope;

                    for (int j = 0; j < cF.Length; j++)
                    {
                        slope = Vector3d.VectorAngle(faceCenters[cF[j]] - p, M.Normals[i]); // angle between vertex-face center and vertex normal
                        areaSum += faceAreas[cF[j]];
                        wAvgCurv += slope * faceAreas[cF[j]];
                    }

                    wAvgCurv /= areaSum;
                    avgC[i] = wAvgCurv;
                }
            });

            DA.SetDataList(0, avgC);
        }

        public double FaceArea(Mesh M, int fI)
        {
            double area = -1;

            if (M.Faces[fI].IsValid() && M.Faces[fI].IsTriangle)
            {
                Vector3d ab, ac;
                double ba, he, ang;
                Point3d a, b, c;
                a = (Point3d)M.Vertices[M.Faces[fI].A];
                b = (Point3d)M.Vertices[M.Faces[fI].B];
                c = (Point3d)M.Vertices[M.Faces[fI].C];

                ab = b - a;
                ac = c - a;
                ang = Vector3d.VectorAngle(ab, ac);
                ba = ab.Length;
                he = ac.Length * Math.Sin(ang);
                area = ba * he * 0.5;

            }
            return area;
        }

        public double FaceAreaHeron(Mesh M, int fI)
        {
            double area = -1;

            if (M.Faces[fI].IsValid() && M.Faces[fI].IsTriangle)
            {
                double ab, bc, ac, pp;
                Point3d a, b, c;
                a = (Point3d)M.Vertices[M.Faces[fI].A];
                b = (Point3d)M.Vertices[M.Faces[fI].B];
                c = (Point3d)M.Vertices[M.Faces[fI].C];

                ab = a.DistanceTo(b);
                ac = a.DistanceTo(c);
                bc = b.DistanceTo(c);
                // area with Heron's formula
                pp = (ab + ac + bc) * 0.5;
                area = Math.Sqrt(pp * (pp - ab) * (pp - bc) * (pp - ac));
            }
            return area;
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
                return Resources.Mesh_curvature_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ad601a6f-4953-476c-a052-ebce10834e5d"); }
        }
    }
}