using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class SurfaceQuadTreeByCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SurfaceQuadTreeByCurve class.
        /// </summary>
        public SurfaceQuadTreeByCurve()
          : base("Surface QuadTree By Curve", "f_SQTreeC",
              "Subdivides iteratively a surface based on a set of curves",
               "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Surface to subdivide", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "C", "Curves (on surface) for Subdivision", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Iterations", "i", "n. of recursive iterations", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Subdiviion surfaces", "S", "Surfaces obtained by subdivision", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface S = null;
            if (!DA.GetData(0, ref S)) return;
            if (S == null || !S.IsValid) return;

            List<Curve> C = new List<Curve>();
            if (!DA.GetDataList(1, C)) return;
            if (C.Count == 0) return;

            int i = 0;
            DA.GetData(2, ref i);

            List<Surface> Srf = new List<Surface>();
            subSrf(S, ref C, ref Srf, i);

            DA.SetDataList(0, Srf);
        }

        void subSrf(Surface S, ref List<Curve> C, ref List<Surface> Srf, int nGen)
        {
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            bool inters = false; // check if intersection happened
            if (nGen > 0)
            {
                //loop through curves
                foreach (Curve Cr in C)
                {
                    Rhino.Geometry.Intersect.CurveIntersections intEvent = Rhino.Geometry.Intersect.Intersection.CurveSurface(Cr, S, tol, tol);
                    if (intEvent.Count > 0)
                    {
                        inters = true;
                        break;
                    }
                }
                if (inters && nGen >= 1)
                { // if intersection happened and nGens>=1 (last loop)
                  // divide surface
                    Surface[] Ss = sTrim(ref S);
                    // this seems a tad faster but does not work on Rhino 8
                    //Surface[] Ss = sDiv(ref S);
                    // loop through surfaces
                    foreach (Surface Sdiv in Ss)
                    {
                        // call recursive subsurf
                        subSrf(Sdiv, ref C, ref Srf, nGen - 1);
                    }
                }
                else
                { // if no intersections event occurred then add surface to list
                    Srf.Add(S);
                } // if nGen =0 (last iteration) add surface to the list
            }
            else
            {
                Srf.Add(S);
            }
        }

        // quad subdivision of a surface (split by domain)
        Surface[] sDiv(ref Surface S)
        {
            Surface[] Ss = new Surface[4];
            Surface[] Sa = S.Split(0, S.Domain(0).Mid);
            for (int i = 0; i < Sa.Length; i++)
            {
                Surface[] Sb = Sa[i].Split(1, Sa[i].Domain(1).Mid);
                for (int j = 0; j < Sb.Length; j++)
                {
                    Ss[i * Sa.Length + j] = Sb[j];
                }
            }
            return Ss;
        }

        // quad subdivision of a surface (trim by domain)
        Surface[] sTrim(ref Surface S)
        {
            Surface[] Ss = new Surface[4];
            Interval u0 = new Interval(S.Domain(0).Min, S.Domain(0).Mid);
            Interval u1 = new Interval(S.Domain(0).Mid, S.Domain(0).Max);
            Interval v0 = new Interval(S.Domain(1).Min, S.Domain(1).Mid);
            Interval v1 = new Interval(S.Domain(1).Mid, S.Domain(1).Max);
            Ss[0] = S.Trim(u0, v0);
            Ss[1] = S.Trim(u0, v1);
            Ss[2] = S.Trim(u1, v0);
            Ss[3] = S.Trim(u1, v1);

            return Ss;
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
                return Resources.SurfaceQuadtreeCurve_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7b00b483-d4b2-4fd6-9a04-c30077ea1f40"); }
        }
    }
}