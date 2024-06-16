using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class Trilateration : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Trilateration class.
        /// </summary>
        public Trilateration()
          : base("Trilateration", "f_TriLat",
              "Finds intersection of 3 spheres with algebraic method\n(faster than geometric intersection of solids)",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Sphere 1", "S1", "First Sphere", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Sphere 2", "S2", "Second Sphere", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Sphere 3", "S3", "Third Sphere", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point A", "Pa", "Point A", GH_ParamAccess.item);
            pManager.AddPointParameter("Point B", "Pb", "Point B", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface surf1 = null;
            if (!DA.GetData(0, ref surf1)) return;
            Surface surf2 = null;
            if (!DA.GetData(1, ref surf2)) return;
            Surface surf3 = null;
            if (!DA.GetData(2, ref surf3)) return;

            surf1.TryGetSphere(out Sphere S1);
            surf2.TryGetSphere(out Sphere S2);
            surf3.TryGetSphere(out Sphere S3);

            //Point3d Pa, Pb;

            //Point3d P1 = S1.Center;
            //Point3d P2 = S2.Center;
            //Point3d P3 = S3.Center;

            //double r1 = S1.Radius;
            //double r2 = S2.Radius;
            //double r3 = S3.Radius;


            //Vector3d t1, t2, t3, e_x, e_y, e_z;
            //double i, j, d, tz, x, y, z;

            //t1 = P2 - P1;
            //t2 = P3 - P1;
            //d = t1.Length;
            //e_x = t1 / d; // unitize t1 and assing it to e_x
            //i = Vector3d.Multiply(e_x, t2);
            //t3 = t2 - i * e_x;
            //e_y = t3 / t3.Length; // unitize t3 and assing it to e_y
            //e_z = Vector3d.CrossProduct(e_x, e_y);

            //j = Vector3d.Multiply(e_y, t2);
            //x = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            //y = (r1 * r1 - r3 * r3 - 2 * i * x + i * i + j * j) / (2 * j);

            //tz = (r1 * r1) - (x * x) - (y * y);
            //if (Math.Round(tz, 6) == 0)
            //{
            //    z = 0;
            //    Pa = P1 + x * e_x + y * e_y + z * e_z;
            //    Pb = Pa;
            //}
            //else
            //{
            //    if (tz < 0)
            //    {
            //        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Spheres do not intersect");
            //    }
            //    else
            //    {
            //        z = Math.Sqrt(tz);
            //        Pa = P1 + x * e_x + y * e_y + z * e_z;
            //        Pb = P1 + x * e_x + y * e_y - z * e_z;
            //        DA.SetData(0, Pa);
            //        DA.SetData(1, Pb);
            //    }
            //}

            if (!Trilaterate(S1, S2, S3, out Point3d Pa, out Point3d Pb))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Spheres do not intersect");
            else
            {
                DA.SetData(0, Pa);
                DA.SetData(1, Pb);
            }
        }

        private bool Trilaterate(Sphere S1, Sphere S2, Sphere S3, out Point3d Pa, out Point3d Pb)
        {
            Pa = Point3d.Unset;
            Pb = Point3d.Unset;

            Point3d P1 = S1.Center;
            Point3d P2 = S2.Center;
            Point3d P3 = S3.Center;

            double r1 = S1.Radius;
            double r2 = S2.Radius;
            double r3 = S3.Radius;

            Vector3d t1, t2, t3, e_x, e_y, e_z;
            double i, j, d, tz, x, y, z;

            t1 = P2 - P1;
            t2 = P3 - P1;
            d = t1.Length;
            e_x = t1 / d; // unitize t1 and assing it to e_x
            i = Vector3d.Multiply(e_x, t2);
            t3 = t2 - i * e_x;
            e_y = t3 / t3.Length; // unitize t3 and assing it to e_y
            e_z = Vector3d.CrossProduct(e_x, e_y);

            j = Vector3d.Multiply(e_y, t2);
            x = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            y = (r1 * r1 - r3 * r3 - 2 * i * x + i * i + j * j) / (2 * j);

            tz = (r1 * r1) - (x * x) - (y * y);

            if (Math.Round(tz, 6) == 0)
            {
                z = 0;
                Pa = P1 + x * e_x + y * e_y + z * e_z;
                Pb = Pa;
                return true;
            }
            else if (tz < 0) return false;
            else
            {
                z = Math.Sqrt(tz);
                Pa = P1 + x * e_x + y * e_y + z * e_z;
                Pb = P1 + x * e_x + y * e_y - z * e_z;
                return true;
            }
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
                return Resources.Trilateration_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F03D7266-CAB9-426F-A8B6-CE51EF278ECB"); }
        }
    }
}