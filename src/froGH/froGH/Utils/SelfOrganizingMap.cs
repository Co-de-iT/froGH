using Grasshopper;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace froGH.Utils
{
    /// <summary>
    /// Based on this example:
    /// https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/january/test-run-self-organizing-maps-using-csharp
    /// Wikipedia: https://en.wikipedia.org/wiki/Self-organizing_map
    /// </summary>
    /// <remarks>
    /// Refactor with Structs/classes for Data & Nodes?
    /// </remarks>
    internal class SelfOrganizingMap
    {
        public double[][][] map;//[nRows][nColumns][nWeights=nFeatures]
        public double[][] dataSet;//[nData][nFeatures]
        public int[] labels;//[nData]
        public int[][] closestNode; //[nData][Row, Col]
        //public SOMNode[] closestNode;
        public double[] distanceToNodeSquared; //[nData]
        /// <summary>
        /// contains indexes of data items for each map node - 
        /// structure: [Row][Column]List of indexes
        /// </summary>
        public List<int>[][] mappedIndexes; //[nRows][nColumns]<list of indexes>
        public List<int> mostCommonValues;
        public readonly bool hasLabels;
        public HashSet<int> labelsSet;
        private readonly Random rnd;
        private readonly int Rows, Cols;
        private readonly int nData, featuresLength;
        private readonly int RangeMax;

        public SelfOrganizingMap(string fileName, bool hasLabels, int Rows, int seed)
        {
            this.hasLabels = hasLabels;
            dataSet = ReadDatasetFromFile(fileName, hasLabels, out labels);
            labelsSet = hasLabels ? CreateLabelsSet(labels) : new HashSet<int>();
            this.Rows = Rows;
            Cols = Rows;
            RangeMax = this.Rows + Cols;
            nData = dataSet.Length;
            featuresLength = dataSet[0].Length;
            mostCommonValues = new List<int>();
            rnd = new Random(seed);

            map = InitializeMap(Rows, Cols, featuresLength);
        }

        public SelfOrganizingMap(double[][] dataSet, bool hasLabels, int Rows, int seed)
        {
            this.hasLabels = hasLabels;
            this.dataSet = CreateDataSet(dataSet, hasLabels, out labels);
            labelsSet = hasLabels ? CreateLabelsSet(labels) : new HashSet<int>();
            this.Rows = Rows;
            Cols = Rows;
            RangeMax = this.Rows + Cols;
            nData = this.dataSet.Length;
            featuresLength = this.dataSet[0].Length;
            mostCommonValues = new List<int>();
            rnd = new Random(seed);

            map = InitializeMap(Rows, Cols, featuresLength);
        }

        private double[][] ReadDatasetFromFile(string filename, bool hasLabels, out int[] labels)
        {
            DataTree<double> data = new DataTree<double>();
            List<int> labelsList = new List<int>();
            FileStream ifs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            string line = "";
            string[] tokens = null;
            int row = 0;
            int nTokens;
            while ((line = sr.ReadLine()) != null)
            {
                tokens = line.Split(',');
                if (hasLabels)
                {
                    nTokens = tokens.Length - 1;
                    labelsList.Add(int.Parse(tokens[tokens.Length - 1]));
                }
                else nTokens = tokens.Length;

                for (int j = 0; j < nTokens; ++j)
                    data.Add(double.Parse(tokens[j]), new GH_Path(row));//, j));
                ++row;
            }
            sr.Close();
            ifs.Close();
            labels = hasLabels ? labelsList.ToArray() : null;
            return Utilities.ToJaggedArray(data);
        }

        private double[][] CreateDataSet(double[][] dataSetOrig, bool hasLabels, out int[] labels)
        {
            labels = new int[dataSetOrig.Length];

            if (!hasLabels) return dataSetOrig;

            double[][] dataSet = new double[dataSetOrig.Length][];

            for (int i = 0; i < dataSetOrig.Length; i++)
            {
                double[] data = dataSetOrig[i];
                dataSet[i] = new double[dataSetOrig[i].Length-1];
                for (int j = 0; j < data.Length - 1; j++)
                    dataSet[i][j] = data[j];
                labels[i] = (int)data[data.Length - 1];
            }

            return dataSet;
        }

        private HashSet<int> CreateLabelsSet(int[] labels)
        {
            HashSet<int> labelsSet = new HashSet<int>();
            foreach (int label in labels)
                labelsSet.Add(label);

            return labelsSet;
        }

        private double[][][] InitializeMap(int Rows, int Cols, int featuresLength)
        {
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

            return map;
        }

        public void Train(int StepsMax, double LearnRateMax)
        {
            double percentLeft, currLearnRate;
            int currentRange, t;
            int[] bmuRowColumn;
            // Train (construct) the SOM
            for (int s = 0; s < StepsMax; ++s)  // main loop
            {
                percentLeft = 1.0 - ((s * 1.0) / StepsMax);
                // currentRange progressively limits the node range for data shifting
                // as the iterations proceed:
                // the further in the iterations (as s increases),
                // the lower the range and the learning rate
                currentRange = (int)(percentLeft * RangeMax);
                currLearnRate = percentLeft * LearnRateMax;

                // Pick random data index
                t = rnd.Next(0, nData);
                // Get (row,col) of closest map node -- 'bmu' (Best Matching Unit)
                bmuRowColumn = ClosestNode(dataSet, t, map);
                // Update weights of each map node WITHIN THE CURRENT RANGE of the bmu
                for (int i = 0; i < Rows; ++i)
                {
                    for (int j = 0; j < Cols; ++j)
                    {
                        // if the current node's Manhattan distance to the bmu is lower than the current range then recompute its weights
                        if (ManhattanDistance(bmuRowColumn[0], bmuRowColumn[1], i, j) <= currentRange)
                            for (int k = 0; k < featuresLength; ++k)
                                // new weight = old weight  + leraningRate  * (data weight   - old weight)
                                map[i][j][k] = map[i][j][k] + currLearnRate * (dataSet[t][k] - map[i][j][k]);
                    } // j
                } // i
            } // s(tep)
        }

        public void BuildMapping()
        {
            // Create empty structure for mapNode-dataIndex assignment
            // A list of indexes for each map node
            // mapping structure: [Rows][Columns]List of indexes of data for this node
            mappedIndexes = new List<int>[Rows][];
            for (int i = 0; i < Rows; ++i)
                mappedIndexes[i] = new List<int>[Cols];
            for (int i = 0; i < Rows; ++i)
                for (int j = 0; j < Cols; ++j)
                    mappedIndexes[i][j] = new List<int>();

            // assign data items to map
            // assign to each map node the indexes t of the closest datum D(t)
            for (int t = 0; t < nData; ++t)  // each data item
            {
                // Find node map coords where node is closest to D(t)
                int[] rowColumn = ClosestNode(dataSet, t, map); // row-column index
                int row = rowColumn[0];
                int column = rowColumn[1];
                mappedIndexes[row][column].Add(t);
            }
        }

        public void FindMostCommonValues()
        {
            // find most common value label for each cell
            mostCommonValues = new List<int>();
            for (int i = 0; i < Rows; ++i)
            {
                for (int j = 0; j < Cols; ++j)
                {
                    List<int> members = new List<int>();  // '0'- '9'
                    foreach (int idx in mappedIndexes[i][j])
                        members.Add(labels[idx]);
                    int mcv = MostCommonValue(members);
                    mostCommonValues.Add(mcv);
                }
            }
        }

        public int ManhattanDistance(int x1, int y1, int x2, int y2)
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

        private double EuclideanDistance(double[] v1, double[] v2)
        {
            return Math.Sqrt(EuclideanDistanceSquared(v1, v2));
        }

        public int[] ClosestNode(double[][] data, int t, double[][][] map)
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
        private int MostCommonValue(List<int> list)
        {
            if (list.Count == 0) return -1;
            int largestCount = 0; int mostCommon = 0;
            int[] counts = new int[labelsSet.Count]; // 10 replace this "magic number" with the n. of different labels
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

    }
}
