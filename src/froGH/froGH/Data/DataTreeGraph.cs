using System;
using System.Collections.Generic;
using System.Drawing;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    public class DataTreeGraph : GH_Component
    {
        float tree_angle = (float)(0.5 * Math.PI);
        float tree_spread = (float)(0.75 * Math.PI);
        float leaf_angle = (float)((10 / 180.0) * Math.PI);
        Transform orient;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DataTreeGraph()
          : base("DataTreeGraph", "f_DTG",
              "Generates a graphics representation of a Data Tree inscribed in an input Circle" +
                "\n\nOriginal VB.net code by David Rutten, posted in this discussion:" +
                "\nhttps://www.grasshopper3d.com/forum/topics/draw-an-actual-data-tree-from-a-data-tree" +
                "\nTranslated to C# and (only slightly) implemented further by Alessio Erioli - Co-de-iT",
              "froGH", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data Tree", "T", "The input Data Tree", GH_ParamAccess.tree);
            pManager.AddCircleParameter("Inscription Circle", "C", "Circle to inscribe the diagram within", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Twigs", "T", "Data Tree twigs curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Leaves", "L", "Data Tree leaves curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Arcs", "A", "Data Tree arcs curves", GH_ParamAccess.list);
            pManager.AddTextParameter("Twigs text", "Tt", "The Tree Branch Path corresponding to each twig curve", GH_ParamAccess.list);
            pManager.AddTextParameter("Leaves text", "Lt", "The number of items contained in each leaf", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> DT = new GH_Structure<IGH_Goo>();
            if (!DA.GetDataTree(0, out DT)) return;
            
            Circle C = new Circle();
            if(!DA.GetData(1, ref C)) return;

            GH_TreeBuilder builder = new GH_TreeBuilder();

            foreach (GH_Path path in DT.Paths)
                builder.AddPathRecursive(path);

            List<GH_Path> allpaths = builder.AllPaths;
            if (allpaths == null || allpaths.Count == 0) return;

            int maxPathDepth = 0;
            foreach (GH_Path path in allpaths)
                if (path.Length > maxPathDepth) maxPathDepth = path.Length;

            GH_GraphicTreeDisplayArgs args = new GH_GraphicTreeDisplayArgs();
            args.radius = (float)C.Radius;
            args.origin = new PointF(0, 0);

            GH_GraphicBranch tree = new GH_GraphicBranch();

            tree.GrowTree(allpaths);
            tree.SolveLeafAngles(tree_angle, tree_spread, leaf_angle, args);
            tree.Distribute_Phylogenetic(1.0 / args.maxPathLength);

            List<Curve> twigsCurves = new List<Curve>();
            List<Curve> leavesCurves = new List<Curve>();
            List<String> twigsText = new List<String>();
            List<String> levesText = new List<String>();
            List<Point3d> tPoints = new List<Point3d>();
            List<Point3d> lPoints = new List<Point3d>();

            orient = Transform.PlaneToPlane(Plane.WorldXY, C.Plane);

            AppendTreeToDiagram(tree, args, twigsCurves, leavesCurves, twigsText, levesText, tPoints, lPoints);

            // draw arcs
            List<Curve> arcsCurves = new List<Curve>();
            Interval arcSpread = new Interval(-0.4 * Math.PI, 1.4 * Math.PI);
            double rad = C.Radius / maxPathDepth;
            for (int i = 1; i <= maxPathDepth; i++)
            {
                Arc a = new Arc(new Circle(C.Plane, rad * i), arcSpread);
                arcsCurves.Add(a.ToNurbsCurve());
            }

            // fill leaves text
            for (int i = 0; i < DT.Branches.Count; i++)
                levesText.Add(DT.Branches[i].Count.ToString());

            DA.SetDataList(0, twigsCurves);
            DA.SetDataList(1, leavesCurves);
            DA.SetDataList(2, arcsCurves);
            DA.SetDataList(3, twigsText);
            DA.SetDataList(4, levesText);
        }

        void AppendTreeToDiagram(GH_GraphicBranch branch, GH_GraphicTreeDisplayArgs args,
    List<Curve> list, List<Curve> leaves, List<String> text, List<String> textL, List<Point3d> tPoints, List<Point3d> lPoints)
        {
            if (branch.Twigs != null)
            {
                foreach (GH_GraphicBranch twig in branch.Twigs)
                    AppendTreeToDiagram(twig, args, list, leaves, text, textL, tPoints, lPoints);
            }

            if (branch.IsRoot) return;
            if (branch.Length < 0.001) return;
            if (branch.IsLeaf)
            {
                PointF p0 = args.RadialCrd(branch.Angle, branch.Offset);
                PointF p1 = args.RadialCrd(branch.Angle, branch.Offset + branch.Length);
                Curve c = new LineCurve(new Point3d(p0.X, -p0.Y, 0.0), new Point3d(p1.X, -p1.Y, 0.0));
                c.Transform(orient);
                lPoints.Add(c.PointAtNormalizedLength(0.5));
                leaves.Add(c);
            }
            else
            {
                int count = 0;
                foreach (GH_GraphicBranch twig in branch.Twigs)
                {
                    PointF p0, p1, p2, p3;
                    SolveBezier(branch, twig, args, out p0, out p1, out p2, out p3);
                    Point3d pa = new Point3d(p0.X, -p0.Y, 0.0);
                    Point3d pb = new Point3d(p1.X, -p1.Y, 0.0);
                    Point3d pc = new Point3d(p2.X, -p2.Y, 0.0);
                    Point3d pd = new Point3d(p3.X, -p3.Y, 0.0);
                    Curve c = Curve.CreateControlPointCurve(new Point3d[] { pa, pb, pc, pd }, 3);
                    c.Transform(orient);
                    tPoints.Add(c.PointAt(0.5));
                    list.Add(c);
                    text.Add(twig.Path.ToString());
                    count++;
                }
            }
        }

        void SolveBezier(GH_GraphicBranch twig, GH_GraphicBranch nextTwig, GH_GraphicTreeDisplayArgs e,
          out PointF P0, out PointF P1, out PointF P2, out PointF P3)
        {
            float A0 = twig.Angle;
            float A1 = nextTwig.Angle;

            float O0 = twig.Offset;
            float O3 = nextTwig.Offset;

            float O1 = O0 + 0.55F * (O3 - O0);
            float O2 = O0 + 0.45F * (O3 - O0);

            P0 = e.RadialCrd(A0, O0);
            P1 = e.RadialCrd(A0, O1);
            P2 = e.RadialCrd(A1, O2);
            P3 = e.RadialCrd(A1, O3);
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
                return Resources.TreeGraph_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("81A80F87-8F3C-4874-9778-C76781260714"); }
        }
    }
}