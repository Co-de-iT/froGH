using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class DoubleShift : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Double_Shift class.
        /// </summary>
        public DoubleShift()
          : base("Double Shift", "f_DShift",
              "Performs a double shift on a list - with optional wrap",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("List", "L", "The input list for double shift", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Wrap", "W", "Wrap to list bounds", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Forward shift", "F", "List shifted Forwards", GH_ParamAccess.list);
            pManager.AddGenericParameter("Backward shift", "B", "List shifted Backwards", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_Goo> data = new List<IGH_Goo>();
            bool wrap = false;

            if (!DA.GetDataList(0, data)) return;
            DA.GetData(1, ref wrap);


            List<IGH_Goo> Fw, Bw;

            Fw = data.ConvertAll(d => d);
            Bw = data.ConvertAll(d => d);

            Fw.RemoveAt(Fw.Count - 1);
            Bw.RemoveAt(0);

            if (wrap)
            {
                IGH_Goo first, last;
                first = data[0];
                last = data[data.Count - 1];

                Fw.Insert(0, last);
                Bw.Add(first);
            }


            DA.SetDataList(0, Fw);
            DA.SetDataList(1, Bw);

        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
                return Resources.double_shift_red_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("dc000eeb-6719-4915-8a45-904d0bfa4193"); }
        }
    }
}