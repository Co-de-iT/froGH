using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class ExtractMeshEdges : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExtractMeshEdges class.
        /// </summary>
        public ExtractMeshEdges()
          : base("Extract Mesh Edges", "f_MEdges",
              "Extract Mesh Edges as Lines",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Edges", "E", "Mesh edges as lines", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Weld Status", "W", "True if edge is unwelded", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Edge status", "S", "Edge status:\n-1 isolated\n0 naked\n1 manifold\n2+ non manifold", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            if (!DA.GetData(0, ref M)) return;
            if (!M.IsValid || M == null) return;

            Line[] edgeLines = new Line[M.TopologyEdges.Count];
            bool[] weldedStatus = new bool[M.TopologyEdges.Count];
            int[] nakedStatus = new int[M.TopologyEdges.Count];

            for (int i = 0; i < M.TopologyEdges.Count; i++)
            {
                edgeLines[i] = M.TopologyEdges.EdgeLine(i);
                weldedStatus[i]=M.TopologyEdges.IsEdgeUnwelded(i);
                nakedStatus[i] = M.TopologyEdges.GetConnectedFaces(i).Length-1;
            }

            DA.SetDataList(0, edgeLines);
            DA.SetDataList(1, weldedStatus);
            DA.SetDataList(2, nakedStatus);
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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
                return Resources.ExtractMeshEdges_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("70AA9E8E-FFF2-4F6B-B5AE-04ECDD58D0B1"); }
        }
    }
}