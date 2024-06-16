using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    [Obsolete]
    public class L_Trilateration : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Trilateration class.
        /// </summary>
        public L_Trilateration()
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
            pManager.AddPointParameter("Center Point 1", "P1", "Center Point 1", GH_ParamAccess.item);
            pManager.AddPointParameter("Center Point 2", "P2", "Center Point 2", GH_ParamAccess.item);
            pManager.AddPointParameter("Center Point 3", "P3", "Center Point 3", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius 1", "r1", "Sphere 1 radius", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius 2", "r2", "Sphere 2 radius", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius 3", "r3", "Sphere 3 radius", GH_ParamAccess.item);
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
            Point3d P1 = new Point3d();
            Point3d P2 = new Point3d();
            Point3d P3 = new Point3d();
            if (!DA.GetData(0, ref P1)) return;
            if (!DA.GetData(1, ref P2)) return;
            if (!DA.GetData(2, ref P3)) return;
            double r1 = 0;
            double r2 = 0;
            double r3 = 0;
            if (!DA.GetData(3, ref r1)) return;
            if (!DA.GetData(4, ref r2)) return;
            if (!DA.GetData(5, ref r3)) return;

            Point3d Pa, Pb;

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
            }
            else
            {
                if (tz < 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Spheres do not intersect\nPa and Pb are null");
                }
                else
                {
                    z = Math.Sqrt(tz);
                    Pa = P1 + x * e_x + y * e_y + z * e_z;
                    Pb = P1 + x * e_x + y * e_y - z * e_z;
                    DA.SetData(0, Pa);
                    DA.SetData(1, Pb);
                }
            }
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
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
                return Resources.Trilateration_3_1_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c8bc9da2-ef4c-4df6-90df-e9f2547ddab1"); }
        }
    }
}