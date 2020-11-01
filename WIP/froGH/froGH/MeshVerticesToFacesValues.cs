using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshVerticesToFacesValues : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshVerticesToFacesValues class.
        /// </summary>
        public MeshVerticesToFacesValues()
          : base("Mesh Vertices To Faces Values", "f_MV2FVal",
              "Converts per-vertex into per-face values",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Input Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Values by Vertex", "vV", "List of values by Vertex (one value per Vertex)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Use Weighted method", "W", "Uses Weighted average if True", GH_ParamAccess.item, false);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Values by Face", "vF", "List of values by Face (one value per Face)", GH_ParamAccess.list);
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

            List<double> Vv = new List<double>();
            if (!DA.GetDataList(1, Vv)) return;

            if (Vv.Count != M.Vertices.Count) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of Values should match number of Vertices in the Mesh");

            bool m = false;
            DA.GetData(2, ref m);

            int pThres = 1000; // parallelization threshold

            List<double> Vf = new List<double>(); // per-Vertices values

            if (Vv.Count < pThres)
            { // non-parallel calculation

                List<double> faceVal = new List<double>();

                if (m)
                { //weighted average
                    for (int i = 0; i < M.Faces.Count; i++)
                    {
                        faceVal.Add(v2FaceWVal(M, Vv, i));
                    }

                }
                else
                { // simple average

                    for (int i = 0; i < M.Faces.Count; i++)
                    {
                        faceVal.Add(v2FaceVal(M, Vv, i));

                    }
                }
                Vf = faceVal;


            }
            else
            { // parallel calculation

                var mVv = new System.Collections.Concurrent.ConcurrentDictionary<int, double>(Environment.ProcessorCount, Vv.Count);
                List<int> fInd = new List<int>();
                for (int i = 0; i < M.Faces.Count; i++)
                {
                    fInd.Add(i);
                }
                foreach (int i in fInd) mVv[i] = 0; // initialise dictionary

                if (m)
                {

                    System.Threading.Tasks.Parallel.ForEach(fInd, i =>
                    {
                        mVv[i] = v2FaceWVal(M, Vv, i); //save your output here
                    }
                      );

                }
                else
                {

                    System.Threading.Tasks.Parallel.ForEach(fInd, i =>
                    {
                        mVv[i] = v2FaceVal(M, Vv, i); //save your output here
                    }
                      );
                }

                Vf = mVv.Values.ToList();

            }

            DA.SetDataList(0, Vf);
        }

        public double v2FaceVal(Mesh M, List<double> Vv, int i)
        {
            double fV = 0;
            int count = 0;
            MeshFace f = M.Faces.GetFace(i);
            if (f.IsValid(M.Vertices.Count))
            {
                fV += Vv[f.A];
                fV += Vv[f.B];
                fV += Vv[f.C];
                count = 3;
                if (f.IsQuad)
                {
                    fV += Vv[f.D];
                    count = 4;
                }
                fV /= count;
            }
            return fV;
        }

        public double v2FaceWVal(Mesh M, List<double> Vv, int i)
        {
            double fV = 0, fDist = 0, dists = 0;

            MeshFace f = M.Faces.GetFace(i);
            if (f.IsValid(M.Vertices.Count))
            {
                Point3d fc = getFaceCenter(M, i);
                fDist = fc.DistanceTo(M.Vertices[f.A]);
                dists += fDist;
                fV += Vv[f.A] * fDist;
                fDist = fc.DistanceTo(M.Vertices[f.B]);
                dists += fDist;
                fV += Vv[f.B] * fDist;
                fDist = fc.DistanceTo(M.Vertices[f.C]);
                dists += fDist;
                fV += Vv[f.C] * fDist;
                if (f.IsQuad)
                {
                    fDist = fc.DistanceTo(M.Vertices[f.D]);
                    dists += fDist;
                    fV += Vv[f.D] * fDist;
                }
                fV /= dists;
            }
            return fV;
        }

        public Point3d getFaceCenter(Mesh M, int fInd)
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
        /// Exposure override for position in the SUbcategory (options primary to septenary)
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
                return Resources.vals_v2f_3_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("36e188c5-7212-4759-b762-3dd15a627b48"); }
        }
    }
}