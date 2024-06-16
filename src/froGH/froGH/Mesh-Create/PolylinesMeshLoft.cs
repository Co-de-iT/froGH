using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class PolylinesMeshLoft : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PolylinesMeshLoft class.
        /// </summary>
        public PolylinesMeshLoft()
          : base("Polylines Mesh Loft", "f_PMLoft",
              "Lofts  polylines with the same number of vertices into a mesh",
              "froGH", "Mesh-Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Polylines to loft", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Closed", "C", "Close loft", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
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
            List<Polyline> P = new List<Polyline>();
            List<Curve> Curves = new List<Curve>();
            if (!DA.GetDataList(0, Curves)) return;
            if (Curves == null || Curves.Count == 0) return;

            for (int i = 0; i < Curves.Count; i++)
            {
                Polyline pp;
                if (Curves[i].TryGetPolyline(out pp)) P.Add(pp);
            }

            bool closed = false;

            DA.GetData(1, ref closed);

            if (P.Count < 2) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide at least 2 Polylines");

            if (P.Count <= 2 && closed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Open Loft - For a closed Loft you need to supply at least 3 Polylines");
                closed = false;
            }

            int pCount = closed ? P.Count : P.Count - 1;
            int vCount;

            Mesh mFace, mesh = new Mesh();
            Point3d[] pts0, pts1;

            for (int i = 0; i < pCount; i++)
            {
                pts0 = P[i % P.Count].ToArray();
                pts1 = P[(i + 1) % P.Count].ToArray();
                vCount = pts0.Length - 1;
                for (int j = 0; j < vCount; j++)
                {
                    mFace = new Mesh();
                    mFace.Vertices.Add(pts0[j]);
                    mFace.Vertices.Add(pts0[j + 1]);
                    mFace.Vertices.Add(pts1[j + 1]);
                    mFace.Vertices.Add(pts1[j]);
                    mFace.Faces.AddFace(0, 1, 2, 3);
                    mesh.Append(mFace);
                }
            }
            mesh.Weld(Math.PI * 0.5);
            mesh.Normals.ComputeNormals();

            DA.SetData(0, mesh);
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
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
                return Resources.PolylineMeshLoft_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6891f0b2-5ce0-416e-aab1-2ed08b6075f0"); }
        }
    }
}