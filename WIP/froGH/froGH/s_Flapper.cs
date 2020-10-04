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
using System.Threading.Tasks;
using System.Linq;
// </Custom using>

/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance2 : GH_ScriptInstance
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
    private void RunScript(List<Mesh> M, double A, double D, ref object Fi, ref object Fo, ref object CL)
    {
        // <Custom code>
         /*
         froGH - a sparse collection of (in)utilities for Grasshopper

         Flapper

         code by Alessio Erioli

         froGH by Co-de-iT is licensed under a Creative Commons Attribution 4.0 International License.
         http://creativecommons.org/licenses/by/4.0/

         */

        InitComp();

        if (M == null) return;

        double biAng = 0;
        Point3d current, previous, next;
        List<Point3d> flapPoints;
        Vector3d incoming, outgoing, zAxis, biSect, vPrev, vNext;
        int cW;

        DataTree<Polyline> Flaps = new DataTree<Polyline>();
        DataTree<GH_Line> FoldInnerEdges = new DataTree<GH_Line>();
        DataTree<GH_Line> FoldOuterEdges = new DataTree<GH_Line>();

        for (int k = 0; k < M.Count; k++)
        {

            FoldInnerEdges.AddRange(GetClothedEdges(M[k]).Select(l => new GH_Line(l)).ToArray(), new GH_Path(k));

            Polyline[] nakedEdges = M[k].GetNakedEdges();
            Polyline[] flapCurves = new Polyline[nakedEdges.Length];

            // verify that naked edges are closed polylines
            bool isClosed = true;
            foreach (Polyline nEdge in nakedEdges)
                isClosed &= nEdge.IsClosed;

            if (!isClosed) return;

            for (int i = 0; i < nakedEdges.Length; i++)
            {
                // add segments to outer edges Data Tree
                FoldOuterEdges.AddRange(nakedEdges[i].GetSegments().Select(s => new GH_Line(s)).ToArray(), new GH_Path(k));

                cW = IsClockWise(nakedEdges[i]) ? -1 : 1; // is polyline clockwise?

                flapPoints = new List<Point3d>();

                // remove duplicate point at start/endpoint
                nakedEdges[i].RemoveAt(nakedEdges[i].Count - 1);
                int nPoints = nakedEdges[i].Count;

                int prevInd, nextInd;

                Vector3d[,] OffsetVectors = new Vector3d[nPoints, 2];
                double[] OffsetAngles = new double[nPoints];

                // find bisector vectors and angles
                for (int j = 0; j < nPoints; j++)
                {
                    prevInd = (j - 1 + nPoints) % nPoints;
                    nextInd = (j + 1) % nPoints;

                    current = nakedEdges[i][j];
                    previous = nakedEdges[i][prevInd];
                    next = nakedEdges[i][nextInd];

                    incoming = previous - current;
                    outgoing = next - current;
                    double incomingLength = incoming.Length;
                    double outgoingLength = outgoing.Length;
                    incoming.Unitize();
                    outgoing.Unitize();
                    zAxis = Vector3d.CrossProduct(incoming, outgoing) * cW;

                    // check if this elbow is convex or concave (referring to the angle inside the polyline)
                    int convex = IsConvex(cW, current, incoming, outgoing, out biAng) ? cW : -cW;

                    // angle correction to get the exact bisector angle
                    biAng *= 0.5;
                    biSect = -(incoming + outgoing) * convex;// * cW;
                    biSect.Unitize();

                    // rotation & angle check
                    double angle = A < biAng ? A * convex * cW : biAng * convex * cW;

                    // vectors for incoming and outgoing directions from a vertex
                    vPrev = incoming;
                    vNext = outgoing;
                    double dist = D / Math.Abs(Math.Sin(angle));
                    //double distP = D / Math.Abs(Math.Sin(angle));

                    // length check
                    double maxLPrev, maxLNext;
                    maxLPrev = incomingLength * 0.5 / Math.Abs(Math.Cos(angle));
                    maxLNext = outgoingLength * 0.5 / Math.Abs(Math.Cos(angle));

                    vPrev *= Math.Min(dist, maxLPrev); //dist;
                    vNext *= Math.Min(dist, maxLNext); //dist;

                    vPrev.Rotate(-angle, zAxis);
                    vNext.Rotate(angle, zAxis);

                    OffsetVectors[j, 0] = vPrev;
                    OffsetVectors[j, 1] = vNext;
                    OffsetAngles[j] = angle;

                }

                double hMin, hsFactorStart, hsFactorEnd;
                Vector3d vStart, vEnd;
                Point3d fStart, fEnd;

                // verify offsets and generate flap points
                for (int j = 0; j < nPoints; j++)
                {

                    nextInd = (j + 1) % nPoints;
                    current = nakedEdges[i][j];
                    vStart = OffsetVectors[j, 1];
                    vEnd = OffsetVectors[nextInd, 0];

                    hsFactorStart = Math.Abs(Math.Sin(OffsetAngles[j]));
                    hsFactorEnd = Math.Abs(Math.Sin(OffsetAngles[nextInd]));

                    hMin = Math.Min(vStart.Length * hsFactorStart, vEnd.Length * hsFactorEnd);
                    vStart.Unitize();
                    vEnd.Unitize();
                    vStart *= hMin / hsFactorStart;
                    vEnd *= hMin / hsFactorEnd;

                    // generate flap points and add them to the array
                    fStart = current + vStart;
                    fEnd = nakedEdges[i][nextInd] + vEnd;
                    flapPoints.Add(current);
                    flapPoints.Add(fStart);
                    flapPoints.Add(fEnd);

                }

                // add again first point to get closed polyline
                flapPoints.Add(flapPoints[0]);
                flapCurves[i] = new Polyline(flapPoints);

            }
            Flaps.AddRange(flapCurves, new GH_Path(k));

        }

        Fi = FoldInnerEdges;
        Fo = FoldOuterEdges;
        CL = Flaps;

        // </Custom code>
    }

    // <Custom additional code> 

    void InitComp()
    {
        Component.Name = "Flapper";
        Component.NickName = "f_Flap";
        Component.Description = "builds flaps on a planar mesh naked edges\nsuggested use with mesh strips\nautomatic compensation for overlaps";
        Component.Params.Input[0].Name = "Mesh";
        Component.Params.Input[0].Description = "Input Mesh Strip";
        Component.Params.Input[1].Name = "Angle";
        Component.Params.Input[1].Description = "Flap tapering angle (0 - max tapering, 0.5 * Pi - perpendicular to edge)";
        Component.Params.Input[2].Name = "Distance";
        Component.Params.Input[2].Description = "Flap Offset Distance";
        Component.Params.Output[0].Name = "Fold inner edges";
        Component.Params.Output[0].Description = "Fold lines for inner (clothed) edges";
        Component.Params.Output[1].Name = "Fold outer edges";
        Component.Params.Output[1].Description = "Fold lines for outer (naked) edges";
        Component.Params.Output[2].Name = "Cut lines";
        Component.Params.Output[2].Description = "Cut lines for flaps";
    }


    /// <summary>
    /// Is polyline clockwise - determines if a polyline is clockwise or not
    /// </summary>
    /// <param name="poly">the Polyline to inspect</param>
    /// <returns>true for cw, false for ccw</returns>
    /// <remarks>code from https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order \nImplemented in VBNet by Mateusz Zwierzycki</remarks>
    public bool IsClockWise(Polyline poly)
    {
        double sum = 0;
        foreach (Line s in poly.GetSegments())
            sum += (s.To.X - s.From.X) * (s.To.Y + s.From.Y);

        return sum > 0;
    }

    public bool IsConvex(int cW, Point3d elbow, Vector3d previousEdge, Vector3d nextEdge, out double angle)
    {
        // this assumes nextEdge and previousEdge are vectors pointing out of a vertex and to the connected ones
        Plane p = new Plane(elbow, previousEdge, nextEdge);
        Point3d x, y;
        p.RemapToPlaneSpace((Point3d)elbow + previousEdge, out x);
        p.RemapToPlaneSpace((Point3d)elbow + nextEdge, out y);

        double convex = ((Math.Atan2(nextEdge.X, nextEdge.Y) - Math.Atan2(previousEdge.X, previousEdge.Y) + Math.PI * 2) % (Math.PI * 2)) - Math.PI;
        //angle = AngSign((Vector3d)x, (Vector3d)y);// + Math.PI*Convert.ToInt32(convex <0);

        angle = convex * cW < 0 ? 2 * Math.PI - AngSign((Vector3d)x, (Vector3d)y) : AngSign((Vector3d)x, (Vector3d)y);

        return convex < 0;
        //    if (angle < 0) {
        //      corner.type = 'convex';
        //    } else if (angle > 0) {
        //      corner.type = 'concave';
        //    } else {
        //      corner.type = 'straight';
        //    }
    }

    double AngSign(Vector3d v1, Vector3d v2)
    {
        v1.Unitize();
        v2.Unitize();
        return (Math.Atan2(v2.Y, v2.X) - Math.Atan2(v1.Y, v1.X));
    }

    List<Line> GetClothedEdges(Mesh M)
    {
        List<Line> cEdges = new List<Line>();
        for (int i = 0; i < M.TopologyEdges.Count; i++)
        {
            if (M.TopologyEdges.GetConnectedFaces(i).Length > 1)
                cEdges.Add(M.TopologyEdges.EdgeLine(i));
        }
        return cEdges;
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
        List<Mesh> M = null;
        if (inputs[0] != null)
        {
            M = GH_DirtyCaster.CastToList<Mesh>(inputs[0]);
        }
        double A = default(double);
        if (inputs[1] != null)
        {
            A = (double)(inputs[1]);
        }

        double D = default(double);
        if (inputs[2] != null)
        {
            D = (double)(inputs[2]);
        }



        //3. Declare output parameters
        object bV = null;
        object bA = null;
        object Fc = null;


        //4. Invoke RunScript
        RunScript(M, A, D, ref bV, ref bA, ref Fc);

        try
        {
            //5. Assign output parameters to component...
            if (bV != null)
            {
                if (GH_Format.TreatAsCollection(bV))
                {
                    IEnumerable __enum_bV = (IEnumerable)(bV);
                    DA.SetDataList(1, __enum_bV);
                }
                else
                {
                    if (bV is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(bV));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(1, bV);
                    }
                }
            }
            else
            {
                DA.SetData(1, null);
            }
            if (bA != null)
            {
                if (GH_Format.TreatAsCollection(bA))
                {
                    IEnumerable __enum_bA = (IEnumerable)(bA);
                    DA.SetDataList(2, __enum_bA);
                }
                else
                {
                    if (bA is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(bA));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(2, bA);
                    }
                }
            }
            else
            {
                DA.SetData(2, null);
            }
            if (Fc != null)
            {
                if (GH_Format.TreatAsCollection(Fc))
                {
                    IEnumerable __enum_Fc = (IEnumerable)(Fc);
                    DA.SetDataList(3, __enum_Fc);
                }
                else
                {
                    if (Fc is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(Fc));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(3, Fc);
                    }
                }
            }
            else
            {
                DA.SetData(3, null);
            }

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