using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshFacesToVerticesValues : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshFacesToVerticesValues class.
        /// </summary>
        public MeshFacesToVerticesValues()
          : base("Mesh Faces To Vertices Values", "f_MF2VVal",
              "Converts per-face into per-vertex values",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Values by Face", "vF", "List of values by Face (one value per Face)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Use Weighted method", "W", "Uses Weighted average if True", GH_ParamAccess.item, false);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Values by Vertex", "vV", "List of values by Vertex (one value per Vertex)", GH_ParamAccess.list);
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

            List<double> Vf = new List<double>();
            if (!DA.GetDataList(1, Vf)) return;

            if (Vf.Count != M.Faces.Count) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of Values should match number of Faces in the Mesh");

            bool m = false;
            DA.GetData(2, ref m);

            List<double> vVerts = new List<double>(); // per-Vertices values
            List<double> Vv = new List<double>(); // per-Vertices values

            int pThres = 1000; // parallelization threshold

            if (Vf.Count < pThres)
            {

                int[] vFaces;
                double vVal, vD, dists;
                Point3d v;

                for (int i = 0; i < M.Vertices.Count; i++)
                {

                    vVal = 0;
                    vD = 0;
                    dists = 0;

                    // get faces shared by vertex
                    vFaces = M.Vertices.GetVertexFaces(i);

                    // get the values

                    if (m)
                    {

                        // get vertex
                        v = (Point3d)M.Vertices[i];

                        // _____________________  weighted average method

                        for (int j = 0; j < vFaces.Length; j++)
                        {

                            // get distances from face center
                            vD = v.DistanceTo(getFaceCenter(M, j));

                            // add weighted value
                            vVal += Vf[vFaces[j]] * vD;
                            // update sum of distances
                            dists += vD;
                        }

                        // weight average
                        vVal /= dists;

                        vVerts.Add(vVal);

                    }
                    else
                    {

                        // _____________________  standard average method

                        for (int j = 0; j < vFaces.Length; j++)
                        {
                            vVal += Vf[vFaces[j]];
                        }
                        // average
                        vVal /= vFaces.Length;

                        vVerts.Add(vVal);

                    }
                }

                Vv = vVerts;

            }
            else
            {

                var mFv = new System.Collections.Concurrent.ConcurrentDictionary<int, double>(Environment.ProcessorCount, Vf.Count);
                List<int> vInd = new List<int>();

                for (int i = 0; i < M.Vertices.Count; i++)
                {
                    vInd.Add(i);
                }

                foreach (int i in vInd) mFv[i] = 0; // initialise dictionary

                if (m)
                {

                    System.Threading.Tasks.Parallel.ForEach(vInd, i =>
                    {
                        // weighted average calculation

                        Point3d v = (Point3d)M.Vertices[i];
                        int[] vFaces = M.Vertices.GetVertexFaces(i);
                        double vVal = 0, dists = 0, vD = 0;

                        for (int j = 0; j < vFaces.Length; j++)
                        {
                            // get distances from face center
                            vD = v.DistanceTo(getFaceCenter(M, j));

                            // add weighted value
                            vVal += Vf[vFaces[j]] * vD;
                            // update sum of distances
                            dists += vD;
                        }
                        // average
                        vVal /= dists;
                        mFv[i] = vVal;
                    }
                      );

                    Vv = mFv.Values.ToList();

                }
                else
                {

                    System.Threading.Tasks.Parallel.ForEach(vInd, i =>
                    {
                        // standard average calculation
                        int[] vFaces = M.Vertices.GetVertexFaces(i);
                        double vVal = 0;
                        for (int j = 0; j < vFaces.Length; j++)
                        {
                            vVal += Vf[vFaces[j]];
                        }
                        // average
                        vVal /= vFaces.Length;
                        mFv[i] = vVal;
                    }
                      );

                    Vv = mFv.Values.ToList();
                }
            }

            DA.SetDataList(0, Vv);
        }

        private Point3d getFaceCenter(Mesh M, int fInd)
        {
            MeshFace f = M.Faces[fInd];
            Point3d c = new Point3d();
            if (f.IsValid(M.Vertices.Count))
            {
                c = Point3d.Add(c, (Point3d)M.Vertices[f.A]);
                c = Point3d.Add(c, (Point3d)M.Vertices[f.B]);
                c = Point3d.Add(c, (Point3d)M.Vertices[f.C]);
                if (f.IsQuad)
                {
                    c = Point3d.Add(c, (Point3d)M.Vertices[f.D]);
                }
            }
            c /= f.IsQuad ? 4 : 3;
            return c;
        }


        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quarternary; }
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
                return Resources.MeshvalFace2Verts_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("13490a68-3039-4412-b7d5-7a2301e75b95"); }
        }
    }
}