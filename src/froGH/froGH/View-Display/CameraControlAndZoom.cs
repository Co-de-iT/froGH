using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class CameraControlAndZoom : GH_Component
    {
        //To get and set camera properties
        Rhino.Display.RhinoViewport vp;

        /// <summary>
        /// Initializes a new instance of the CameraControl class.
        /// </summary>
        public CameraControlAndZoom()
          : base("Camera Control And Zoom To Geometry", "f_CamConZ2Geo",
              "Controls Rhino camera from Grasshopper, with the ability to Zoom to a given geometry",
              "froGH", "View/Display")
        {
            Params.ParameterSourcesChanged += new GH_ComponentParamServer.ParameterSourcesChangedEventHandler(ParamSourceChanged);
        }

        // this autolist method is from: https://discourse.mcneel.com/t/automatic-update-of-valuelist-only-when-connected/152879/6?u=ale2x72
        // works much better as it does not clog the solver with exceptions if a list of numercal values is connected
        private void ParamSourceChanged(object sender, GH_ParamServerEventArgs e)
        {
            if ((e.ParameterSide == GH_ParameterSide.Input) && (e.ParameterIndex == 2))
            {
                foreach (IGH_Param source in e.Parameter.Sources)
                {
                    if (source is Grasshopper.Kernel.Special.GH_ValueList)
                    {
                        Grasshopper.Kernel.Special.GH_ValueList vList = source as Grasshopper.Kernel.Special.GH_ValueList;

                        if (!vList.NickName.Equals("Camera type"))
                        {
                            vList.ClearData();
                            vList.ListItems.Clear();
                            vList.NickName = "Camera type";
                            var item0 = new Grasshopper.Kernel.Special.GH_ValueListItem("Viewport's mode", "-1");
                            var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Perspective", "0");
                            var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Parallel", "1");
                            var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Two-point Perspective", "2");

                            vList.ListItems.Add(item0);
                            vList.ListItems.Add(item1);
                            vList.ListItems.Add(item2);
                            vList.ListItems.Add(item3);

                            vList.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown; // change this for a different mode (DropDown is the default)
                            vList.ExpireSolution(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to frame with Zoom option\n" +
                                            "Leave empty for no zoom\n" +
                                            "Ignored if Zoom option is false", GH_ParamAccess.list);
            pManager.AddTextParameter("Viewport Name", "V", "Name of the Rhino Viewport to control \nleave empty for active viewport", GH_ParamAccess.item, "");
            pManager.AddIntegerParameter("Camera Type", "Ct", "Camera Type (attach a value list for autovalue, leave empty for viewport's)\n" +
                                            "-1 - Viewport camera type (default)\n" +
                                            "0 - Perspective\n" +
                                            "1 - Parallel\n" +
                                            "2 - Two-point Perspective", GH_ParamAccess.item, -1);
            pManager.AddTextParameter("Display Mode", "Dm", "Set Display Mode by Name\n" +
                "Leave empty for Viewport's one", GH_ParamAccess.item, "");
            pManager.AddPointParameter("Camera location", "C", "Camera location\n" +
                "Leave empty for Viewport's one", GH_ParamAccess.item);
            pManager.AddPointParameter("Target location", "T", "Target location\n" +
                "Leave empty for Viewport's one", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lens", "L", "Lens\n" +
                "Leave empty for Viewport's one", GH_ParamAccess.item);
            pManager.AddVectorParameter("Up Vector", "u", "Up Vector\n" +
                "Leave empty for Viewport's one", GH_ParamAccess.item);
            pManager.AddNumberParameter("Zoom factor", "Zf", "Zoom factor\n" +
                                             "zoom out < 0 > zoom in\n" +
                                             "Ignored if G has no input", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("Zoom", "z", "Zoom on the input Geometry\n" +
                                             "use a Toggle\n" +
                                             "Ignored if G has no input", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Activate", "a", "Take control or release it back to Rhino\n" +
                                             "use a Toggle", GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
            pManager[10].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("View Name Output", "O", "View Name output (use with View Capture To File)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> G = new List<GeometryBase>();
            DA.GetDataList(0, G);

            string viewName = "";
            DA.GetData("Viewport Name", ref viewName);

            //Get viewport
            if (String.IsNullOrWhiteSpace(viewName))
            {
                vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            }
            else
            {
                try
                {
                    vp = Rhino.RhinoDoc.ActiveDoc.Views.Find(viewName, false).MainViewport;
                }
                catch
                {
                    vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
                }
            }

            int camType = 0;
            DA.GetData("Camera Type", ref camType);

            string dispayModeName = "";
            DA.GetData("Display Mode", ref dispayModeName);

            Point3d location = new Point3d();
            if (!DA.GetData("Camera location", ref location)) location = vp.CameraLocation;

            Point3d target = new Point3d();
            if (!DA.GetData("Target location", ref target)) target = vp.CameraTarget;

            double lens = 0;
            if (!DA.GetData("Lens", ref lens)) lens = vp.Camera35mmLensLength;
            if (lens == 0) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Lens cannot be zero");

            Vector3d up = Vector3d.ZAxis;
            if (!DA.GetData("Up Vector", ref up)) up = vp.CameraUp;

            double zoomFactor = 1.0;
            DA.GetData("Zoom factor", ref zoomFactor);

            bool zoom = false;
            DA.GetData("Zoom", ref zoom);

            bool activate = false;
            DA.GetData("Activate", ref activate);

            if (!activate)
            {
                Message = "";
                return;
            }

            Message = vp.Name;

            //Set Display Mode
            Rhino.Display.DisplayModeDescription displayMode = Rhino.Display.DisplayModeDescription.FindByName(dispayModeName);
            if (displayMode != null)
                vp.DisplayMode = displayMode;

            //Set camera

            switch (camType)
            {
                case -1:// do nothing (keep actual projection and change lens only if not parallel)
                    if (!vp.IsParallelProjection) vp.Camera35mmLensLength = lens;
                    break;
                case 0:
                    vp.ChangeToPerspectiveProjection(true, lens);
                    break;
                case 1:
                    vp.ChangeToParallelProjection(true);
                    break;
                case 2:
                    vp.ChangeToTwoPointPerspectiveProjection(lens);
                    break;
                default:
                    goto case -1;
            }

            // Check whether to zoom or use cam position and target
            // if geometry is null zoom is ignored and if zoom is false geomtry is ignored
            if (zoom && (G != null && G.Count > 0))
            {
                BoundingBox bb, bTemp;
                bb = BoundingBox.Empty;
                for (int i = 0; i < G.Count; i++)
                {
                    if (G[i] != null && G[i].IsValid)
                    {
                        bTemp = G[i].GetBoundingBox(false);
                        bb.Union(bTemp);
                    }
                }
                if (zoomFactor < 0.0)
                    bb.Inflate(bb.Diagonal.Length * -zoomFactor);

                vp.ZoomBoundingBox(bb);
                if (zoomFactor > 0.0)
                    vp.Magnify(zoomFactor + 1.0, false);
            }
            else
            {
                vp.SetCameraLocations(target, location);
                vp.CameraUp = up;
            }

            DA.SetData(0, viewName);
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
                return Resources.CameraControlZoom2Geometry_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("077DF442-0235-40E3-B562-77B88B5AECC4"); }
        }
    }
}