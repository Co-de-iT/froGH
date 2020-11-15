using System;
using System.Collections.Generic;
using froGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace froGH
{
    public class NamedViewsToC4D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NamedViewsToC4D class.
        /// </summary>
        public NamedViewsToC4D()
          : base("Named Views To Cinema 4D", "f_NV2C4D",
              "Translates Named Views Data for Cinema 4D",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Update", "U", "Update to get named views data\nUse a button for a one-shot retrieval, a toggle + timer for continuous data", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("C4D Data", "C4D", "Named Views camera data in Cinema 4D format", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool update = false;
            DA.GetData(0, ref update);


            //Extract Named viewports list
            
            Rhino.DocObjects.Tables.NamedViewTable nV = Rhino.RhinoDoc.ActiveDoc.NamedViews;

            // return if list is empty
            if (nV.Count == 0) return;

            DataTree<String> camData = new DataTree<String>();
            //camData.AddRange( new string[]{"name", "lens", "P.X", "P.Y", "P.Z", "H", "P", "B"}, new GH_Path(0));

            ViewportInfo vp;
            List<String> nVData;
            Vector3d dir, up;
            double H, P, B;

            for (int i = 0; i < nV.Count; i++)
            {
                nVData = new List<String>();

                // name
                nVData.Add(nV[i].Name);

                vp = nV[i].Viewport;

                // lens
                nVData.Add(Math.Round(vp.Camera35mmLensLength, 1).ToString());

                // P.X, P.Y, P.Z (in Cinema 4D Y is the up-axis, so X and Y coordinates are swapped)
                nVData.Add(Math.Round(vp.CameraLocation.X, 3).ToString());
                nVData.Add(Math.Round(vp.CameraLocation.Z, 3).ToString());
                nVData.Add(Math.Round(vp.CameraLocation.Y, 3).ToString());

                dir = vp.CameraDirection;
                up = vp.CameraUp;

                // H (Heading)
                Vector3d flatDir = new Vector3d(dir.X, dir.Y, 0);
                H = Vector3d.VectorAngle(Vector3d.XAxis, flatDir, Plane.WorldXY);
                H = Math.Round((180 / Math.PI) * H - 90, 3);
                nVData.Add(Convert.ToString(H));

                // P (Pitch)
                Vector3d invDir = new Vector3d(-dir.X, -dir.Y, -dir.Z);
                P = Vector3d.VectorAngle(invDir, Vector3d.ZAxis);
                P = Math.Round((180 / Math.PI) * P - 90, 3);
                nVData.Add(Convert.ToString(P));

                // B (Bank)
                Vector3d flatRot = Vector3d.CrossProduct(Vector3d.ZAxis, dir);
                B = Vector3d.VectorAngle(up, flatRot);
                B = Math.Round((180 / Math.PI) * B - 90, 3);
                nVData.Add(Convert.ToString(B));

                camData.AddRange(nVData, new GH_Path(i));

            }

            // update if you add new named views to Rhino
            if (update) ExpireSolution(true);

            DA.SetDataTree(0, camData);
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
                return Resources.Camera_to_C4D_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3cc351fe-327a-42d3-8a42-6174ccdd663b"); }
        }
    }
}