using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    public class WeightedRandomChoice : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WeightedRandomChoice class.
        /// </summary>
        public WeightedRandomChoice()
          : base("Weighted Random Choice", "f_WRC",
              "Generates weighted random sequences from a given list of values and weights",
              "froGH", "Data")
        {
        }

        Random rnd;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data to sort by Weighted Random algorithm", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Weights", "W", "Integer Weights (one value per data item)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of items", "n", "Number of output items", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Seed", "S", "The Seed for the Random engine", GH_ParamAccess.item, 0);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Random Weighted List", "L", "Random Weighted List from input Data", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_Goo> data = new List<IGH_Goo>();
            List<IGH_Goo> wData = new List<IGH_Goo>();

            if (!DA.GetDataList(0, data)) return;

            List<int> weights = new List<int>();
            if (!DA.GetDataList(1, weights)) return;

            if (data.Count != weights.Count)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Weights number should match number of data items");

            int n = 0, seed = 0;

            DA.GetData(2, ref n);
            DA.GetData(3, ref seed);

            wData = WRC(data, weights, n, seed);

            DA.SetDataList(0, wData);
        }

        private List<IGH_Goo> WRC(List<IGH_Goo> data, List<int> weights, int n,int seed)
        {
            rnd = new Random(seed);
            List<IGH_Goo> values = new List<IGH_Goo>();

            int totWeights = weights.Sum(w => w);

            for (int i = 0; i < n; i++)
            {

                int chosenInd = rnd.Next(totWeights);
                int valueInd = 0;

                while (chosenInd >= 0)
                {
                    chosenInd -= weights[valueInd];
                    valueInd++;
                }

                valueInd -= 1;
                values.Add(data[valueInd]);
            }

            return values;
        }

        /// <summary>
        /// Exposure override for position in the SUbcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
                return Resources.Weighted_Random_Choice_3_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cea60118-6226-406f-9c2a-1fc9793ffeb4"); }
        }
    }
}