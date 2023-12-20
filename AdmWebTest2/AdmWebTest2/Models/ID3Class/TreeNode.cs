using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admtest5.Id3Classes
{
    class TreeNode
    {
        public Attribut node_attribute { get; set; }
        public string condition { get; set; }
        public List<TreeNode> childern { get; set; }
        public Dictionary<Attribut, bool> is_parent { get; set; }
        public TreeNode(Attribut node_attribute, string condition, Dictionary<Attribut, bool> is_parent)
        {
            this.node_attribute = node_attribute;
            this.condition = condition;
            this.childern = new List<TreeNode>();
            this.is_parent = is_parent;
        }
    }
}
