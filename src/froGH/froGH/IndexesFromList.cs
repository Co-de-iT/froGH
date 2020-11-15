using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    public class IndexesFromList : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IndexesFromList class.
        /// </summary>
        public IndexesFromList()
          : base("Indexes From List", "f_iFromL",
              "Generates a list of 0-based integer indexes from a given list of data",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "The Data List to index", GH_ParamAccess.list);
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
            List<IGH_Goo> data = new List<IGH_Goo>();

            if (!DA.GetDataList(0, data)) return;

            GH_Integer[] indexes = new GH_Integer[data.Count];

            for (int j = 0; j < data.Count; j++)
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
                return Resources.Indexes_from_List_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b3692e2f-9a83-49ab-8ea2-5c9a31229b05"); }
        }
    }
}