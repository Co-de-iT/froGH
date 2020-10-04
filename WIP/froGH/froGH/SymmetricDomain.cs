using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class SymmetricDomain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SymmetricDomain class.
        /// </summary>
        public SymmetricDomain()
          : base("Symmetric Domain", "f_SDom",
              "Generates a (-X, X) domain",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Domain semiextension", "X", "Half length of the Domain", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D", "Symmetric Domain", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double halfLen = 0;
            Interval cDomain;

            if (!DA.GetData(0, ref halfLen)) return;

            cDomain = new Interval(-halfLen , halfLen);

            DA.SetData(0, cDomain);
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
                return Resources.Symmetric_Domain_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1d1a23bc-ba3d-40ee-97fa-58e690745e95"); }
        }
    }
}