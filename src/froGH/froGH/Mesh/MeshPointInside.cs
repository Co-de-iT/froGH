using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshPointInside : GH_Component
    {
        MeshValues meshValues;

        /// <summary>
        /// Initializes a new instance of the MeshPointInside class.
        /// </summary>
        public MeshPointInside()
          : base("Mesh Point Inside", "f_MPInside",
              "Verify if a Point is inside or outside a closed Mesh\n" +
                "This component uses the Mesh Winding Number (MWN) method\n" +
                "see Jacobson et al http://igl.ethz.ch/projects/winding-number/",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "The point to check", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Inside", "I", "True if point is inside, False otherwise", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mesh Winding Number", "Mwn", "Mesh Winding Number computed for the input point", GH_ParamAccess.item);
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

            Point3d P = new Point3d();
            if (!DA.GetData(1, ref P)) return;

            M.Faces.ConvertQuadsToTriangles();

            double MeshWindingNumber = WindingNumberPrecompute(M, P);

            // see http://www.gradientspace.com/tutorials/2018/9/14/point-set-fast-winding
            DA.SetData(0, Math.Abs(MeshWindingNumber) > 0.5);
            DA.SetData(1, MeshWindingNumber);
        }

        double WindingNumberPrecompute(Mesh M, Point3d P)
        {
            double wN = 0;
            double[] wNFaces = new double[M.Faces.Count];

            Vector3d pVector = (Vector3d)P;

            meshValues = new MeshValues(M, pVector);

            Parallel.For(0, M.Faces.Count, i =>
            {
                wNFaces[i] = ComputeFace(M.Faces[i]);
            });

            for (int i = 0; i < M.Faces.Count; i++) wN += wNFaces[i];

            return wN/(4.0 * Math.PI);
        }

        public double ComputeFace(MeshFace face)
        {
            return TriSolidAngle(meshValues.Vectors[face.A], meshValues.Vectors[face.B], meshValues.Vectors[face.C],
                  meshValues.VectorLenghts[face.A], meshValues.VectorLenghts[face.B], meshValues.VectorLenghts[face.C]);
        }

        // from https://github.com/gradientspace/geometry3Sharp/blob/master/math/MathUtil.cs#L8
        /// <summary>
        /// signed winding angle of oriented triangle [a,b,c] wrt point p
        /// formula from Jacobson et al 13 http://igl.ethz.ch/projects/winding-number/
        /// </summary>
        public double TriSolidAngle(Vector3d a, Vector3d b, Vector3d c, Vector3d p)
        {
            a -= p;
            b -= p;
            c -= p;
            double la = a.Length, lb = b.Length, lc = c.Length;
            double bottom = (la * lb * lc) + a * b * lc + b * c * la + c * a * lb;
            double top = a.X * (b.Y * c.Z - c.Y * b.Z) - a.Y * (b.X * c.Z - c.X * b.Z) + a.Z * (b.X * c.Y - c.X * b.Y);
            return 2.0 * Math.Atan2(top, bottom);
        }

        public double TriSolidAngle(Vector3d a, Vector3d b, Vector3d c, double la, double lb, double lc)
        {
            double bottom = (la * lb * lc) + a * b * lc + b * c * la + c * a * lb;
            double top = a.X * (b.Y * c.Z - c.Y * b.Z) - a.Y * (b.X * c.Z - c.X * b.Z) + a.Z * (b.X * c.Y - c.X * b.Y);
            return 2.0 * Math.Atan2(top, bottom);
        }

        /// <summary>
        /// Precomputes P->Vertices vectors and their lengths
        /// </summary>
        private class MeshValues
        {
            public double[] VectorLenghts;
            public Vector3d[] Vectors;

            public MeshValues(Mesh M, Vector3d p)
            {
                VectorLenghts = new double[M.Vertices.Count];
                Vectors = new Vector3d[M.Vertices.Count];
                // Parallelize computation above 1000 vertices
                if (M.Vertices.Count > 1000)
                    Parallel.For(0, M.Vertices.Count, i =>
                    {
                        //Vectors[i] = new Vector3d(M.Vertices[i].X - p.X, M.Vertices[i].Y - p.Y, M.Vertices[i].Z - p.Z);
                        Vectors[i] = ComputeVertexVector(M.Vertices[i], p);
                        VectorLenghts[i] = Vectors[i].Length;
                    });
                else
                    for (int i = 0; i < M.Vertices.Count; i++)
                    {
                        Vectors[i] = ComputeVertexVector(M.Vertices[i], p);
                        VectorLenghts[i] = Vectors[i].Length;
                    }
            }

            public Vector3d ComputeVertexVector(Point3f v, Vector3d p)
            {
                return new Vector3d(v.X - p.X, v.Y - p.Y, v.Z - p.Z);
            }
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
            get { return new Guid("72F60794-6FD1-4EFC-83AD-3DA6EC348E33"); }
        }
    }
}