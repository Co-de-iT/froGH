using Rhino.Collections;

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
