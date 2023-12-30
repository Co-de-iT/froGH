using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace froGH
{
    public class Flapper : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Flapper class.
        /// </summary>
        public Flapper()
          : base("Flapper", "f_Flap",
              "builds flaps on a planar mesh naked edges\nSuggested use with mesh strips\nAutomatic compensation for overlaps",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tapering Angle", "A", "Flap tapering angle (0 - max tapering, 0.5 * Pi - perpendicular to edge)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Distance", "D", "Flap Offset Distance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Fold inner Edges", "Fi", "", GH_ParamAccess.tree);
            pManager.AddLineParameter("Fold outer Edges", "Fo", "", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Cut Lines", "CL", "Cut Lines for Flaps", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> M = new List<Mesh>();
            if (!DA.GetDataList(0, M)) return;

            double A = 0;
            DA.GetData(1, ref A);

            double D = 0;
            DA.GetData(2, ref D);

            double biAng = 0;
            Point3d current, previous, next;
            List<Point3d> flapPoints;
            Vector3d incoming, outgoing, zAxis, biSect, vPrev, vNext;
            int cW;

            DataTree<Polyline> Flaps = new DataTree<Polyline>();
            DataTree<GH_Line> FoldInnerEdges = new DataTree<GH_Line>();
            DataTree<GH_Line> FoldOuterEdges = new DataTree<GH_Line>();

            for (int k = 0; k < M.Count; k++)
            {

                FoldInnerEdges.AddRange(GetClothedEdges(M[k]).Select(l => new GH_Line(l)).ToArray(), new GH_Path(k));

                Polyline[] nakedEdges = M[k].GetNakedEdges();
                Polyline[] flapCurves = new Polyline[nakedEdges.Length];

                // verify that naked edges are closed polylines
                bool isClosed = true;
                foreach (Polyline nEdge in nakedEdges)
                    isClosed &= nEdge.IsClosed;

                if (!isClosed) return;

                for (int i = 0; i < nakedEdges.Length; i++)
                {
                    // add segments to outer edges Data Tree
                    FoldOuterEdges.AddRange(nakedEdges[i].GetSegments().Select(s => new GH_Line(s)).ToArray(), new GH_Path(k));

                    cW = IsClockWise(nakedEdges[i]) ? -1 : 1; // is polyline clockwise?

                    flapPoints = new List<Point3d>();

                    // remove duplicate point at start/endpoint
                    nakedEdges[i].RemoveAt(nakedEdges[i].Count - 1);
                    int nPoints = nakedEdges[i].Count;

                    int prevInd, nextInd;

                    Vector3d[,] OffsetVectors = new Vector3d[nPoints, 2];
                    double[] OffsetAngles = new double[nPoints];

                    // find bisector vectors and angles
                    for (int j = 0; j < nPoints; j++)
                    {
                        prevInd = (j - 1 + nPoints) % nPoints;
                        nextInd = (j + 1) % nPoints;

                        current = nakedEdges[i][j];
                        previous = nakedEdges[i][prevInd];
                        next = nakedEdges[i][nextInd];

                        incoming = previous - current;
                        outgoing = next - current;
                        double incomingLength = incoming.Length;
                        double outgoingLength = outgoing.Length;
                        incoming.Unitize();
                        outgoing.Unitize();
                        zAxis = Vector3d.CrossProduct(incoming, outgoing) * cW;

                        // check if this elbow is convex or concave (referring to the angle inside the polyline)
                        int convex = IsConvex(cW, current, incoming, outgoing, out biAng) ? cW : -cW;

                        // angle correction to get the exact bisector angle
                        biAng *= 0.5;
                        biSect = -(incoming + outgoing) * convex;// * cW;
                        biSect.Unitize();

                        // rotation & angle check
                        double angle = A < biAng ? A * convex * cW : biAng * convex * cW;

                        // vectors for incoming and outgoing directions from a vertex
                        vPrev = incoming;
                        vNext = outgoing;
                        double dist = D / Math.Abs(Math.Sin(angle));
                        //double distP = D / Math.Abs(Math.Sin(angle));

                        // length check
                        double maxLPrev, maxLNext;
                        maxLPrev = incomingLength * 0.5 / Math.Abs(Math.Cos(angle));
                        maxLNext = outgoingLength * 0.5 / Math.Abs(Math.Cos(angle));

                        vPrev *= Math.Min(dist, maxLPrev); //dist;
                        vNext *= Math.Min(dist, maxLNext); //dist;

                        vPrev.Rotate(-angle, zAxis);
                        vNext.Rotate(angle, zAxis);

                        OffsetVectors[j, 0] = vPrev;
                        OffsetVectors[j, 1] = vNext;
                        OffsetAngles[j] = angle;

                    }

                    double hMin, hsFactorStart, hsFactorEnd;
                    Vector3d vStart, vEnd;
                    Point3d fStart, fEnd;

                    // verify offsets and generate flap points
                    for (int j = 0; j < nPoints; j++)
                    {

                        nextInd = (j + 1) % nPoints;
                        current = nakedEdges[i][j];
                        vStart = OffsetVectors[j, 1];
                        vEnd = OffsetVectors[nextInd, 0];

                        hsFactorStart = Math.Abs(Math.Sin(OffsetAngles[j]));
                        hsFactorEnd = Math.Abs(Math.Sin(OffsetAngles[nextInd]));

                        hMin = Math.Min(vStart.Length * hsFactorStart, vEnd.Length * hsFactorEnd);
                        vStart.Unitize();
                        vEnd.Unitize();
                        vStart *= hMin / hsFactorStart;
                        vEnd *= hMin / hsFactorEnd;

                        // generate flap points and add them to the array
                        fStart = current + vStart;
                        fEnd = nakedEdges[i][nextInd] + vEnd;
                        flapPoints.Add(current);
                        flapPoints.Add(fStart);
                        flapPoints.Add(fEnd);

                    }

                    // add again first point to get closed polyline
                    flapPoints.Add(flapPoints[0]);
                    flapCurves[i] = new Polyline(flapPoints);

                }
                Flaps.AddRange(flapCurves, new GH_Path(k));

            }

            DA.SetDataTree(0, FoldInnerEdges);
            DA.SetDataTree(1, FoldOuterEdges);
            DA.SetDataTree(2, Flaps);
        }

        /// <summary>
        /// Is polyline clockwise - determines if a polyline is clockwise or not
        /// </summary>
        /// <param name="poly">the Polyline to inspect</param>
        /// <returns>true for cw, false for ccw</returns>
        /// <remarks>code from https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order \nImplemented in VBNet by Mateusz Zwierzycki</remarks>
        public bool IsClockWise(Polyline poly)
        {
            double sum = 0;
            foreach (Line s in poly.GetSegments())
                sum += (s.To.X - s.From.X) * (s.To.Y + s.From.Y);

            return sum > 0;
        }

        /// <summary>
        /// Determines if Polyline is convex
        /// </summary>
        /// <param name="cW"></param>
        /// <param name="elbow"></param>
        /// <param name="previousEdge"></param>
        /// <param name="nextEdge"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public bool IsConvex(int cW, Point3d elbow, Vector3d previousEdge, Vector3d nextEdge, out double angle)
        {
            // this assumes nextEdge and previousEdge are vectors pointing out of a vertex and to the connected ones
            Plane p = new Plane(elbow, previousEdge, nextEdge);
            Point3d x, y;
            p.RemapToPlaneSpace((Point3d)elbow + previousEdge, out x);
            p.RemapToPlaneSpace((Point3d)elbow + nextEdge, out y);

            double convex = ((Math.Atan2(nextEdge.X, nextEdge.Y) - Math.Atan2(previousEdge.X, previousEdge.Y) + Math.PI * 2) % (Math.PI * 2)) - Math.PI;

            angle = convex * cW < 0 ? 2 * Math.PI - AngSign((Vector3d)x, (Vector3d)y) : AngSign((Vector3d)x, (Vector3d)y);

            return convex < 0;
        }

        /// <summary>
        /// Angle with Sign
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        double AngSign(Vector3d v1, Vector3d v2)
        {
            v1.Unitize();
            v2.Unitize();
            return (Math.Atan2(v2.Y, v2.X) - Math.Atan2(v1.Y, v1.X));
        }

        /// <summary>
        /// Get a Mesh clothed edges
        /// </summary>
        /// <param name="M"></param>
        /// <returns></returns>
        List<Line> GetClothedEdges(Mesh M)
        {
            List<Line> cEdges = new List<Line>();
            for (int i = 0; i < M.TopologyEdges.Count; i++)
            {
                if (M.TopologyEdges.GetConnectedFaces(i).Length > 1)
                    cEdges.Add(M.TopologyEdges.EdgeLine(i));
            }
            return cEdges;
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
                return Resources.Flapper_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("fcd49d1d-9eab-49c3-a4e1-d95a81547c7d"); }
        }
    }
}