using System;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class FastMeshFromPolyline : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FastMeshFromPolyline class.
        /// </summary>
        public FastMeshFromPolyline()
          : base("Fast Mesh From Polyline", "f_FMesh",
              "Creates a fast mesh from a closed polyline",
              "froGH", "Mesh-Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "Closed Polylines", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Fast and cheap Meshes", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
                 base  code by James Ramsden - http://james-ramsden.com/fastmesh-v2-the-stupidly-easy-way-of-creating-efficient-meshes-from-polylines/
                 parallel implementation by Alessio Erioli
             */

            List<Polyline> P = new List<Polyline>();
            List<Curve> C = new List<Curve>();
            if (!DA.GetDataList(0, C)) return;
            if (C == null || C.Count == 0) return;

            for (int i = 0; i < C.Count; i++)
            {
                Polyline pp;
                if (C[i].TryGetPolyline(out pp)) P.Add(pp);
            }    

            if (P.Count < 10000)
            { // change the number if you want to change parallelization threshold
                List<Mesh> m = new List<Mesh>();

                for (int i = 0; i < P.Count; i++)
                {
                    m.Add(Mesh.CreateFromClosedPolyline(P[i]));
                }

                DA.SetDataList(0, m);

            }
            else
            {


                var mp = new System.Collections.Concurrent.ConcurrentDictionary<Polyline, Mesh>(Environment.ProcessorCount, P.Count);
                foreach (Polyline p in P) mp[p] = new Mesh(); //initialise dictionary

                System.Threading.Tasks.Parallel.ForEach(P, p =>
                {
                    mp[p] = Mesh.CreateFromClosedPolyline(p); //save your output here
                }
                  );

                DA.SetDataList(0, mp.Values.ToList());
            }
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
                return Resources.fast_mesh_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("592fdc17-3013-4df6-a5dd-a77b139239be"); }
        }
    }
}