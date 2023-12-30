using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class Topologizer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Topologizer class.
        /// </summary>
        public Topologizer()
          : base("Topologizer", "f_TLN",
              "Solves line network topology",
              "froGH", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines network", "L", "Lines network, connected at endpoints only", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Decimals", "d", "Number of decimals for connection tolerance\n(3 is default and usually ok)", GH_ParamAccess.item, 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "List of unique point nodes", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Line-Points data", "LP", "Indexes of start and end nodes for each connection", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Point-2-Points connectivity", "PP", "Indexes of connected nodes to each unique node", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Line Direction at each Point", "LD", "Connections direction at each node\n1 departing, -1 arriving", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> L = new List<Line>();
            if (!DA.GetDataList(0, L)) return;
            if (L.Count == 0) return;
            int d=0;
            DA.GetData(1, ref d);


            Line[] L1 = L.ToArray();
            List<Point3d> pts = new List<Point3d>();
            //List<Point3d> ptsOrig = new List<Point3d>();

            for (int i = 0; i < L1.Length; i++)
            {
                //ptsOrig.Add(L1[i].From);
                //ptsOrig.Add(L1[i].To);

                L1[i].FromX = Math.Round(L1[i].FromX, d);
                L1[i].FromY = Math.Round(L1[i].FromY, d);
                L1[i].FromZ = Math.Round(L1[i].FromZ, d);
                L1[i].ToX = Math.Round(L1[i].ToX, d);
                L1[i].ToY = Math.Round(L1[i].ToY, d);
                L1[i].ToZ = Math.Round(L1[i].ToZ, d);

                pts.Add(L1[i].From);
                pts.Add(L1[i].To);
            }

            //List<int> pInd = pts.Select(pt => pts.IndexOf(pt)).ToList();

            // create the set of unique points
            // Note: try to generate with index to retrieve and output the original points (not the approximated ones)
            // using ptsOrig & pInd (now commented)
            HashSet<Point3d> points = new HashSet<Point3d>(pts); // , new Point3dComparer(doc.ModelAbsoluteTolerance));

            List<Point3d> ptList = new List<Point3d>();
            ptList.AddRange(points);


            // create the Line-Points connectivity tree
            // each branch corresponds to a line and contains
            // the indexes of the connected points
            DataTree<int> LinePts = new DataTree<int>();
            int iS, iE;
            GH_Path p;
            for (int i = 0; i < L1.Length; i++)
            {
                p = new GH_Path(i);
                iS = ptList.IndexOf(L1[i].From);
                iE = ptList.IndexOf(L1[i].To);
                LinePts.Add(iS, p);
                LinePts.Add(iE, p);
            }

            // create the Point-Points connectivity tree (neighbours)
            DataTree<int> PtPts = new DataTree<int>();
            DataTree<int> PtLDir = new DataTree<int>();
            GH_Path pS, pE;
            for (int i = 0; i < LinePts.BranchCount; i++)
            {
                // adds the index of the start point to the branch of the end point and vice-versa
                pS = new GH_Path(LinePts.Branches[i][0]);
                pE = new GH_Path(LinePts.Branches[i][1]);
                PtPts.Add(LinePts.Branches[i][0], pE);
                PtLDir.Add(-1, pE);
                PtPts.Add(LinePts.Branches[i][1], pS);
                PtLDir.Add(1, pS);
            }

            // output the set of unique points as list
            DA.SetDataList(0, ptList);
            DA.SetDataTree(1, LinePts);
            DA.SetDataTree(2, PtPts);
            DA.SetDataTree(3, PtLDir);

        }

        public class Point3dComparer : IEqualityComparer<Point3d>
        {
            double m_tol;

            public Point3dComparer(double tol)
            {
                m_tol = tol;
            }
            public bool Equals(Point3d a, Point3d b)
            {
                //return a.EpsilonEquals(b, m_tol);
                //return (a.X - b.X) < m_tol && (a.Y - b.Y) < m_tol && (a.Z - b.Z) < m_tol;
                return nEqual(a.X, b.X, m_tol) && nEqual(a.Y, b.Y, m_tol) && nEqual(a.Z, b.Z, m_tol);

            }
            public int GetHashCode(Point3d pnt)
            {
                return (pnt.X.GetHashCode() ^ pnt.Y.GetHashCode() ^ pnt.Z.GetHashCode());
            }

            // see: https://floating-point-gui.de/errors/comparison/
            public static bool nEqual(double a, double b, double epsilon)
            {
                double absA = Math.Abs(a);
                double absB = Math.Abs(b);
                double diff = Math.Abs(a - b);

                if (a == b)
                { // shortcut, handles infinities
                    return true;
                }
                else if (a == 0 || b == 0 || (absA + absB < Double.MinValue))
                {
                    // a or b is zero or both are extremely close to it
                    // relative error is less meaningful here

                    return diff < (epsilon * Double.MinValue);
                }
                else
                { // use relative error
                    return diff / Math.Min((absA + absB), Double.MaxValue) < epsilon;
                }
            }
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
                return Resources.Topologizer_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8e87e1ac-738c-4831-b7bb-769343504f0f"); }
        }
    }
}