using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace froGH.Utils
{
    internal class SOMNode
    {
        public int rowIndex;
        public int colIndex;
        public List<int> dataItems; // mostCommonValues
        public Vector2d[] dataDistances; // distances from node centers in 2D space
        public double[] weights;

        public SOMNode(int rowIndex, int colIndex, int nFeatures)
        {
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            dataItems = new List<int>();
            weights = new double[nFeatures];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="neighbours">Array of 4 nodes in this order: right, up, left, down</param>
        public void ComputeDistances(SOMNode[] neighbours)
        {
            dataDistances = new Vector2d[dataItems.Count];

            for(int i=0; i< dataItems.Count; i++)
            {
                // simplest case: internal node with all neighbours
                
            }
        }
    }
}
