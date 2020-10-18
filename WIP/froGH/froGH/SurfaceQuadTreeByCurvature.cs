using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.MeshRefinements;

namespace froGH
{
    public class SurfaceQuadTreeByCurvature : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SurfaceQuadTreeByCurvature class.
        /// </summary>
        public SurfaceQuadTreeByCurvature()
          : base("Surface QuadTree By Curvature", "f_SQTreeK",
              "Subdivides iteratively a surface based on local curvature",
              "froGH", "Geometry")
        {
        }

        List<Surface> surfs;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Surface to subdivide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "t", "Subdivision tolerance\n(WARNING: do not set this too small - start large and diminish gradually)", GH_ParamAccess.item);
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
            double t = 0.001;
            if (!DA.GetData(1, ref t)) return;

            // clears surface list
            surfs = new List<Surface>();
            // reparametrize surface
            S.SetDomain(0, new Interval(0, 1));
            S.SetDomain(1, new Interval(0, 1));

            // calls subdivision
            subdivide(S, t);

            DA.SetDataList(0, surfs);
        }

        /// <summary>
        /// Subdivides surface according to curvature, tolerance method
        /// </summary>
        /// <param name="tsurf">the input surf</param>
        /// <param name="_tol">tolerance</param>
        /// <returns></returns>
        public void subdivide(Surface tsurf, double _tol)
        {

            Surface[] sur4 = new Surface[4];
            Vector3d[] vec = new Vector3d[1];
            Point3d p0, p1, pm0, pms = new Point3d();

            // get surface domain in u & v
            Interval u = tsurf.Domain(0);
            Interval v = tsurf.Domain(1);

            // get domain edge values
            double umin = u.Min;
            double umax = u.Max;
            double vmin = v.Min;
            double vmax = v.Max;

            // evaluate corner points
            tsurf.Evaluate(umin, vmin, 0, out p0, out vec);
            tsurf.Evaluate(umax, vmax, 0, out p1, out vec);

            // evaluate surface parametric midpoint
            tsurf.Evaluate((umax + umin) / 2, (vmax + vmin) / 2, 0, out pms, out vec);

            // get diagonal midpoint
            pm0 = (p0 + p1) / 2;

            if ((pm0.DistanceTo(pms)) < _tol)
            {
                surfs.Add(tsurf);
                return;
            }
            else
            {

                // calculate average values
                double um = (umax + umin) / 2;
                double vm = (vmax + vmin) / 2;

                // build division intervals
                Interval u0 = new Interval(umin, um);
                Interval u1 = new Interval(um, umax);
                Interval v0 = new Interval(vmin, vm);
                Interval v1 = new Interval(vm, vmax);

                // trim surface with intervals
                sur4[0] = tsurf.Trim(u0, v0);
                // if anything goes wrong, throw an exception!
                // if (sur4[0] == null)
                //  throw new Exception("found a null, pirla!!! " + u0 + " " + v0);
                sur4[1] = tsurf.Trim(u1, v0);
                sur4[2] = tsurf.Trim(u0, v1);
                sur4[3] = tsurf.Trim(u1, v1);

                subdivide(sur4[0], _tol);
                subdivide(sur4[1], _tol);
                subdivide(sur4[2], _tol);
                subdivide(sur4[3], _tol);

            }

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
                return Resources.curvature_4tree_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cd35bfe3-08c6-478c-8e4d-40b8d945fcf0"); }
        }
    }
}