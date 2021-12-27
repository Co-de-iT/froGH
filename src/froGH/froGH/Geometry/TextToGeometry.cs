using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using froGH.Properties;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace froGH
{
    public class TextToGeometry : GH_Component
    {
        private string outputType;

        private readonly string[][] justification = new string[][]{ new string[] { "TopLeft", "0" }, new string[] { "TopCenter", "1" }, new string[]{ "TopRight", "2" },
            new string[] { "MiddleLeft", "3" },  new string[] { "MiddleCenter", "4" }, new string[]{ "MiddleRight", "5" },
            new string[] { "BottomLeft", "6" }, new string[]{ "BottomCenter", "7" },   new string[]{ "BottomRight", "8" } };

        /// <summary>
        /// Initializes a new instance of the FontParser class.
        /// </summary>
        public TextToGeometry()
          : base("Text To Geometry", "f_T2Geom",
              "Converts text into geometry (Polylines, Curves or Surfaces)\nuse single-line fonts for best results in fabrication",
              "froGH", "Geometry")
        {
            outputType = GetValue("TextOutputType", "Polylines");
            UpdateMessage();
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text string", "T", "Text to convert", GH_ParamAccess.item);
            pManager.AddTextParameter("Font face", "F", "Font face\nUse Font List component for a list of available fonts", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Closed", "C", "Closed Text Curves/Polylines", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Size", "S", "Text size (in Rhino units)", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Sampling Precision", "sp", "Sampling Precision\nAffects Polylines only\n>1 - Usually a value between 10-100 is ok", GH_ParamAccess.item, 50);
            pManager.AddPlaneParameter("Base Plane", "P", "Plane for orienting Text", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Justification", "J", "text justification (0-8) - attach a value list for autovalues", GH_ParamAccess.item, 0);

            for (int i = 2; i < pManager.ParamCount; i++)
                pManager[i].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Text Geometry", "T", "Text as geometry", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string S = "";
            DA.GetData(0, ref S);
            string fontString = "";
            DA.GetData(1, ref fontString);
            bool close = false;
            DA.GetData(2, ref close);
            double size = 0.0;
            DA.GetData(3, ref size);
            double precision = 1.0;
            DA.GetData(4, ref precision);
            Plane basePlane = new Plane();
            DA.GetData(5, ref basePlane);
            int J = 0;
            DA.GetData(6, ref J);

            // __________________ autoList part __________________

            // variable for the list
            Grasshopper.Kernel.Special.GH_ValueList vList;
            // tries to cast input as list
            try
            {

                // if the list is not the first parameter then change Input[6] to the corresponding value
                vList = Params.Input[6].Sources[0] as Grasshopper.Kernel.Special.GH_ValueList;

                // check if the list must be created

                if (!vList.NickName.Equals("Justification"))
                {
                    vList.ClearData();
                    vList.ListItems.Clear();
                    vList.NickName = "Justification";

                    for (int i = 0; i < justification.Length; i++)
                        vList.ListItems.Add(new Grasshopper.Kernel.Special.GH_ValueListItem(justification[i][0], justification[i][1]));

                    vList.ListItems[0].Value.CastTo(out J);
                }
            }
            catch
            {
                // handles anything that is not a value list
            }

            // Align transformation
            Transform align = Transform.PlaneToPlane(Plane.WorldXY, basePlane);

            float fontSize = size == 0 ? 1 : (float)size;
            precision = Math.Max(1.0, precision);
            System.Drawing.Font local_font = new System.Drawing.Font(fontString, (float)fontSize);

            // define text entity
            TextEntity text = new TextEntity();
            text.PlainText = S;
            DimensionStyle myStyle = new DimensionStyle();
            myStyle.Font = new Rhino.DocObjects.Font(fontString);
            TextHorizontalAlignment horizontal;
            TextVerticalAlignment vertical;
            MapJustification(J, out vertical, out horizontal);
            myStyle.TextVerticalAlignment = vertical;
            myStyle.TextHorizontalAlignment = horizontal;
            myStyle.TextHeight = size;
            myStyle.DrawForward = false;
            text.Transform(align);

            // get value of appended menu item
            //string outputType = GetValue("OutputType", "Polylines");
            if (outputType == "Curves")
            {
                Curve[] textCurves = text.CreateCurves(myStyle, !close); // 1.0,0.0);

                DA.SetDataList(0, textCurves);

            }
            else if (outputType == "Surfaces")
            {
                Brep[] textSurfaces = text.CreateSurfaces(myStyle);

                DA.SetDataList(0, textSurfaces);

            }
            else if (outputType == "Polylines")
            {
                Curve[] textCurves = text.CreateCurves(myStyle, !close); // 1.0,0.0);

                PolylineCurve[] textPolylines = new PolylineCurve[textCurves.Length];

                double minSeg = 5.0 / precision;

                for (int i = 0; i < textCurves.Length; i++)
                    textPolylines[i] = textCurves[i].ToPolyline(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, Rhino.RhinoDoc.ActiveDoc.ModelAngleToleranceRadians, minSeg, 0);

                DA.SetDataList(0, textPolylines);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            ToolStripMenuItem toolStripMenuItem = Menu_AppendItem(menu, "Curves", Curves_Click, true, GetValue("TextOutputType", "Polylines") == "Curves");
            ToolStripMenuItem toolStripMenuItem2 = Menu_AppendItem(menu, "Polylines", Polylines_Click, true, GetValue("TextOutputType", "Polylines") == "Polylines");
            ToolStripMenuItem toolStripMenuItem3 = Menu_AppendItem(menu, "Surfaces", Surfaces_Click, true, GetValue("TextOutputType", "Polylines") == "Surfaces");
            Menu_AppendSeparator(menu);
        }

        private void Curves_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Curves");
            SetValue("TextOutputType", "Curves");
            outputType = GetValue("TextOutputType", "Polylines");
            UpdateMessage();
            ExpireSolution(true);
        }

        private void Polylines_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Polylines");
            SetValue("TextOutputType", "Polylines");
            outputType = GetValue("TextOutputType", "Polylines");
            UpdateMessage();
            ExpireSolution(true);
        }

        private void Surfaces_Click(object sender, EventArgs e)
        {
            RecordUndoEvent("Surfaces");
            SetValue("TextOutputType", "Surfaces");
            outputType = GetValue("TextOutputType", "Polylines");
            UpdateMessage();
            ExpireSolution(true);
        }

        private void UpdateMessage()
        {
            Message = outputType;
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("TextOutputType", outputType);

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("TextOutputType", ref outputType);
            UpdateMessage();
            return base.Read(reader);
        }

        private void MapJustification(int just, out TextVerticalAlignment vertical, out TextHorizontalAlignment horizontal)
        {
            switch (just)
            {
                case 0:// "TopLeft":
                    vertical = TextVerticalAlignment.Top;
                    horizontal = TextHorizontalAlignment.Left;
                    break;
                case 1:// "TopCenter":
                    vertical = TextVerticalAlignment.Top;
                    horizontal = TextHorizontalAlignment.Center;
                    break;
                case 2:// "TopRight":
                    vertical = TextVerticalAlignment.Top;
                    horizontal = TextHorizontalAlignment.Right;
                    break;
                case 3:// "MiddleLeft":
                    vertical = TextVerticalAlignment.Middle;
                    horizontal = TextHorizontalAlignment.Left;
                    break;
                case 4:// "MiddleCenter":
                    vertical = TextVerticalAlignment.Middle;
                    horizontal = TextHorizontalAlignment.Center;
                    break;
                case 5:// "MiddleRight":
                    vertical = TextVerticalAlignment.Middle;
                    horizontal = TextHorizontalAlignment.Right;
                    break;
                case 6:// "BottomLeft":
                    vertical = TextVerticalAlignment.Bottom;
                    horizontal = TextHorizontalAlignment.Left;
                    break;
                case 7:// "BottomCenter":
                    vertical = TextVerticalAlignment.Bottom;
                    horizontal = TextHorizontalAlignment.Center;
                    break;
                case 8:// "BottomRight":
                    vertical = TextVerticalAlignment.Bottom;
                    horizontal = TextHorizontalAlignment.Right;
                    break;

                default:// "BottomLeft":
                    vertical = TextVerticalAlignment.Bottom;
                    horizontal = TextHorizontalAlignment.Left;
                    break;
            }
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.quinary; }
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
                return Resources.font_parser_2_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0013a919-d163-422d-a530-bee716528f2d"); }
        }
    }
}