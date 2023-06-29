using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class RotationZYX : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RotationZYX class.
        /// </summary>
        public RotationZYX()
          : base("Rotation from ZYX Euler Angles", "f_RotZYX",
              "Create a Rotation Transform from Euler Angles ZYX (aka yaw, pitch, roll)",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Z angle", "E1_Z", "Euler angle 1 (Z) - Yaw", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y angle", "E2_Y", "Euler angle 2 (Y) - Pitch", GH_ParamAccess.item);
            pManager.AddNumberParameter("X angle", "E3_X", "Euler angle 3 (X) - Roll", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTransformParameter("Transform", "T", "Rotation Transformation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double E1_Z = 0, E2_Y = 0, E3_X = 0;

            DA.GetData(0, ref E1_Z);
            DA.GetData(1, ref E2_Y);
            DA.GetData(2, ref E3_X);

            DA.SetData(0, Transform.RotationZYX(E1_Z, E2_Y, E3_X));
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
                return Resources.ZYX_2_Trans_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8AD22CCC-15ED-4C3D-AAC1-9F75C03A0994"); }
        }
    }
}