using froGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace froGH
{
    [Obsolete("Removed to avoid duplicates across plugins - use Human's component instead")]
    public class L_ListDisplayModes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DisplayModesList class.
        /// </summary>
        public L_ListDisplayModes()
          : base("List Display Modes", "f_ListDispModes",
              "Get the list of Rhino Display Modes",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Display Modes", "D", "List of Rhino Display Modes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> displayModes = Rhino.Display.DisplayModeDescription.GetDisplayModes().Select(x => x.EnglishName).ToList();
            DA.SetDataList(0, displayModes);
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
                return Resources.List_Display_Modes_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C62D0DFA-9E8C-4429-98E3-26C8E4EBC7DE"); }
        }
    }
}