using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using froGH.Properties;

namespace froGH
{
    public class SlidersDisplay : GH_Component
    {
        BoundingBox _clip;
        Point3d from;
        Vector3d yOffset;
        Plane basePlane;
        Color idle, active;

        Slider slid;

        List<Slider> sliders;

        /// <summary>
        /// Initializes a new instance of the SliderInterface class.
        /// </summary>
        public SlidersDisplay()
          : base("Slider Value display", "f_SlVDisp",
              "Display Slider values on screen",
              "froGH", "View/Display")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Sliders", "Sl", "Plug Sliders directly here, one by one, in order\ndo not use intermediate components such as merge", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Orientation Plane", "P", "Orientation Plane\nPlane origin is the Top-Left corner", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Slider Length", "l", "Length of Slider Display Line", GH_ParamAccess.item, 10.0);
            pManager.AddTextParameter("Text Font", "F", "Text Font\nleave empty for default interface Font", GH_ParamAccess.item);
            pManager.AddNumberParameter("Text Size", "S", "Text size", GH_ParamAccess.item, 1.0);
            pManager.AddColourParameter("Base Color", "bC", "Base color", GH_ParamAccess.item, Color.DarkGray);
            pManager.AddColourParameter("Highlight Color", "hC", "Highlight color", GH_ParamAccess.item, Color.Black);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
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


            Plane P = Plane.WorldXY;
            DA.GetData(1, ref P);
            if (P == null) P = Plane.WorldXY;
            from = P.Origin;
            double len = 0;
            DA.GetData(2, ref len);
            string fontFace = "";
            DA.GetData(3, ref fontFace);
            double th = 0;
            DA.GetData(4, ref th);
            idle = Color.DarkGray;
            active = Color.Black;
            DA.GetData(5, ref idle);
            DA.GetData(6, ref active);

            basePlane = new Plane(P);
            yOffset = basePlane.YAxis * (-th * 6);

            sliders = new List<Slider>();
            Grasshopper.Kernel.Special.GH_NumberSlider slider;

            // input through merge (test)
            // see James Ramsden gh tips on how to place a new component (Merge) and control it 
            // Merge is part of the Math components
            //Grasshopper.Kernel.Components.GH_MergeGroupComponent merge = new Grasshopper.Kernel.Components.GH_MergeGroupComponent();
            //merge.CreateAttributes();

            //for (int i = 0; i < merge.Params.Input.Count; i++)
            //{

            //    try
            //    {
            //        slider = merge.Params.Input[i].Sources[0] as Grasshopper.Kernel.Special.GH_NumberSlider;
            //    }
            //    catch
            //    {
            //        continue;
            //    }
            //    basePlane.Origin = from;
            //    slid = new Slider(slider, basePlane, fontFace, len, th, slider.NickName);
            //    sliders.Add(slid);
            //    _clip.Union(slid.GetBoundingBox());
            //    from += yOffset;
            //}

            // for direct slider input
            for (int i = 0; i < Params.Input[0].Sources.Count; i++)
            {
                slider = Params.Input[0].Sources[i] as Grasshopper.Kernel.Special.GH_NumberSlider;
                basePlane.Origin = from;
                slid = new Slider(slider, basePlane, fontFace, len, th, slider.NickName);
                sliders.Add(slid);
                _clip.Union(slid.GetBoundingBox());
                from += yOffset;
            }
        }

        class Slider
        {

            public Plane plane;
            public double vMin, vMax, val, normVal;
            public Line range;
            public Line sValue;
            public string name;
            public string fontFace;
            public Rhino.Display.Text3d label;
            public Rhino.Display.Text3d tMin;
            public Rhino.Display.Text3d tMax;
            public Rhino.Display.Text3d tVal;
            double textHeight;

            public Slider(Grasshopper.Kernel.Special.GH_NumberSlider slider, Plane p, string fontFace, double length, double textHeight, string name)
            {
                plane = p;
                vMin = (double)slider.Slider.Minimum;
                vMax = (double)slider.Slider.Maximum;
                val = (double)slider.CurrentValue;
                normVal = (val - vMin) / (vMax - vMin);

                range = new Line(p.Origin, p.XAxis, length);
                sValue = new Line(p.Origin, p.XAxis, length * normVal);

                this.name = name;
                this.textHeight = textHeight;
                this.fontFace = fontFace;

                label = new Rhino.Display.Text3d(name);
                label.Height = textHeight;

                Plane labPlane = new Plane(plane);
                labPlane.Translate(p.YAxis * -(textHeight * 1.8));
                label.TextPlane = labPlane;

                tMin = new Rhino.Display.Text3d(Convert.ToString(vMin));
                tMax = new Rhino.Display.Text3d(Convert.ToString(vMax));
                tVal = new Rhino.Display.Text3d(Convert.ToString(val));

                if (this.fontFace != "")
                {
                    tMin.FontFace = this.fontFace;
                    tMax.FontFace = this.fontFace;
                    tVal.FontFace = this.fontFace;
                    label.FontFace = this.fontFace;
                }

                tMin.Height = textHeight;
                tMax.Height = textHeight;
                tVal.Height = textHeight * 1.2;
                Plane tPlane = new Plane(plane);
                tPlane.Translate(p.XAxis * -(textHeight * 0.8));
                tMin.TextPlane = tPlane;
                tPlane.Origin = p.Origin + p.XAxis * length;
                tPlane.Translate(p.XAxis * (textHeight * 0.8));
                tMax.TextPlane = tPlane;
                tPlane.Origin = p.Origin + p.XAxis * length * normVal;
                tPlane.Translate(p.YAxis * textHeight);
                tVal.TextPlane = tPlane;
                tMin.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Right;
                tMax.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
                tVal.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                tVal.Bold = true;
            }

            public BoundingBox GetBoundingBox()
            {
                BoundingBox box = BoundingBox.Empty;
                box.Union(tMin.BoundingBox);
                box.Union(tMax.BoundingBox);
                box.Union(tVal.BoundingBox);
                box.Union(label.BoundingBox);
                return box;
            }
        }

        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        //Draw all wires and points in this method.
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            foreach (Slider slid in sliders)
            {
                args.Display.DrawLine(slid.range, idle, 1);
                args.Display.DrawLine(slid.sValue, active, 5);
                args.Display.Draw3dText(slid.label, active);
                args.Display.Draw3dText(slid.tMin, active);
                args.Display.Draw3dText(slid.tMax, active);
                args.Display.Draw3dText(slid.tVal, active);
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
                return Resources.Sliders_Display_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8c8224b8-085d-4d76-bb2e-a9992647c248"); }
        }
    }
}