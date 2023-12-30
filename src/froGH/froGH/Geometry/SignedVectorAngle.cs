using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class SignedVectorAngle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VectorAngleWIthSign class.
        /// </summary>
        public SignedVectorAngle()
          : base("Signed Vector Angle", "f_SVAng",
              "Computes angle between vectors with direction-dependent sign\nclockwise is -, counter-clockwise is +",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector 1", "V1", "First Vector", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector 2", "V2", "Second Vector", GH_ParamAccess.item);
            pManager.AddVectorParameter("Z axis", "Z", "Z axis of reference plane - default is World Z", GH_ParamAccess.item, Vector3d.ZAxis);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Signed Angle", "A", "Angle with Sign between vectors", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d V1 = Vector3d.Zero;
            Vector3d V2 = Vector3d.Zero;
            Vector3d Z = Vector3d.ZAxis;
            if (!DA.GetData(0, ref V1)) return;
            if (!DA.GetData(1, ref V2)) return;
            DA.GetData(2, ref Z);

            double A;

            // maps vector coordinates to the plane space
            if (Z.IsZero) Z = Vector3d.ZAxis;
            Plane p = new Plane(new Point3d(0, 0, 0), V1, Z);
            p.Rotate(-Math.PI * 0.5, V1);
            Point3d p1, p2;
            p.RemapToPlaneSpace((Point3d)V1, out p1);
            p.RemapToPlaneSpace((Point3d)V2, out p2);

            A = angSign((Vector3d)p1, (Vector3d)p2);

            DA.SetData(0, A);
        }

        double angSign(Vector3d v1, Vector3d v2)
        {
            v1.Unitize();
            v2.Unitize();
            return (System.Math.Atan2(v2.Y, v2.X) - System.Math.Atan2(v1.Y, v1.X));
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
                return Resources.vector_angle_sign_4_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("312eb5d4-b850-4be8-8f8f-fa62c3d93d86"); }
        }
    }
}