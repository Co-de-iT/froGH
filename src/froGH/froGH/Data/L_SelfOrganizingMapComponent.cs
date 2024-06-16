using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH
{
    [Obsolete]
    /// <summary>
    /// Based on this example:
    /// https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/january/test-run-self-organizing-maps-using-csharp
    /// Wikipedia: https://en.wikipedia.org/wiki/Self-organizing_map
    /// </summary>
    public class L_SelfOrganizingMapComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SelfOrganizingMap class.
        /// </summary>
        public L_SelfOrganizingMapComponent()
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
            pManager.AddIntegerParameter("Data Labels", "L", "List of Data Labels", GH_ParamAccess.list);
            pManager.AddVectorParameter("Cell size vector", "C", "Vector with cell size in X and Y", GH_ParamAccess.item, new Vector3d(1, 1, 0));
            pManager.AddIntegerParameter("n. of Rows", "R", "Number of Rows for the Grid", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("Learning Rate", "Lr", "Learning Rate for the Self-Organizing part", GH_ParamAccess.item, 0.5);
            pManager.AddIntegerParameter("Max Steps", "s", "Max Steps for Self-Organization", GH_ParamAccess.item, 100000);
            pManager.AddBooleanParameter("Start Training", "S", "Use a toggle to start training", GH_ParamAccess.item, false);

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
            //pManager.AddNumberParameter("Map", "M", "Map", GH_ParamAccess.tree);
            //pManager.AddIntegerParameter("Region Indexes", "Ri", "Sorted Region indexes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Random rnd = new Random(0);
            // inputs
            // This input takes VERY long to compute
            // switch to File name input to be read?
            GH_Structure<GH_Number> values;
            if (!DA.GetDataTree(0, out values)) return;
            double[][] dataSet = ToJaggedArray(values);

            // input filename and has labels, then use something like this
            //dataSet = ReadDatasetFromFile(filename, hasLabels, out labels);

            int featuresLength = dataSet[0].Length;
            int nData = dataSet.Length;

            List<int> labelsList = new List<int>();
            if (!DA.GetDataList("Data Labels", labelsList)) return;
            int[] labels = labelsList.ToArray();

            Vector3d cellSize = new Vector3d();
            DA.GetData("Cell size vector", ref cellSize);

            int Rows = 5, Cols;
            DA.GetData("n. of Rows", ref Rows);
            Cols = Rows;
            int RangeMax = Rows + Cols;

            double LearnRateMax = 0.5;
            DA.GetData("Learning Rate", ref LearnRateMax);

            int StepsMax = 100000;
            DA.GetData("Max Steps", ref StepsMax);

            // build map
            // Initialize SOM nodes to random values
            double[][][] map = new double[Rows][][];  // [r][c][vec]
            for (int i = 0; i < Rows; ++i)
            {
                map[i] = new double[Cols][];
                for (int j = 0; j < Cols; ++j)
                {
                    map[i][j] = new double[featuresLength];
                    for (int k = 0; k < featuresLength; ++k)
                        map[i][j][k] = rnd.NextDouble();
                }
            }

            bool start = false;
            DA.GetData("Start Training", ref start);
            if (!start) return;

            // Train (construct) the SOM
            for (int s = 0; s < StepsMax; ++s)  // main loop
            {
                double pctLeft = 1.0 - ((s * 1.0) / StepsMax);
                // currentRange progressively limits the node range for data shifting
                // as the iterations proceed:
                // the further in the iterations (as s increases), the lower the range
                int currentRange = (int)(pctLeft * RangeMax);
                double currLearnRate = pctLeft * LearnRateMax;

                // Pick random data index
                int t = rnd.Next(0, nData);
                // Get (row,col) of closest map node -- 'bmu' (Best Matching Unit)
                int[] bmuRowColumn = ClosestNode(dataSet, t, map);
                // Update weights of each map node WITHIN THE CURRENT RANGE of the bmu
                for (int i = 0; i < Rows; ++i)
                {
                    for (int j = 0; j < Cols; ++j)
                    {
                        // if the current node Manhattan distance to the bmu is lower than the current range then recompute its weights
                        if (ManhattanDistance(bmuRowColumn[0], bmuRowColumn[1], i, j) <= currentRange)
                            for (int k = 0; k < featuresLength; ++k)
                                // new weight = old weight  + leraningRate  * (data weight   - old weight)
                                map[i][j][k] = map[i][j][k] + currLearnRate * (dataSet[t][k] - map[i][j][k]);
                    } // j
                } // i
            } // s(tep)

            // Map has been trained
            // Create empty structure for mapNode-dataIndex assignment
            // A list of indexes for each map node
            // mapping structure: [Rows][Columns]List of indexes of data for this node
            List<int>[][] mapping = new List<int>[Rows][];
            for (int i = 0; i < Rows; ++i)
                mapping[i] = new List<int>[Cols];
            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Cols; ++j)
                    mapping[i][j] = new List<int>();

            // assign data items to map
            // assign to each map node the indexes t of the closest datum D(t)
            for (int t = 0; t < nData; ++t)  // each data item
            {
                // Find node map coords where node is closest to D(t)
                int[] rowColumn = ClosestNode(dataSet, t, map); // row-column index
                int row = rowColumn[0];
                int column = rowColumn[1];
                mapping[row][column].Add(t);
            }

            // find most common value label for each cell
            List<int> mostComVal = new List<int>();
            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Cols; ++j)
                {
                    List<int> members = new List<int>();  // '0'- '9'
                    foreach (int idx in mapping[i][j])
                        members.Add(labels[idx]);
                    int mcv = MostCommonVal(members);
                    mostComVal.Add(mcv);
                }
            }

            // output data
            //DataTree<double> mapTree = mapTree = Utilities.ToDataTree(map);
            DataTree<int> mappingTree = ToDataTree(mapping);

            DA.SetDataTree(0, mappingTree);
            DA.SetDataList(1, mostComVal);
            //DA.SetDataTree(2, mapTree);
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

        private DataTree<T> ToDataTree<T>(List<T>[][] values)
        {
            DataTree<T> valuesTree = new DataTree<T>();
            for (int i = 0; i < values.Length; i++)
                for (int j = 0; j < values[i].Length; j++)
                    valuesTree.AddRange(values[i][j], new GH_Path(i, j));

            return valuesTree;
        }

        private int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        private double EuclideanDistanceSquared(double[] v1, double[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; ++i)
                sum += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            return sum;
        }

        private int[] ClosestNode(double[][] data, int t, double[][][] map)
        {
            // Coords in map of node closest to data[t]
            double smallDist = double.MaxValue;
            int[] result = new int[] { 0, 0 };  // (row, col)
            for (int i = 0; i < map.Length; ++i)
            {
                for (int j = 0; j < map[0].Length; ++j)
                {
                    double dist = EuclideanDistanceSquared(data[t], map[i][j]);
                    if (dist < smallDist)
                    {
                        smallDist = dist;
                        result[0] = i;
                        result[1] = j;
                    }
                }
            }
            return result;
        }
        private int MostCommonVal(List<int> list)
        {
            if (list.Count == 0) return -1;
            int largestCount = 0; int mostCommon = 0;
            int[] counts = new int[10]; // replace this "magic number" with the n. of different labels
            foreach (int val in list)
            {
                ++counts[val];
                if (counts[val] > largestCount)
                {
                    largestCount = counts[val];
                    mostCommon = val;
                }
            }
            return mostCommon;
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B18D55CB-E928-4983-AFE8-C0AB5B2FC63D"); }
        }
    }
}