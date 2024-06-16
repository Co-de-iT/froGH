using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class FrenetSerretFrame : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FrenetSerretFrame class.
        /// </summary>
        public FrenetSerretFrame()
          : base("Frenet Serret Frame", "f_FSFrame",
              "Solves the Frenet-Serret Frame (Tangent, Normal and Binormal) for a Polyline",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "Input Polyline", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Tangents", "T", "Tangent Vectors", GH_ParamAccess.list);
            pManager.AddVectorParameter("Normals", "N", "Normal Vectors", GH_ParamAccess.list);
            pManager.AddVectorParameter("Binormal", "B", "Binormal Vectors", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Polyline P = null;
            Curve C = null;
            if (!DA.GetData(0, ref C)) return;
            C.TryGetPolyline(out P);
            if (P == null) return;

            List<Vector3d> tangents = new List<Vector3d>();
            List<Vector3d> normals = new List<Vector3d>();
            List<Vector3d> binormals = new List<Vector3d>();

            Vector3d avg = Vector3d.Zero;
            for (int i = 0; i < P.Capacity; i++)
                avg += (Vector3d)P[i];

            avg /= P.Capacity;

            Vector3d t, n, b, p;

            for (int i = 0; i < P.Capacity - 1; i++)
            {
                t = P[i + 1] - P[i];
                p = (Vector3d)(P[i + 1] + P[i]);
                t.Unitize();
                tangents.Add(t);

                b = Vector3d.CrossProduct(t, avg);
                b.Unitize();

                binormals.Add(b);

                n = Vector3d.CrossProduct(t, b);
                normals.Add(n);
            }

            if (!P.IsClosed)
            {
                // replicate last segment TNB data if polyline is open
                tangents.Add(tangents[P.Capacity - 2]);
                binormals.Add(binormals[P.Capacity - 2]);
                normals.Add(normals[P.Capacity - 2]);

            }

            DA.SetDataList(0, tangents);
            DA.SetDataList(1, normals);
            DA.SetDataList(2, binormals);

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
                return Resources.FrenetSerretFrame_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e642075b-7ced-48f0-ba6a-2880c0b849fb"); }
        }
    }
}