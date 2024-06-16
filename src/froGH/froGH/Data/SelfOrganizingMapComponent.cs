using froGH.Properties;
using froGH.Utils;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;

namespace froGH
{
    public class SelfOrganizingMapComponent : GH_Component
    {
        SelfOrganizingMap m_SelfOrganizingMap;
        /// <summary>
        /// Initializes a new instance of the SelfOrganizingMapComponent class.
        /// </summary>
        public SelfOrganizingMapComponent()
          : base("Self-Organizing Map", "f_SOM",
              "Self-Organizing Map",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Feature Data", "D", "Tree of Feature Data Vectors", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Has Labels", "L", "True if data has labels (default)\n" +
                "Labels are integer data at the end of each list of features", GH_ParamAccess.item, true);
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
            // inputs
            // This input takes VERY long to compute (cast inside the component?)
            GH_Structure<GH_Number> values;
            if (!DA.GetDataTree(0, out values)) return;
            double[][] dataSet = ToJaggedArray(values);

            bool hasLabels = false;
            DA.GetData("Has Labels", ref hasLabels);

            int Rows = 5;
            DA.GetData("n. of Rows", ref Rows);

            double LearnRateMax = 0.5;
            DA.GetData("Learning Rate", ref LearnRateMax);

            int StepsMax = 100000;
            DA.GetData("Max Steps", ref StepsMax);

            int seed = 0;
            DA.GetData("Seed", ref seed);

            m_SelfOrganizingMap = new SelfOrganizingMap(dataSet, hasLabels, Rows, seed);
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

        double[][] ToJaggedArray(GH_Structure<GH_Number> values)
        {
            double[][] valuesArray = new double[values.PathCount][];

            double[] valArray;
            for (int i = 0; i < values.PathCount; i++)
            {
                valArray = new double[values.Branches[i].Count];

                for (int j = 0; j < values.Branches[i].Count; j++)
                {
                    valArray[j] = values.Branches[i][j].Value;
                }
                valuesArray[i] = valArray;
            }
            return valuesArray;
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
            get { return new Guid("41C739A7-4099-4751-B34C-E7D15B81857E"); }
        }
    }
}