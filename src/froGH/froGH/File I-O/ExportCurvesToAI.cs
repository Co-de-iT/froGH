using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class ExportCurvesToAI : GH_Component
    {
        private bool pending = false;

        /// <summary>
        /// Initializes a new instance of the ExportCurvesToAI class.
        /// </summary>
        public ExportCurvesToAI()
          : base("Export Curves to AI", "f_AIexp",
              "Export curves to Adobe Illustrator format, retaining layers",
              "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves to export", GH_ParamAccess.list);
            pManager.AddTextParameter("Layer", "L", "Name of curves destination layer (one value per curve)", GH_ParamAccess.list);
            pManager.AddColourParameter("Layer Color", "LC", "Layer Color (one value per curve)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Preserve Units", "PU", "Preserve Units\nTrue to preserve units, False to export a view snapshot scaled to fit", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("View Bounds", "VB", "Viewport Bounds\nTrue to draw the viewport bounds rectangle", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Color Style", "CS", "Color Style option\nTrue for RGB, False for CMYK", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Solid Hatch", "SH", "Solid Hatch option\nTrue to export Hatches as solid fills, False to export them as lines", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Order Layers", "OL", "Order Layers option\nTrue to export Layers in their display order, False to export them by indices", GH_ParamAccess.item, true);
            pManager.AddTextParameter("File Path", "P", "File Path", GH_ParamAccess.item);
            pManager.AddTextParameter("File Name", "F", "File Name", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Export trigger", "E", "connect a Button or Toggle and set to True to export", GH_ParamAccess.item);

            for (int i = 3; i < 8; i++) pManager[i].Optional = true;
            pManager[10].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> C = new List<Curve>();
            List<string> Layer = new List<string>();
            List<Color> LColor = new List<Color>();
            if (!DA.GetDataList(0, C)) return;
            if (C == null || C.Count == 0) return;

            if (!DA.GetDataList(1, Layer))
                for (int i = 0; i < C.Count; i++) Layer.Add("Default");
            if (Layer == null || Layer.Count != C.Count) return;

            if (!DA.GetDataList(2, LColor))
                for (int i = 0; i < C.Count; i++) LColor.Add(Color.Black);
            if (LColor == null || LColor.Count != C.Count) return;

            bool PreserveUnits = true, ViewBound = true, ColStyle = true, SolidHatch = true, OrderLayers = true;
            DA.GetData(3, ref PreserveUnits);
            DA.GetData(4, ref ViewBound);
            DA.GetData(5, ref ColStyle);
            DA.GetData(6, ref SolidHatch);
            DA.GetData(7, ref OrderLayers);

            string path = "", name = "";
            if (!DA.GetData(8, ref path)) return;
            if (!DA.GetData(9, ref name)) return;

            bool export = false;
            DA.GetData(10, ref export);

            // NOTE
            // normally, a button would freeze GH canvas after a RunScript operation, while a Toggle would not;
            // this fix (use a pending variable) was suggested by Florian Frank here:
            // https://www.grasshopper3d.com/forum/topics/boolean-button-runscript
            if (!export && !pending) return;
            
            // as suggested by Rutten here: https://discourse.mcneel.com/t/rhino-command-export-in-c/63610/4
            if (Rhino.RhinoDoc.ActiveDoc == null) return;

            if (!pending)
            {
                pending = true;
                return;
            }

            pending = false;
            // export options
            string presUn = PreserveUnits ? "Yes" : "No";
            string vBound = ViewBound ? "Yes" : "No";
            string cs = ColStyle ? "RGB" : "CMYK";
            string solHatch = SolidHatch ? "Yes" : "No";
            string ordLay = OrderLayers ? "Yes" : "No";

            //                                         Yes/No                          Yes/No            RGB/CMYK                          Yes/No                      Yes/No
            string scriptOptions = " PreserveUnits=" + presUn + " ViewportBoundary=" + vBound + " Color=" + cs + " HatchesAsSolidFills=" + solHatch + " OrderLayers=" + ordLay;
            string scriptString = "-_Export " + '"' + path + name + ".ai" + '"' + scriptOptions + " _Enter";

            Guid[] id = new Guid[C.Count];
            string[] lay = Layer.ToArray();
            Color[] col = LColor.ToArray();

            // lists of effectively added layers
            List<int> addedLayers = new List<int>();

            //Make new attribute to set name
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            Rhino.RhinoApp.RunScript("_SelNone", false);
            IEnumerator it = C.GetEnumerator();
            int k = 0;
            int ind = 0;
            int cInd = 0;

            foreach (Curve curve in C)
            {

                //Set layer
                if (!string.IsNullOrEmpty(lay[ind]) && Rhino.DocObjects.Layer.IsValidName(lay[ind]))
                {
                    //Get the current layer index
                    Rhino.DocObjects.Tables.LayerTable layerTable = Rhino.RhinoDoc.ActiveDoc.Layers;
                    //int layerIndex = layerTable.Find(lay[ind], true);
                    int layerIndex = layerTable.FindByFullPath(lay[ind], -1);

                    if (layerIndex < 0) //This layer does not exist, we add it
                    {
                        Rhino.DocObjects.Layer onlayer = new Rhino.DocObjects.Layer(); //Make a new layer
                        onlayer.Name = lay[ind];

                        onlayer.Color = col[ind];

                        layerIndex = layerTable.Add(onlayer); //Add the layer to the layer table
                        if (layerIndex > -1) //We managed to add layer!
                        {
                            att.LayerIndex = layerIndex;
                            addedLayers.Add(layerIndex); // add index to the newly added layers list
                                                         //Print("Added new layer to the document at position " + layerIndex + " named " + Layer + ". ");
                        } //else
                          //Print("Layer did not add. Try cleaning up your layers."); //This never happened to me.
                    }
                    else
                        att.LayerIndex = layerIndex; //We simply add to the existing layer
                }

                // bake and select the object
                id[k] = Rhino.RhinoDoc.ActiveDoc.Objects.AddCurve(curve, att);
                Rhino.RhinoDoc.ActiveDoc.Objects.Select(id[k], true);

                // increment indexes
                k++;
                if (ind < lay.Length - 1) ind++;
                if (cInd < col.Length - 1) cInd++;
            }

            Rhino.RhinoApp.RunScript(scriptString, false);
            Rhino.RhinoApp.RunScript("_delete", false);


            // delete only the added layers
            Rhino.DocObjects.Tables.LayerTable layerTableNew = Rhino.RhinoDoc.ActiveDoc.Layers;

            foreach (int lInd in addedLayers)
                layerTableNew.Delete(lInd, true);

        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
                return Resources.Export_to_AI_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("04460d9b-c999-415e-acb0-46d7deadf054"); }
        }
    }
}