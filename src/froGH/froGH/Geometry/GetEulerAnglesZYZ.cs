using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;

namespace froGH
{
    public class GetEulerAnglesZYZ : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetEulerAnglesZYZ class.
        /// </summary>
        public GetEulerAnglesZYZ()
          : base("Get Euler Angles ZYZ", "f_EuZYZ",
              "the Euler angles for a rotation transformation in ZYZ sequence\n" +
                "Most common in robotics",
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
            pManager.AddNumberParameter("Z angle", "E1_Z", "Euler angle 1 (Z)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y angle", "E2_Y", "Euler angle 2 (Y)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Z angle", "E3_Z", "Euler angle 3 (Z)", GH_ParamAccess.item);
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
                Vector3d translation;
                Transform rotation;
                transform.DecomposeRigid(out translation, out rotation, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                transform = rotation;
            }

            double[] ZYZAngles = new double[3];

            if (!transform.GetEulerZYZ(out ZYZAngles[0], out ZYZAngles[1], out ZYZAngles[2]))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Transformation is not a rotation");

            DA.SetData(0, ZYZAngles[0]);
            DA.SetData(1, ZYZAngles[1]);
            DA.SetData(2, ZYZAngles[2]);
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
                return Resources.Trans_2_ZYZ_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("767BC258-773C-4013-B1DF-F2F1C3E8742A"); }
        }
    }
}