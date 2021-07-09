using Rhino.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace froGH.Utils
{
    public class ClosestPointSearchData
    {
		public Point3dList Points
		{
			get;
			set;
		}

		public double Distance
		{
			get;
			set;
		}

		public int Index
		{
			get;
			set;
		}

		public ClosestPointSearchData(Point3dList points)
		{
			Points = points;
			Index = -1;
		}
	}
}
