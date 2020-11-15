using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class DivideCurveByTangentAngle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DivideCurveByTangentAngle class.
        /// </summary>
        public DivideCurveByTangentAngle()
          : base("Divide Curve By Tangent Angle", "f_DivTanAng",
              "Adaptively Divides a Curve By Tangent Angle threshold",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to Divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Increment", "i", "The increment for curve crawling - 0 < inc <  1", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "t", "Angle threshold for division", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Division Points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Tangents", "T", "Tangent vectors at division points", GH_ParamAccess.list);
            pManager.AddNumberParameter("t parameters", "t", "t parameters at division points", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve C = null;
            if (!DA.GetData(0, ref C)) return;
            double inc = 0;
            double th = 0;
            if (!DA.GetData(1, ref inc)) return;
            if (!DA.GetData(2, ref th)) return;

            if (inc <= 0)
            {
                inc = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Increment cannot be zero or negative, It has been set to the absolute tolerance");
            }


            List<Point3d> points = new List<Point3d>();
            List<Vector3d> tangents = new List<Vector3d>();
            List<double> tPars = new List<double>();

            double te = C.Domain[0];//0;
            double incRe = C.Domain.Length * inc;

            Point3d pc, pp = C.PointAt(te);
            Vector3d vc, vp = C.TangentAt(te);

            points.Add(pp);
            tangents.Add(vp);
            tPars.Add(te);

            te += incRe;
            while (te < C.Domain[1])
            {
                pc = C.PointAt(te);
                vc = C.TangentAt(te);
                if (Vector3d.VectorAngle(vc, vp) > th)
                {
                    points.Add(pc);
                    tangents.Add(vc);
                    tPars.Add(te);
                    pp = pc;
                    vp = vc;
                }
                te += incRe;
                if (te > C.Domain[1] && te < C.Domain[1] + incRe)
                {
                    points.Add(C.PointAt(C.Domain[1]));
                    tangents.Add(C.TangentAt(C.Domain[1]));
                    tPars.Add(C.Domain[1]);
                }
            }

            DA.SetDataList(0, points);
            DA.SetDataList(1, tangents);
            DA.SetDataList(2, tPars);

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
        /// 
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.Divide_Curve_by_tangent_angle_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cdaa9e75-9a3a-431e-9110-80acbc8514cc"); }
        }
    }
}