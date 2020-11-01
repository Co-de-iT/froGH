using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class MeshBridge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshBridge class.
        /// </summary>
        public MeshBridge()
          : base("Mesh Bridge", "f_MBridge",
              "Builds a Mesh Bridge between lists of points",
              "froGH", "Mesh-Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Ponints A", "Pa", "First set of Points", GH_ParamAccess.list);
            pManager.AddPointParameter("Ponints B", "Pb", "Second set of Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Subdivisions", "s", "Number of subdivisions", GH_ParamAccess.item, 1);

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The output Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> Pa = new List<Point3d>();
            List<Point3d> Pb = new List<Point3d>();
            int S = 0;

            if (!DA.GetDataList(0, Pa)) return;
            if (!DA.GetDataList(1, Pb)) return;
            if (Pa == null || Pb == null || Pa.Count == 0 || Pb.Count == 0 ) return;
            if (Pa.Count != Pb.Count) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pa and Pb must have the same number of Points");

            DA.GetData(2, ref S);

            Mesh mesh = new Mesh();
            Mesh mFace;// = new Mesh();

            Point3d[][] pts = new Point3d[S + 1][];
            pts[0] = Pa.ToArray();
            pts[S] = Pb.ToArray();
            // generate points
            for (int i = 1; i < S; i++)
            {
                pts[i] = new Point3d[Pa.Count];
                for (int j = 0; j < Pa.Count; j++)
                {
                    Vector3d v = Vector3d.Subtract((Vector3d)Pb[j], (Vector3d)Pa[j]);
                    v = Vector3d.Divide(v, S);
                    pts[i][j] = Point3d.Add(pts[i - 1][j], v);
                }
            }
            // make the mesh
            for (int i = 0; i < pts.Length - 1; i++)
            {

                for (int j = 0; j < pts[i].Length-1; j++)
                {
                    mFace = new Mesh();
                    mFace.Vertices.Add(pts[i][j]);
                    mFace.Vertices.Add(pts[i][j+1]);
                    mFace.Vertices.Add(pts[i + 1][j+1]);
                    mFace.Vertices.Add(pts[i + 1][j]);
                    mFace.Faces.AddFace(0, 1, 2, 3);
                    mesh.Append(mFace);
                }

            }

            if (mesh != null) mesh.Weld(0.01);

            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Exposure override for position in the SUbcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
                return Resources.Mesh_bridge_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b9ef0282-f225-466e-8afa-6f26eb0198be"); }
        }
    }
}