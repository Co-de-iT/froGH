using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace froGH
{
    public class KDTreeComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the KDTreeComponent class.
        /// </summary>
        public KDTreeComponent()
          : base("KDTreeComponent", "f_KDT",
              "Create a KDTree search structure from a list of Points",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to build KDTree", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("KD Tree", "KD", "The KD Tree from input points", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> points = new List<GH_Point>();
            if (!DA.GetDataList(0, points)) return;

            List<KDNode> nodes = points.Select(p => new KDNode(p.Value)).ToList();

            KDTree kdT = new KDTree(3, nodes);

            DA.SetData(0, kdT);
        }

        // hide component in release
#if !DEBUG
        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

#endif

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.ZZ_placeholder_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ECFC775C-0FFA-4ACB-BCBC-C3F2F6CE2E04"); }
        }
    }
}