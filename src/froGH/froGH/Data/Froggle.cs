using froGH.Properties;
using Grasshopper.Kernel;
using System;

namespace froGH
{
    public class Froggle : GH_Component
    {
        // internal because it is used in the Froggle_Attributes
        internal bool result;
        /// <summary>
        /// Initializes a new instance of the Frogger class.
        /// </summary>
        public Froggle()
          : base("Froggle", "f_Fro",
              "Like a Toggle, but flipped by a Button" +
                "\nyou can also double click on the component to flip status" +
                "\nBY DESIGN, it resets to False when opening a file containing it," +
                "\nand the Froggle status flips only when a True value is input",
              "froGH", "Data")
        {
            result = false;
            Message = result.ToString();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Button", "B", "A button to start and stop the toggle", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Toggle status", "T", "The toggle status (True or False)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool T = false;
            DA.GetData(0, ref T);
            if (T) result = !result;

            Message= result.ToString();

            DA.SetData(0, result);
        }

        public override void CreateAttributes()
        {
            m_attributes = new Froggle_Attributes(this);
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
                return Resources.Froggle_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4F883D58-F651-4746-9758-6D5BE614C32F"); }
        }
    }
}