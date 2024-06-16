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
              "Translates Named Views Data for Cinema 4D\nUse the input, Double-click or attach a trigger to update",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Update", "U", "Update named views data", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("C4D Data", "C4D", "Named Views camera data in Cinema 4D format:\n" +
                "0 - View name\n1 - lens\n2 - X\n3 - Y\n4 - Z\n5 - Heading\n6 - Pitch\n7 - Bank", GH_ParamAccess.tree);
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
            double heading, pitch, bank;

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

                // H (Heading) or Yaw
                Vector3d flatDir = new Vector3d(dir.X, dir.Y, 0);
                heading = Vector3d.VectorAngle(Vector3d.XAxis, flatDir, Plane.WorldXY);
                heading = Math.Round((180 / Math.PI) * heading - 90, 3);
                nVData.Add(Convert.ToString(heading));

                // P (Pitch)
                Vector3d invDir = new Vector3d(-dir.X, -dir.Y, -dir.Z);
                pitch = Vector3d.VectorAngle(invDir, Vector3d.ZAxis);
                pitch = Math.Round((180 / Math.PI) * pitch - 90, 3);
                nVData.Add(Convert.ToString(pitch));

                // B (Bank) or Roll
                Vector3d flatRot = Vector3d.CrossProduct(Vector3d.ZAxis, dir);
                bank = Vector3d.VectorAngle(up, flatRot);
                bank = Math.Round((180 / Math.PI) * bank - 90, 3);
                nVData.Add(Convert.ToString(bank));

                camData.AddRange(nVData, new GH_Path(i));

            }

            // update if you add new named views to Rhino
            if (update) ExpireSolution(true);

            DA.SetDataTree(0, camData);
        }

        public override void CreateAttributes()
        {
            m_attributes = new NamedViewsToC4D_Attributes(this);
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
                return Resources.NamedViews2C4D_GH;
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