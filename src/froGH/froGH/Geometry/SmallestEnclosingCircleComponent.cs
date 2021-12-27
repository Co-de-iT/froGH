using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class SmallestEnclosingCircleComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SmallestEnclosingCircle class.
        /// </summary>
        public SmallestEnclosingCircleComponent()
          : base("Smallest Enclosing Circle", "f_SEC",
              "Finds the Smallest Enclosing Circle for the given points in the given plane\n" +
                "original code Copyright (c) 2020 Project Nayuki\nhttps://www.nayuki.io/page/smallest-enclosing-circle",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to sort", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Reference Plane", "P", "Reference Plane for the circle", GH_ParamAccess.item, Plane.WorldXY);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCircleParameter("Smallest Enclosing Circle", "C", "Smallest Enclosing Circle for the given points in the given plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> P = new List<Point3d>();

            if (!DA.GetDataList(0, P)) return;

            Plane planeInput = new Plane();
            DA.GetData(1, ref planeInput);

            DA.SetData(0, MakeSECircle(P, planeInput));
        }

        public Circle MakeSECircle(List<Point3d> points, Plane plane)
        {
            Circle circle = new Circle();
            SECircle seCircle;
            List<SEPoint> pointsForSECircle = new List<SEPoint>();

            // project points on plane and transforms them to the XY plane
            Transform projectToPlane = Transform.PlanarProjection(plane);
            Transform toXYPlane = Transform.PlaneToPlane(plane, Plane.WorldXY);
            Transform toOriginalPlane = Transform.PlaneToPlane(Plane.WorldXY, plane);

            for (int i = 0; i < points.Count; i++)
            {
                Point3d projected = new Point3d(points[i]);
                projected.Transform(projectToPlane);
                projected.Transform(toXYPlane);
                pointsForSECircle.Add(new SEPoint(projected.X, projected.Y));
            }

            seCircle = SmallestEnclosingCircle.MakeCircle(pointsForSECircle);

            circle = new Circle(new Point3d(seCircle.c.x, seCircle.c.y, 0), seCircle.r);
            circle.Transform(toOriginalPlane);
            return circle;
        }

        /* 
 * Smallest enclosing circle - Library (C#)
 * 
 * Copyright (c) 2020 Project Nayuki
 * https://www.nayuki.io/page/smallest-enclosing-circle
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program (see COPYING.txt and COPYING.LESSER.txt).
 * If not, see <http://www.gnu.org/licenses/>.
 */




        public sealed class SmallestEnclosingCircle
        {

            /* 
             * Returns the smallest circle that encloses all the given points. Runs in expected O(n) time, randomized.
             * Note: If 0 points are given, a circle of radius -1 is returned. If 1 point is given, a circle of radius 0 is returned.
             */
            // Initially: No boundary points known
            public static SECircle MakeCircle(IList<SEPoint> points)
            {
                // Clone list to preserve the caller's data, do Durstenfeld shuffle
                List<SEPoint> shuffled = new List<SEPoint>(points);
                Random rand = new Random();
                for (int i = shuffled.Count - 1; i > 0; i--)
                {
                    int j = rand.Next(i + 1);
                    SEPoint temp = shuffled[i];
                    shuffled[i] = shuffled[j];
                    shuffled[j] = temp;
                }

                // Progressively add points to circle or recompute circle
                SECircle c = SECircle.INVALID;
                for (int i = 0; i < shuffled.Count; i++)
                {
                    SEPoint p = shuffled[i];
                    if (c.r < 0 || !c.Contains(p))
                        c = MakeCircleOnePoint(shuffled.GetRange(0, i + 1), p);
                }
                return c;
            }


            // One boundary point known
            private static SECircle MakeCircleOnePoint(List<SEPoint> points, SEPoint p)
            {
                SECircle c = new SECircle(p, 0);
                for (int i = 0; i < points.Count; i++)
                {
                    SEPoint q = points[i];
                    if (!c.Contains(q))
                    {
                        if (c.r == 0)
                            c = MakeDiameter(p, q);
                        else
                            c = MakeCircleTwoPoints(points.GetRange(0, i + 1), p, q);
                    }
                }
                return c;
            }


            // Two boundary points known
            private static SECircle MakeCircleTwoPoints(List<SEPoint> points, SEPoint p, SEPoint q)
            {
                SECircle circ = MakeDiameter(p, q);
                SECircle left = SECircle.INVALID;
                SECircle right = SECircle.INVALID;

                // For each point not in the two-point circle
                SEPoint pq = q.Subtract(p);
                foreach (SEPoint r in points)
                {
                    if (circ.Contains(r))
                        continue;

                    // Form a circumcircle and classify it on left or right side
                    double cross = pq.Cross(r.Subtract(p));
                    SECircle c = MakeCircumcircle(p, q, r);
                    if (c.r < 0)
                        continue;
                    else if (cross > 0 && (left.r < 0 || pq.Cross(c.c.Subtract(p)) > pq.Cross(left.c.Subtract(p))))
                        left = c;
                    else if (cross < 0 && (right.r < 0 || pq.Cross(c.c.Subtract(p)) < pq.Cross(right.c.Subtract(p))))
                        right = c;
                }

                // Select which circle to return
                if (left.r < 0 && right.r < 0)
                    return circ;
                else if (left.r < 0)
                    return right;
                else if (right.r < 0)
                    return left;
                else
                    return left.r <= right.r ? left : right;
            }


            public static SECircle MakeDiameter(SEPoint a, SEPoint b)
            {
                SEPoint c = new SEPoint((a.x + b.x) / 2, (a.y + b.y) / 2);
                return new SECircle(c, Math.Max(c.Distance(a), c.Distance(b)));
            }


            public static SECircle MakeCircumcircle(SEPoint a, SEPoint b, SEPoint c)
            {
                // Mathematical algorithm from Wikipedia: Circumscribed circle
                double ox = (Math.Min(Math.Min(a.x, b.x), c.x) + Math.Max(Math.Max(a.x, b.x), c.x)) / 2;
                double oy = (Math.Min(Math.Min(a.y, b.y), c.y) + Math.Max(Math.Max(a.y, b.y), c.y)) / 2;
                double ax = a.x - ox, ay = a.y - oy;
                double bx = b.x - ox, by = b.y - oy;
                double cx = c.x - ox, cy = c.y - oy;
                double d = (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) * 2;
                if (d == 0)
                    return SECircle.INVALID;
                double x = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
                double y = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;
                SEPoint p = new SEPoint(ox + x, oy + y);
                double r = Math.Max(Math.Max(p.Distance(a), p.Distance(b)), p.Distance(c));
                return new SECircle(p, r);
            }

        }



        public struct SECircle
        {

            public static readonly SECircle INVALID = new SECircle(new SEPoint(0, 0), -1);

            private const double MULTIPLICATIVE_EPSILON = 1 + 1e-14;


            public SEPoint c;   // Center
            public double r;  // Radius


            public SECircle(SEPoint c, double r)
            {
                this.c = c;
                this.r = r;
            }


            public bool Contains(SEPoint p)
            {
                return c.Distance(p) <= r * MULTIPLICATIVE_EPSILON;
            }


            public bool Contains(ICollection<SEPoint> ps)
            {
                foreach (SEPoint p in ps)
                {
                    if (!Contains(p))
                        return false;
                }
                return true;
            }

        }



        public struct SEPoint
        {

            public double x;
            public double y;


            public SEPoint(double x, double y)
            {
                this.x = x;
                this.y = y;
            }


            public SEPoint Subtract(SEPoint p)
            {
                return new SEPoint(x - p.x, y - p.y);
            }


            public double Distance(SEPoint p)
            {
                double dx = x - p.x;
                double dy = y - p.y;
                return Math.Sqrt(dx * dx + dy * dy);
            }


            // Signed area / determinant thing
            public double Cross(SEPoint p)
            {
                return x * p.y - y * p.x;
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
                return Resources.SmallestEnclosingCircle_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3A861ADE-FC48-428E-ADC2-1AA1CA4F4F3F"); }
        }
    }
}