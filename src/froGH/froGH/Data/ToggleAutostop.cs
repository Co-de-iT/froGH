using froGH.Properties;
using Grasshopper.Kernel;
using System;

namespace froGH
{
    public class ToggleAutostop : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AutoStopToggle class.
        /// </summary>
        public ToggleAutostop()
          : base("Toggle Autostop", "f_TAS",
              "Stops a Toggle (reverting its status from True to False) when a condition is met",
              "froGH", "Data")
        {
        }

        bool updated;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Toggle", "T", "The Toggle to check", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Condition", "c", "The stopping condition", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool cond = false, T = false;
            DA.GetData(0, ref T);
            DA.GetData(1, ref cond);

            if (!cond)
            {
                updated = false;
                return;
            }

            var toggle = Params.Input[0].Sources[0] as Grasshopper.Kernel.Special.GH_BooleanToggle;

            if (!updated || (updated && toggle.Value == true))
            {
                toggle.Value = false;
                toggle.ExpireSolution(true);
                updated = true;
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
                return Resources.ToggleAutostop_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1375e223-14af-441c-bf39-d7ebfdc1c984"); }
        }
    }
}