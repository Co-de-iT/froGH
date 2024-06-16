using froGH.Properties;
using Grasshopper.Kernel;
using System;

namespace froGH
{
    public class Counter : GH_Component
    {
        bool release;
        int count;
        /// <summary>
        /// Initializes a new instance of the Counter class.
        /// </summary>
        public Counter()
          : base("Counter", "f_Count",
              "A Good, Old-Fashioned Counter\nAttach a Trigger to use it",
              "froGH", "Data")
        {
            release = false;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("From", "F", "Number to start from", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("To", "T", "Number to reach", GH_ParamAccess.item, 10);
            pManager.AddBooleanParameter("Reset", "R", "Reset the counter", GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Count", "C", "The progressive count", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int start = 0;
            DA.GetData(0, ref start);

            int stop = 10;
            DA.GetData(1, ref stop); 

            bool reset = false;
            DA.GetData(2, ref reset);

            if (reset)
            {
                count = start;
                release = false;
            }
            else if (count < stop)
                if (!release) release = true;
                else count++;

            DA.SetData(0, count);
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
                return Resources.Counter_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E49FB665-07A0-4F4B-8FF8-956DC5E69120"); }
        }
    }
}