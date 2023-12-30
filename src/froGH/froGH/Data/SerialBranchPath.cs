using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class SerialBranchPath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SerialBranchPath class.
        /// </summary>
        public SerialBranchPath()
          : base("Serial Branch Path", "f_SBPath",
              "Retrieves the data tree path and data for a specific branch serial index",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Tree", "T", "The Data Tree to search", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Branch number", "n", "The serial index of the branch to sample", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPathParameter("Path", "P", "Branch Path", GH_ParamAccess.item);
            pManager.AddGenericParameter("Data", "D", "The Branch Content", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> T = new GH_Structure<IGH_Goo>();
            if (!DA.GetDataTree(0, out T)) return;
            int i = 0;
            DA.GetData(1, ref i);
            GH_Path P = new GH_Path();
            List<IGH_Goo> data = new List<IGH_Goo>();

            if (T.Branches.Count > 0)
            {
                int ind = i % T.Branches.Count;

                P = T.Paths[ind];
                data = T.Branches[ind];

            }

            DA.SetData(0, P);
            DA.SetDataList(1, data);
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
                return Resources.SeqTreeBranch_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a2dcb291-ee89-48a1-a222-2864e25f2821"); }
        }
    }
}