using System;
using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace froGH
{
    public class TopologyInverter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TopologyInverter class.
        /// </summary>
        public TopologyInverter()
          : base("Topology Inverter", "f_TInv",
              "Invert topology tree indexes\nex. in a network, from nodes sorted by lines\nto lines sorted by node and vice - versa",
              "froGH", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Topology Data", "T", "List of correspondences\n{Ti}(e0, e1, ...en)\nTi - tree branch path index - index of the current element\n (e0 ... en) - tree branch content - indexes of connected elements", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Inverse Topology Data", "Ti", "Inverse Topological Data as Tree", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> Tt;
            if (!DA.GetDataTree(0, out Tt)) return;

            GH_Structure<GH_Integer> Tinv = new GH_Structure<GH_Integer>();

            for (int i = 0; i < Tt.Branches.Count; i++)
                for (int j = 0; j < Tt.Branches[i].Count; j++)
                    Tinv.Append(new GH_Integer(Tt.Paths[i].Indices[0]), new GH_Path(Tt.Branches[i][j].Value));

            DA.SetDataTree(0, Tinv);
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
                return Resources.Topology_Inverter_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a40b8ff4-d079-45d0-8c0f-6eb31c9e8284"); }
        }
    }
}