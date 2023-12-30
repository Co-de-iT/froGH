using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace froGH.Utils
{
    public class GH_froGHRTree : GH_Goo<froGHRTree>
    {
        public override bool IsValid => Value.Points != null;

        public override string TypeName => "froGHRTree";

        public override string TypeDescription => "An RTree class to pass around";

        public GH_froGHRTree()
        { }

        public GH_froGHRTree(froGHRTree froGHTree)
        {
            Value = froGHTree;
        }

        public override IGH_Goo Duplicate()
        {
            return GH_Convert.ToGoo(Value);
        }

        public override string ToString()
        {
            return Value.Source.ToString() + " RTree containing " + Value.Points.Count.ToString() + " points";
        }
    }
}
