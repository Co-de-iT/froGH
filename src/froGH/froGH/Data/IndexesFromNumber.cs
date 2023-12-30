using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;

namespace froGH
{
    public class IndexesFromNumber : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IndexesFromNumber class.
        /// </summary>
        public IndexesFromNumber()
          : base("IndexesFromNumber", "f_iFromN",
              "Generates a list of 0-based integer indexes from a given amount",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Amount", "n", "the number of sequential indexes to generate", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Indexes", "i", "the generated indexes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int n = 0;
            if (!DA.GetData(0, ref n)) return;

            GH_Integer[] indexes = new GH_Integer[n];

            for (int j = 0; j < n; j++)
                indexes[j] = new GH_Integer(j);

            DA.SetDataList(0, indexes);

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
                return Resources.Indexes_from_number_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8d60dda2-e69d-4e13-8069-25f87c04bb2b"); }
        }
    }
}