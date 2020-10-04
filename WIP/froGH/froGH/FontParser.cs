using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;


namespace froGH
{
    public class FontParser : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FontParser class.
        /// </summary>
        public FontParser()
          : base("Font Parser", "f_FPars",
              "Converts text into polylines - useful for fabrication \nuse single-line fonts for best results",
              "froGH", "Fabrication")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text string", "T", "Text to parse into polylines", GH_ParamAccess.item);
            pManager.AddTextParameter("Font face", "F", "Font face\nUse Font List for a list of available fonts", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Closed", "C", "Closed Text Polylines True/False (default)", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Size", "S", "Text size (in Rhino units)", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Sampling Precision", "sp", "Sampling Precision\nUsually a value between 100-250 is ok", GH_ParamAccess.item, 250);
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
            pManager.AddCurveParameter("Curves", "C", "Text as Polylines", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string S = "";
            DA.GetData(0, ref S);
            string font = "";
            DA.GetData(1, ref font);
            bool close = false;
            DA.GetData(2, ref close);
            double size = 0.0;
            DA.GetData(3, ref size);
            double precision = 0.0;
            DA.GetData(4, ref precision);
            Plane basePlane = new Plane();
            DA.GetData(5, ref basePlane);
            int J = 0;
            DA.GetData(6, ref J);

            float fS = size == 0 ? 1 : (float)size;
            Font local_font = new Font(font, (float)fS);

            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddString(S, local_font.FontFamily, (int)local_font.Style, local_font.Size, new PointF(0, 0), new StringFormat());

            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix(); // transformation matrix
            matrix.Reset(); // build identity matrix

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

                    var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("top left", "0");
                    var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("middle left", "1");
                    var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("bottom left", "2");
                    var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("top center", "3");
                    var item5 = new Grasshopper.Kernel.Special.GH_ValueListItem("middle center", "4");
                    var item6 = new Grasshopper.Kernel.Special.GH_ValueListItem("bottom center", "5");
                    var item7 = new Grasshopper.Kernel.Special.GH_ValueListItem("top right", "6");
                    var item8 = new Grasshopper.Kernel.Special.GH_ValueListItem("middle right", "7");
                    var item9 = new Grasshopper.Kernel.Special.GH_ValueListItem("bottom right", "8");
                    vList.ListItems.Add(item1);
                    vList.ListItems.Add(item2);
                    vList.ListItems.Add(item3);
                    vList.ListItems.Add(item4);
                    vList.ListItems.Add(item5);
                    vList.ListItems.Add(item6);
                    vList.ListItems.Add(item7);
                    vList.ListItems.Add(item8);
                    vList.ListItems.Add(item9);

                    vList.ListItems[0].Value.CastTo(out J);
                }
            }
            catch
            {
                // handles anything that is not a value list
            }

            // ______________ text justification ______________

            RectangleF rec = path.GetBounds(); // bounding rectangle for text
            float dX = Convert.ToSingle(rec.Width * -0.5), dY = Convert.ToSingle(rec.Height * 0.5);
            switch (J)
            {
                case 0:// top left
                    dX = 0;
                    dY = fS;
                    break;
                case 1: // middle left
                    dX = 0;
                    dY = Convert.ToSingle((-rec.Height) * 0.5 + fS);
                    break;
                case 2:  // bottom left
                    dX = 0;
                    dY = Convert.ToSingle(-rec.Height + fS * 0.5);
                    break;
                case 3: // top center
                    dX = Convert.ToSingle(rec.Width * -0.5);
                    dY = fS;
                    break;
                case 4: // middle center
                    dX = Convert.ToSingle(rec.Width * -0.5);
                    dY = Convert.ToSingle((-rec.Height) * 0.5 + fS);
                    break;
                case 5: // bottom center
                    dX = Convert.ToSingle(rec.Width * -0.5);
                    dY = Convert.ToSingle(-rec.Height + fS * 0.5);
                    break;
                case 6: // top right
                    dX = Convert.ToSingle(rec.Width * -1);
                    dY = fS;
                    break;
                case 7: // middle right
                    dX = Convert.ToSingle(rec.Width * -1);
                    dY = Convert.ToSingle((-rec.Height) * 0.5 + fS);
                    break;
                case 8: // bottom right
                    dX = Convert.ToSingle(rec.Width * -1);
                    dY = Convert.ToSingle(-rec.Height + fS * 0.5);
                    break;

            }
            //float dX = Convert.ToSingle(rec.Width * -0.5);
            //float dY = Convert.ToSingle(rec.Height * 0.5);
            System.Drawing.Drawing2D.Matrix mTrans = new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, dX, dY); // build transformation matrix
            path.Transform(mTrans); // transform text path

            // ______________ convert to polylines ______________

            path.Flatten(matrix, (float)(size / precision)); // turns the path into a polyline that approximates the path

            PointF[] pts = path.PathPoints; // get path points
            Byte[] tps = path.PathTypes;   // get path point types
            List<Polyline> strokes = new List<Polyline>(); // List for strokes
            Polyline stroke = new Polyline();

            Byte typ_start = Convert.ToByte(System.Drawing.Drawing2D.PathPointType.Start);// find start points condition

            // the conversion loop
            for (int i = 0; i < pts.Length; i++)
            {
                // if a start point is found, and the existing polyline is not null nor a single point,
                // add polyline to the strokes and create a new polyline
                if (tps[i] == typ_start)
                {
                    if (stroke != null && stroke.Count > 1)
                    {
                        if (close) stroke.Add(stroke[0]); // close polyline if necessary
                        strokes.Add(stroke);
                    }
                    stroke = new Polyline();
                }
                // in any other case add the next point to a polyline
                stroke.Add(pts[i].X, -pts[i].Y + size, 0);
                // add last stroke to the list
                if (i == pts.Length - 1)
                {
                    if (close) stroke.Add(stroke[0]); // and close it if necessary
                    strokes.Add(stroke);
                }
            }


            // ______________ align strokes to given plane ______________

            Transform align = Transform.PlaneToPlane(Plane.WorldXY, basePlane); // align transformation
            for (int j = 0; j < strokes.Count; j++)
            {
                strokes[j].Transform(align);
            }

            DA.SetDataList(0, strokes);
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
            get { return new Guid("1bb9960e-8051-4303-ac32-9a7ad69cbf37"); }
        }
    }
}