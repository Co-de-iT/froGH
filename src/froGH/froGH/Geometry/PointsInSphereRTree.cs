using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH.Geometry
{
    public class PointsInSphereRTree : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointsInSphereRTree class.
        /// </summary>
        public PointsInSphereRTree()
          : base("PointsInSphereRTree", "f_PtsSphRT",
              "Detects Points within a Sphere of given center and radius in a given RTree\nUse this for fixed search sets\nFor dynamic search sets use the other Points In Sphere",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Sphere Center", "P", "Point to search from (needle)", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Search Radius", "R", "Search Radius", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RTree", "RT", "The RTree to search", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "The Points within the sphere radius", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Points indices", "i", "Indices of the points in the original list", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> points = new GH_Structure<GH_Point>();
            GH_Structure<GH_Number> indices = new GH_Structure<GH_Number>();
            GH_Structure<IGH_Goo> fTrees = new GH_Structure<IGH_Goo>();

            if (!DA.GetDataTree(0, out points)) return;
            if (!DA.GetDataTree(1, out indices)) return;
            if (!DA.GetDataTree(2, out fTrees)) return;

            GH_Structure<GH_Point> foundPts = new GH_Structure<GH_Point>();
            GH_Structure<GH_Integer> foundInd = new GH_Structure<GH_Integer>();

            for (int i = 0; i < points.PathCount; i++)
            {
                GH_Path path = points.Paths[i];
                List<IGH_Goo> fTreeList = fTrees.Branches[fTrees.PathCount < i ? fTrees.PathCount - 1 : i];
                if (fTreeList.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No RTree for branch" + points.Paths[i].ToString());
                    return;
                }
                if (fTreeList.Count > 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Multiple RTrees for branch" + points.Paths[i].ToString() + "to search multiple RTrees place them in individual branches");
                    return;
                }
                froGHRTree fTree = ((GH_froGHRTree)fTreeList[0]).Value;
                List<GH_Number> indList = indices.Branches[indices.PathCount < i ? indices.PathCount - 1 : i];
                int count = 0;
                foreach (GH_Point ghP in points.Branches[i])
                {
                    GH_Path foundPath = path.AppendElement(count);
                    // perform search
                    List<int> fIndices = fTree.IndicesInSphere(ghP.Value, indList[indList.Count > count ? count : indList.Count - 1].Value);
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
                return Resources.PointsInSphereRTree_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("95705848-2386-41cd-8c50-e1e56c9fd0e4"); }
        }
    }
}