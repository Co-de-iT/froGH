using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace froGH
{
    public class Clusterizer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Clusterizer class.
        /// </summary>
        public Clusterizer()
          : base("Clusterizer", "f_Clstr",
              "Groups topology data into clusters of indexes",
               "froGH", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Topology Data", "T", "List of correspondances\ntree branch path index is the index of the current element\ntree branch content is a list of indexes of connected elements", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Clusters", "C", "Indexes grouped by clusters", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> Ts;
            if (!DA.GetDataTree(0, out Ts)) return;
            if (Ts.Branches.Count == 0) return;

            if (Ts.Branches.Count == 1)
            {
                DA.SetData(0, Ts);
                return;
            }

            // converts GH_Structure in DataTree of int
            DataTree<int> Td = new DataTree<int>();
            for (int i = 0; i < Ts.Branches.Count; i++)
                Td.AddRange(Ts.Branches[i].Select(n => n.Value).ToList(), Ts.Paths[i]);

            DataTree<int> clusters = new DataTree<int>(); // tree for clusters
            List<int> tP = new List<int>(); // list of branch indexes for Td
            for (int i = 0; i < Td.BranchCount; i++) tP.Add(i);

            // start from first branch: call clusterize with 0 both as cb and sb
            clusterize(0, 0, ref Td, ref clusters, ref tP);

            DA.SetDataTree(0, clusters);
        }

        void clusterize(int cb, int sb, ref DataTree<int> Td, ref DataTree<int> cl, ref List<int> tP)
        {
            // current branch path
            GH_Path cp = new GH_Path(cb);

            if (cb == sb) // when cb==sb create a new branch in cl
            {
                //
                // create new cluster
                //
                GH_Path clp = new GH_Path(cl.BranchCount); //new cluster path

                cl.Add(cb, clp); // add Td branch index as first element
                                 // add all branch indexes
                cl.AddRange(Td.Branches[cb]);

                // call clusterize for each of them
                for (int i = 0; i < Td.Branches[cb].Count; i++)
                    clusterize(Td[cp, i], cb, ref Td, ref cl, ref tP);

                tP.RemoveAt(tP.IndexOf(cb));

                //
                // call new cluster if there are still available indexes
                //
                if (tP.Count > 0) clusterize(tP[0], tP[0], ref Td, ref cl, ref tP);
            }
            else
            {
                //
                // add elements to existing cluster
                //

                // scan elements of current branch
                for (int i = 0; i < Td.Branches[cb].Count; i++)
                {
                    // if elements in current branch (cb) of Td are different
                    // from sending branch index (sb):
                    if (Td[cp, i] != sb)
                    {
                        // if elements are not yet in cluster, add them
                        // and call clusterize on them
                        if (!cl.Branches[cl.BranchCount - 1].Contains(Td[cp, i]))
                        {
                            cl.Add(Td[cp, i], new GH_Path(cl.BranchCount - 1));
                            clusterize(Td[cp, i], cb, ref Td, ref cl, ref tP);
                        }
                    }
                    else if (tP.Contains(cb)) tP.RemoveAt(tP.IndexOf(cb)); //remove branch index from list
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
                return Resources.Clusterizer_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("47f97948-d55a-47dd-a639-72773bc6c612"); }
        }
    }
}