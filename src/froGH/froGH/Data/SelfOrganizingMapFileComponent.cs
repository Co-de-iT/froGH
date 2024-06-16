using froGH.Properties;
using froGH.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using System;

namespace froGH
{

    // If data has no labels, instead of the Most Common Value,
    // output the closest data to the weights for that cell
    // use the closestNode and distanceToNodeSquared fields (to be implemented)
    // of the SelfOrganizingMap class
    public class SelfOrganizingMapFileComponent : GH_Component
    {
        SelfOrganizingMap m_SelfOrganizingMap;
        /// <summary>
        /// Initializes a new instance of the SelfOrganizingMapFileComponent class.
        /// </summary>
        public SelfOrganizingMapFileComponent()
          : base("Self-Organizing Map From File", "f_SOMF",
              "Self-Organizing Map From File",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "P", "Full path to data file\n" +
                "Data must be in txt format, one line per data item, comma separated\n" +
                "integer label (optional) at the end", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Has Labels", "L", "True if data has labels (default)", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("n. of Rows", "R", "Number of Rows for the Grid", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Learning Rate", "Lr", "Learning Rate for the Self-Organizing part", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Max Steps", "St", "Max Steps for Self-Organization", GH_ParamAccess.item, 100000);
            pManager.AddIntegerParameter("Seed", "s", "Seed for the pseudorandom initializer", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Start Training", "T", "Use a toggle to start training", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Mapped Data", "D", "Mapped Feature Data", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Most Common Values", "Mcv", "Most common value label for each map node", GH_ParamAccess.list);
            pManager.AddNumberParameter("Map", "M", "Map", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string filePath = "";
            if (!DA.GetData(0, ref filePath)) return;

            bool hasLabels = false;
            DA.GetData("Has Labels", ref hasLabels);

            int Rows = 5;
            DA.GetData("n. of Rows", ref Rows);
            int Cols = Rows;
            int RangeMax = Rows + Cols;
            double LearnRateMax = 0.5;
            DA.GetData("Learning Rate", ref LearnRateMax);

            int StepsMax = 100000;
            DA.GetData("Max Steps", ref StepsMax);

            int seed = 0;
            DA.GetData("Seed", ref seed);

            m_SelfOrganizingMap = new SelfOrganizingMap(filePath, hasLabels, Rows, seed);

            bool train = false;
            DA.GetData("Start Training", ref train);

            DataTree<int> mappedIndexesTree = new DataTree<int>();
            DataTree<double> mapTree;

            if (train)
            {
                m_SelfOrganizingMap.Train(StepsMax, LearnRateMax);
                m_SelfOrganizingMap.BuildMapping();
                if (m_SelfOrganizingMap.hasLabels) m_SelfOrganizingMap.FindMostCommonValues();

                // output data
                mappedIndexesTree = Utilities.ToDataTree(m_SelfOrganizingMap.mappedIndexes);
            }

            mapTree = Utilities.ToDataTree(m_SelfOrganizingMap.map);

            DA.SetDataTree(0, mappedIndexesTree);
            DA.SetDataList(1, m_SelfOrganizingMap.mostCommonValues);
            DA.SetDataTree(2, mapTree);
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
            get { return new Guid("89D8BADE-3992-4121-A9BC-62D6F62ECC40"); }
        }
    }
}