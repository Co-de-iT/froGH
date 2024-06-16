using froGH.Properties;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class GetEulerAnglesZYX : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetEulerAnglesZYZ class.
        /// </summary>
        public GetEulerAnglesZYX()
          : base("Get Euler Angles ZYX", "f_EuZYX",
              "the Euler angles for a rotation transformation in ZYX order\n" +
                "Also known as Yaw, Pitch, Roll",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTransformParameter("Transform", "T", "Transformation to process (must be a rigid transformation)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Z angle", "E1_Z", "Euler angle 1 (Z) - Yaw", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y angle", "E2_Y", "Euler angle 2 (Y) - Pitch", GH_ParamAccess.item);
            pManager.AddNumberParameter("X angle", "E3_X", "Euler angle 3 (X) - Roll", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Transform transform = new Transform();
            DA.GetData(0, ref transform);

            if (transform == null) return;

            // if the transformation is not affine there's nothing we can do
            if (!transform.IsAffine)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Transformation is not affine, can't extract a rotation");

            // if the transformation is affine but NOT a rotation, try to get the rotation part only
            if (!transform.IsRotation)
            {
                if (transform.RigidType == 0)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Transformation is not rigid, can't extract a rotation");
                Transform rotation;
                _ = transform.DecomposeRigid(out _, out rotation, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                transform = rotation;
            }

            double[] ZYXAngles = new double[3];

            if (!transform.GetYawPitchRoll(out ZYXAngles[0], out ZYXAngles[1], out ZYXAngles[2]))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Transformation is not a rotation");

            DA.SetData(0, ZYXAngles[0]);
            DA.SetData(1, ZYXAngles[1]);
            DA.SetData(2, ZYXAngles[2]);
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
                return Resources.EulerAnglesZYX_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3312D1CC-6A99-408F-B774-5FAC0B961AFD"); }
        }
    }
}