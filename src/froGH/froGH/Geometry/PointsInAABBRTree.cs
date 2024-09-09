using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class PointsInAABBRTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointsInAABBRTree class.
        /// </summary>
        public PointsInAABBRTree()
          : base("Points In AABB (RTree)", "f_PtsAABBRT",
              "Detects Points within a Bounding Box in a given RTree\nUse this for fixed search sets\nFor dynamic search sets use the other Points In AABB",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Bounding Box", "B", "The World XYZ aligned Bounding Box for Search", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RTree", "RT", "The RTree to search", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "The Points within the Bounding Box", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Points indices", "i", "Indices of the points in the original list", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // The idea of using a fixed RTree (via a custom class) for faster search
            // comes from the LunchBox plugin (it's actually implemented there already)
            // I already had a component with dynamic RTree search by Sphere, it only made sense
            // to me to complete the set. Plus, I had the chance to study and learn some new tricks!

            GH_Structure<GH_Box> boxes = new GH_Structure<GH_Box>();
            GH_Structure<IGH_Goo> fTrees = new GH_Structure<IGH_Goo>();

            if (!DA.GetDataTree(0, out boxes)) return;
            if (!DA.GetDataTree(1, out fTrees)) return;

            if (boxes == null) return;
            if (fTrees == null) return;

            GH_Structure<GH_Point> foundPts = new GH_Structure<GH_Point>();
            GH_Structure<GH_Integer> foundInd = new GH_Structure<GH_Integer>();

            for (int i = 0; i < boxes.PathCount; i++)
            {
                GH_Path path = boxes.Paths[i];
                List<IGH_Goo> fTreeList = fTrees.Branches[i < fTrees.PathCount - 1 ? i : fTrees.PathCount - 1];
                if (fTreeList.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No RTree for branch" + boxes.Paths[i].ToString());
                    return;
                }
                if (fTreeList.Count > 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Multiple RTrees for branch" + boxes.Paths[i].ToString() + "to search multiple RTrees place them in individual branches");
                    return;
                }
                FroGHRTree fTree = ((GH_froGHRTree)fTreeList[0]).Value;
                int count = 0;
                foreach (GH_Box ghB in boxes.Branches[i])
                {
                    if (ghB == null) continue;
                    GH_Path foundPath = path.AppendElement(count);
                    // perform search
                    List<int> fIndices = fTree.IndicesInAABB(ghB.Value.BoundingBox);
                    if (fIndices.Count == 0)
                    {
                        foundPts.AppendRange(new List<GH_Point>(), foundPath);
                        foundInd.AppendRange(new List<GH_Integer>(), foundPath);
                    }
                    else
                    {
                        foreach (int foundIndex in fIndices)
                        {
                            foundPts.Append(new GH_Point(fTree.Points[foundIndex]), foundPath);
                            foundInd.Append(new GH_Integer(foundIndex), foundPath);
                        }
                    }

                    count++;
                }

            }
            DA.SetDataTree(0, foundPts);
            DA.SetDataTree(1, foundInd);
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
                return Resources.PointsInAABBRTree_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FD56D4D2-59F7-4B8C-BBED-67215C5C67C7"); }
        }
    }
}