using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace froGH
{
    public class SlidersDisplay : GH_Component, IGH_VariableParameterComponent
    {
        BoundingBox _clip;
        Point3d from;
        Vector3d yOffset;
        Plane basePlane;
        Color idle, active;
        int slidersStartIndex;
        double yOffsetMultiplier;

        List<DisplaySlider> displaySliders;

        /// <summary>
        /// Initializes a new instance of the SliderInterface class.
        /// </summary>
        public SlidersDisplay()
          : base("Slider Value display", "f_SlVDisp",
              "Display Slider values on screen",
              "froGH", "View/Display")
        {
            slidersStartIndex = 6;
            yOffsetMultiplier = 5.8;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Orientation Plane", "P", "Orientation Plane\nPlane origin is the Top-Left corner", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Slider Length", "L", "Length of Slider Display Line", GH_ParamAccess.item, 10.0);
            pManager.AddTextParameter("Text Font", "F", "Text Font\nleave empty for default interface Font", GH_ParamAccess.item);
            pManager.AddNumberParameter("Text Size", "S", "Text size", GH_ParamAccess.item, 1.0);
            pManager.AddColourParameter("Base Color", "bC", "Base color", GH_ParamAccess.item, Color.DarkGray);
            pManager.AddColourParameter("Highlight Color", "hC", "Highlight color", GH_ParamAccess.item, Color.Black);
            pManager.AddGenericParameter("Slider", "S0", "Plug a Slider component\nUse the zoomable interface to add inputs\nEmpty inputs are used as spacers", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].WireDisplay = GH_ParamWireDisplay.hidden;
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
            DA.GetData(0, ref P);
            if (P == null) P = Plane.WorldXY;
            from = P.Origin;
            double sliderLength = 0;
            DA.GetData(1, ref sliderLength);
            string fontFace = "";
            DA.GetData(2, ref fontFace);
            double textHeight = 0;
            DA.GetData(3, ref textHeight);
            idle = Color.DarkGray;
            active = Color.Black;
            DA.GetData(4, ref idle);
            DA.GetData(5, ref active);

            basePlane = new Plane(P);
            yOffset = basePlane.YAxis * (-textHeight * yOffsetMultiplier);

            displaySliders = new List<DisplaySlider>();
            Grasshopper.Kernel.Special.GH_NumberSlider slider;


            DisplaySlider dSlider;

            for (int i = slidersStartIndex; i < Params.Input.Count; i++)
            {
                try
                {
                    slider = Params.Input[i].Sources[0] as Grasshopper.Kernel.Special.GH_NumberSlider;
                }
                catch
                {
                    from += yOffset;
                    continue;
                }
                basePlane.Origin = from;
                dSlider = new DisplaySlider(slider, basePlane, fontFace, sliderLength, textHeight, slider.NickName);
                displaySliders.Add(dSlider);
                _clip.Union(dSlider.GetBoundingBox());
                from += yOffset;
            }
        }

        private class DisplaySlider
        {

            internal Plane plane;
            internal double vMin, vMax, val, normalizedValue;
            internal Line range;
            internal Line sValue;
            internal string name;
            internal string fontFace;
            internal Rhino.Display.Text3d label;
            internal Rhino.Display.Text3d tMin;
            internal Rhino.Display.Text3d tMax;
            internal Rhino.Display.Text3d tVal;
            private const double labelMultiplier = 1.8;
            private const double textValMultiplier = 1.2;
            private const double textExMultiplier = 0.8;

            internal DisplaySlider(Grasshopper.Kernel.Special.GH_NumberSlider slider, Plane p, string fontFace, double length, double textHeight, string name)
            {
                plane = p;
                vMin = (double)slider.Slider.Minimum;
                vMax = (double)slider.Slider.Maximum;
                val = (double)slider.CurrentValue;
                normalizedValue = (val - vMin) / (vMax - vMin);

                range = new Line(p.Origin, p.XAxis, length);
                sValue = new Line(p.Origin, p.XAxis, length * normalizedValue);

                this.name = name;
                this.fontFace = fontFace;

                label = new Rhino.Display.Text3d(name);
                label.Height = textHeight;

                Plane labPlane = new Plane(plane);
                labPlane.Translate(p.YAxis * -(textHeight * labelMultiplier));
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
                tVal.Height = textHeight * textValMultiplier;
                Plane tPlane = new Plane(plane);
                tPlane.Translate(p.XAxis * -(textHeight * textExMultiplier));
                tMin.TextPlane = tPlane;
                tPlane.Origin = p.Origin + p.XAxis * length;
                tPlane.Translate(p.XAxis * (textHeight * textExMultiplier));
                tMax.TextPlane = tPlane;
                tPlane.Origin = p.Origin + p.XAxis * length * normalizedValue;
                tPlane.Translate(p.YAxis * textHeight);
                tVal.TextPlane = tPlane;
                tMin.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Right;
                tMax.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
                tMin.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Top;
                tMax.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Top;
                tVal.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Center;
                tVal.Bold = true;
            }

            internal BoundingBox GetBoundingBox()
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
            displaySliders = new List<DisplaySlider> ();
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        //Draw all wires and points in this method.
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (displaySliders == null) return;
            foreach (DisplaySlider dSlider in displaySliders)
            {
                if (dSlider == null) continue;
                args.Display.DrawLine(dSlider.range, idle, 1);
                args.Display.DrawLine(dSlider.sValue, active, 5);
                args.Display.Draw3dText(dSlider.label, active);
                args.Display.Draw3dText(dSlider.tMin, active);
                args.Display.Draw3dText(dSlider.tMax, active);
                args.Display.Draw3dText(dSlider.tVal, active);
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return (side != GH_ParameterSide.Output) && index > 5;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return (side == GH_ParameterSide.Input) && (index > 6);
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Param_GenericObject param = new Param_GenericObject();

            param.Name = $"S{Params.Input.Count - 6}";
            param.NickName = param.Name;
            param.Description = "Slider Input";
            param.Optional = true;
            param.WireDisplay = GH_ParamWireDisplay.hidden;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            for (int i = 6; i < Params.Input.Count; i++)
            {
                Params.Input[i].Name = $"S{i - 6}";
                Params.Input[i].NickName = Params.Input[i].Name;
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
                return Resources.SlidersDisplay_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("57ACDFF0-3B06-43CD-BCBD-E3EB75647885"); }
        }
    }
}