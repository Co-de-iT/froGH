using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class AdaptiveFillet : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AdaptiveFillet class.
        /// </summary>
        public AdaptiveFillet()
          : base("Adaptive Fillet", "f_AdFlt",
              "Fillets a polyline proportionally to the segment length and angle",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "The Polyline to Fillet", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fillet Ratio", "r", "The the segment ratio (0-1) to use for the fillet", GH_ParamAccess.item, 0.5);
            pManager.AddBooleanParameter("Angle Dependent", "A", "The the segment % to use for the fillet", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Filleted Polyline", "P", "The Filleted Polyline", GH_ParamAccess.list);
            pManager.AddArcParameter("Arcs", "A", "All Arcs", GH_ParamAccess.list);
            pManager.AddLineParameter("Lines", "L", "All Lines", GH_ParamAccess.list);
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
            if (P.Length < 3) return;
            double fr = 0;
            DA.GetData(1, ref fr);
            bool angle = false;
            DA.GetData(2, ref angle);
            Line[] segs = P.GetSegments();

            Arc a;
            Line l;
            List<Arc> arcs = new List<Arc>();
            List<Line> lines = new List<Line>();
            List<NurbsCurve> filletCurve = new List<NurbsCurve>();
            double ang, angDep;
            int count, nextI;
            Vector3d dir;

            count = P.IsClosed ? segs.Length : segs.Length - 1;

            // make arcs
            for (int i = 0; i < count; i++)
            {
                nextI = (i + 1) % (segs.Length);
                dir = segs[i].Direction;
                dir.Reverse();
                ang = Vector3d.VectorAngle(dir, segs[nextI].Direction);
                if (angle) angDep = 0.01 + ang / Math.PI; else angDep = 1;
                a = Curve.CreateFillet((Curve)segs[i].ToNurbsCurve(), (Curve)segs[nextI].ToNurbsCurve(),
                  (segs[i].Length + segs[nextI].Length) * 0.5 * fr * angDep, 0.9, 0.1);
                arcs.Add(a);
            }

            // make lines
            if (!P.IsClosed)
            {
                lines.Add(new Line(P[0], arcs[0].StartPoint));
                filletCurve.Add(lines[0].ToNurbsCurve());
            }
            for (int i = 0; i < arcs.Count - 1; i++)
            {
                filletCurve.Add(arcs[i].ToNurbsCurve());
                l = new Line(arcs[i].EndPoint, arcs[i + 1].StartPoint);
                lines.Add(l);
                filletCurve.Add(l.ToNurbsCurve());
            }
            if (P.IsClosed)
            {
                l = new Line(arcs[arcs.Count - 1].EndPoint, arcs[0].StartPoint);
            }
            else
            {
                l = new Line(arcs[arcs.Count - 1].EndPoint, P[P.Capacity - 1]);
            }
            lines.Add(l);

            filletCurve.Add(arcs[arcs.Count - 1].ToNurbsCurve());
            filletCurve.Add(l.ToNurbsCurve());

            Curve[] fCurves = NurbsCurve.JoinCurves(filletCurve);

            DA.SetDataList(0, fCurves);
            DA.SetDataList(1, arcs);
            DA.SetDataList(2, lines);
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
                return Resources.Adaptive_Fillet_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ebdc73f3-ca84-46f2-9419-5c1b430accb1"); }
        }
    }
}