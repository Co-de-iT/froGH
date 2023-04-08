using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

namespace froGH
{
    class GHFilePathInfo_Attributes : GH_ComponentAttributes
    {
        public GHFilePathInfo_Attributes(GHFilePathInfo owner): base(owner)
        { }

        protected override void Layout()
        {
            base.Layout();
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            bool flag = this.ContentBox.Contains(e.CanvasLocation);
            GH_ObjectResponse result;
            if (flag)
            {
                GHFilePathInfo component = (GHFilePathInfo)this.Owner;
                component.ExpireSolution(true);
                result = GH_ObjectResponse.Handled;// 3;

            }
            else
            {
                result = 0;
            }
            return result;
        }
    }
}
