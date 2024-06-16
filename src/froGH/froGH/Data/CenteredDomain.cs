using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class CenteredDomain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CenteredDomain class.
        /// </summary>
        public CenteredDomain()
          : base("Centered Domain", "f_CDom",
              "Generates a domain of length L, centered in 0",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Domain length", "L", "Total length of the Domain", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D", "Centered Domain of given Length", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double len = 0;
            Interval cDomain;

            if (!DA.GetData(0, ref len)) return;
            

            cDomain = new Interval(-len * 0.5, len * 0.5);

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
                return Resources.CenteredDomain_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("79771d01-d2d2-4f5c-800b-de0e16fb5eb6"); }
        }
    }
}