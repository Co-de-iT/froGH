using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class SortPointsByAngle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SortPointsByAngle class.
        /// </summary>
        public SortPointsByAngle()
          : base("Sort Points By Angle", "f_PtsByAng",
              "Sorts a list of points by angle from their average point",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to sort", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Reference Plane", "P", "Reference Plane for angle sorting", GH_ParamAccess.item, Plane.WorldXY);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Sorted Points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("indexes", "i", "Sorted Points indexes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Angles", "A", "Sorted Angles", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> P = new List<Point3d>();

            if (!DA.GetDataList(0, P)) return;

            Plane Pl = new Plane();
            DA.GetData(1, ref Pl);

            // calculate minimum
            Point3d avg = Average(P);

            // determine reference Z
            Vector3d X, Z;
            if (Pl.IsValid)
            {
                X = -Pl.XAxis;
                Z = Pl.ZAxis;
            }
            else
            {
                X = -Vector3d.XAxis;
                Z = Vector3d.ZAxis;
            }
            // calculate angles
            SortedDictionary<double, Point3d> sortedPoints = new SortedDictionary<double, Point3d>();

            // sort points by angles
            foreach (Point3d p in P)
                sortedPoints.Add(AngleWithSign(X, p - avg, Z), p);

            DA.SetDataList(0, sortedPoints.Values);
            DA.SetDataList(1, sortedPoints.Values.ToList().Select(p => P.IndexOf(p)));
            DA.SetDataList(2, sortedPoints.Keys.ToList().Select(x => x + Math.PI));
        }

        double AngleWithSign(Vector3d V1, Vector3d V2, Vector3d Z)
        {
            //if (Z.IsZero) Z = Vector3d.ZAxis;
            Plane p = new Plane(new Point3d(0, 0, 0), V1, Z);
            p.Rotate(-Math.PI * 0.5, V1);
            Point3d p1, p2;
            p.RemapToPlaneSpace((Point3d)V1, out p1);
            p.RemapToPlaneSpace((Point3d)V2, out p2);

            return angSign((Vector3d)p1, (Vector3d)p2);
        }

        double angSign(Vector3d v1, Vector3d v2)
        {
            v1.Unitize();
            v2.Unitize();
            return (Math.Atan2(v2.Y, v2.X) - Math.Atan2(v1.Y, v1.X));
        }

        Point3d Average(List<Point3d> pts)
        {
            Point3d avg = new Point3d();
            foreach (Point3d p in pts) avg += p;
            avg /= pts.Count;
            return avg;
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
                return Resources.SortPointsAngle_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f6f3c65b-aabf-4063-821f-c5817758588e"); }
        }
    }
}