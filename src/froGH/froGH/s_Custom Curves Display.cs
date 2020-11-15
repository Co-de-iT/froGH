using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// <Custom using>
using System.Drawing;
using Grasshopper.Kernel.Components;
// </Custom using>

/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance3 : GH_ScriptInstance
{
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { __out.Add(text); }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments, 
    /// Output parameters as ref arguments. You don't have to assign output parameters, 
    /// they will have a default value.
    /// </summary>
    private void RunScript(Curve C, Color col, int W)
    {

        // <Custom code>

        /*
      froGH - a sparse collection of (in)utilities for Grasshopper

      Custom Curve Display

      code by Alessio Erioli

      froGH by Co-de-iT is licensed under a Creative Commons Attribution 4.0 International License.
      http://creativecommons.org/licenses/by/4.0/

      */

        initComp();


        //GH_CustomPreviewItem item = default(GH_CustomPreviewItem);

        //item.Geometry = (IGH_PreviewData)C;

            //if (cur is IGH_PreviewData) Print("ok");
            _clip = BoundingBox.Union(_clip, C.GetBoundingBox(false));
            _curve.Add(C);
            _color.Add(col);
            _width.Add(W);


        
        //item.Geometry = 

        // </Custom code>
    }

    // <Custom additional code> 
    private BoundingBox _clip;
    private List<Curve> _curve = new List<Curve>();
    private List<Color> _color = new List<Color>();
    private List<int> _width = new List<int>();

    void initComp()
    {
        Component.Name = "Custom Curve Display";
        Component.NickName = "f_CCD";
        Component.Description = "Render-compatible custom curve display";
        Component.Params.Input[0].Name = "Curves";
        Component.Params.Input[0].Description = "Curves to display";
        Component.Params.Input[1].Name = "Colour";
        Component.Params.Input[1].Description = "Display colour";
        Component.Params.Input[2].Name = "Width";
        Component.Params.Input[2].Description = "Display width";
    }


    /// <summary>
    /// This method will be called once every solution, before any calls to RunScript.
    /// </summary>
    public override void BeforeRunScript()
    {
        _clip = BoundingBox.Empty;
        _curve.Clear();
        _color.Clear();
        _width.Clear();
    }
    /// <summary>
    /// This method will be called once every solution, after any calls to RunScript.
    /// </summary>
    public override void AfterRunScript()
    { }


    //Return a BoundingBox that contains all the geometry you are about to draw.
    public override BoundingBox ClippingBox
    {
        get { return _clip; }
    }

    //Draw all meshes in this method.
    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
    }

    //Draw all wires and points in this method.
    public override void DrawViewportWires(IGH_PreviewArgs args)
    {
        for (int i = 0; i < _curve.Count; i++)
            args.Display.DrawCurve(_curve[i], _color[i], _width[i]);
    }

    // </Custom additional code> 

    private List<string> __err = new List<string>(); //Do not modify this list directly.
    private List<string> __out = new List<string>(); //Do not modify this list directly.
    private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
    private IGH_ActiveObject owner;                  //Legacy field.
    private int runCount;                            //Legacy field.

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        //Prepare for a new run...
        //1. Reset lists
        this.__out.Clear();
        this.__err.Clear();

        this.Component = owner;
        this.Iteration = iteration;
        this.GrasshopperDocument = owner.OnPingDocument();
        this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

        this.owner = this.Component;
        this.runCount = this.Iteration;
        this.doc = this.RhinoDocument;

        //2. Assign input parameters
        List<Curve> C = null;
        if (inputs[0] != null)
        {
            C = GH_DirtyCaster.CastToList<Curve>(inputs[0]);
        }
        Color col = default(Color);
        if (inputs[1] != null)
        {
            col = (Color)(inputs[1]);
        }

        int W = default(int);
        if (inputs[2] != null)
        {
            W = (int)(inputs[2]);
        }



        //3. Declare output parameters


        //4. Invoke RunScript
        RunScript(C, col, W);

        try
        {
            //5. Assign output parameters to component...

        }
        catch (Exception ex)
        {
            this.__err.Add(string.Format("Script exception: {0}", ex.Message));
        }
        finally
        {
            //Add errors and messages... 
            if (owner.Params.Output.Count > 0)
            {
                if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
                {
                    List<string> __errors_plus_messages = new List<string>();
                    if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
                    if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
                    if (__errors_plus_messages.Count > 0)
                        DA.SetDataList(0, __errors_plus_messages);
                }
            }
        }
    }
}