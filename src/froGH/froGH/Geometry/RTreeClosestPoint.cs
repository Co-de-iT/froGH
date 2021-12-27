using froGH.Properties;
using froGH.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class RTreeClosestPoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RTreeClosestPoint class.
        /// </summary>
        public RTreeClosestPoint()
          : base("RTree Closest Point", "f_RTreeCP",
              "Returns the Closest Point to a given RTree",
              "froGH", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Search Points", "P", "Points to search from (needle points)", GH_ParamAccess.tree);
            pManager.AddGenericParameter("RTree", "RT", "The RTree to search", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Closest Point", "P", "The closest Point in the RTree", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Closest Points index", "i", "Index of the closest Point in the RTree", GH_ParamAccess.tree);
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

            GH_Structure<GH_Point> points = new GH_Structure<GH_Point>();
            GH_Structure<IGH_Goo> fTrees = new GH_Structure<IGH_Goo>();
            DA.GetDataTree(0, out points);
            DA.GetDataTree(1, out fTrees);
            GH_Structure<GH_Integer> foundInd = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Point> foundPts = new GH_Structure<GH_Point>();
            for (int i = 0; i < points.PathCount; i++)
            {
                GH_Path path = points.Paths[i];
                List<IGH_Goo> fTreeList = fTrees.Branches[(fTrees.PathCount > i) ? i : (fTrees.PathCount - 1)];
                if (fTreeList.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No RTree for branch " + points.Paths[i].ToString());
                    return;
                }
                if (fTreeList.Count > 1)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Multiple RTrees for branch " + points.Paths[i].ToString() + "to search multiple RTrees place them in individual branches");
                    return;
                }
                froGHRTree fRTree = ((GH_froGHRTree)fTreeList[0]).Value;
                foreach (GH_Point ghP in points.Branches[i])
                {
                    int ind = fRTree.ClosestPointIndex(ghP.Value);
                    foundPts.Append(new GH_Point((ind > -1) ? fRTree.Points[ind] : ghP.Value));
                    foundInd.Append(new GH_Integer(ind), path);
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
                return Resources.RTreeClosestPoint_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6587b9a0-25e9-4f0f-8ea9-ad543cc025e9"); }
        }
    }
}