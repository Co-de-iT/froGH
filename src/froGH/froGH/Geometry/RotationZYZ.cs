using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class RotationZYZ : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RotateZYZ class.
        /// </summary>
        public RotationZYZ()
          : base("Rotation from ZYZ Euler Angles", "f_RotZYZ",
              "Create a Rotation Transform from Euler Angles ZYZ",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Z angle", "E1_Z", "Euler angle 1 (Z)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Y angle", "E2_Y", "Euler angle 2 (Y)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Z angle", "E3_Z", "Euler angle 3 (Z)", GH_ParamAccess.item);
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
            double E1_Z = 0, E2_Y = 0, E3_Z = 0;

            DA.GetData(0, ref E1_Z);
            DA.GetData(1, ref E2_Y);
            DA.GetData(2, ref E3_Z);

            DA.SetData(0, Transform.RotationZYZ(E1_Z, E2_Y, E3_Z));
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
                return Resources.RotationFromEulerAnglesZYZ_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F62C6FCA-04CA-4B49-AAAA-104121CB558A"); }
        }
    }
}