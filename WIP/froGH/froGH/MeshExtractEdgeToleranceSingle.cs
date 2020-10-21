using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using GH_IO.Types;
using froGH.Properties;

namespace froGH
{
    public class MeshExtractEdgeToleranceSingle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshExtractEdgeToleranceSingle class.
        /// </summary>
        public MeshExtractEdgeToleranceSingle()
          : base("Mesh Extract Edges with tolerance angle", "f_MsEEA",
              "Extract Mesh edges with specific angle tolerance.\n\nOptimized for a single mesh",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Input Mesh to process", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "The Tolerance Angle (in degrees)", GH_ParamAccess.item, 45);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Internal Edges", "Ei", "Internal edges lines under tolerance", GH_ParamAccess.list);
            pManager.AddLineParameter("Naked Edges", "En", "Naked edges lines", GH_ParamAccess.list);
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

            double A = 45.0;
            DA.GetData(1, ref A);

            double num = A / 57.2958;

            ConcurrentBag<Line> list = new ConcurrentBag<Line>();
            ConcurrentBag<Line> list2 = new ConcurrentBag<Line>();


            M.Normals.ComputeNormals();
            Rhino.Geometry.Collections.MeshTopologyEdgeList topologyEdges = M.TopologyEdges;

            int num2 = topologyEdges.Count - 1;
            Parallel.For(0, topologyEdges.Count, i =>
            //for (int i = 0; i <= num2; i++)
            {
                int[] connectedFaces = topologyEdges.GetConnectedFaces(i);
                if (connectedFaces.Length < 2)
                    list2.Add(topologyEdges.EdgeLine(i));
                
                if (connectedFaces.Length == 2)
                {
                    Vector3f val2 = M.FaceNormals[connectedFaces[0]];
                    Vector3f val3 = M.FaceNormals[connectedFaces[1]];
                    double num3 = Vector3d.VectorAngle(new Vector3d((double)val2.X, (double)val2.Y, (double)val2.Z),
                      new Vector3d((double)val3.X, (double)val3.Y, (double)val3.Z));
                    if (num3 > num)
                        list.Add(topologyEdges.EdgeLine(i));

                }
            });

            DA.SetDataList(0, list);
            DA.SetDataList(1, list2);
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
                return Resources.Extract_mesh_edges_angle_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("66843ed4-d04d-4771-824e-5a92038c67d1"); }
        }
    }
}