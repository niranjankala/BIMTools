using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XbimInvestigator.Business
{
    /// <summary>
    /// Represents a node in the model tree
    /// </summary>
    public class XTreeNode
    {
        /// <summary>
        /// The text to be displayed on the tree node
        /// </summary>
       [JsonProperty(PropertyName = "label")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "key")]
        public string GlobalID { get; set; }
        /// <summary>
        /// Child nodes
        /// </summary>
        [JsonProperty(PropertyName = "children")]
        public List<XTreeNode> ChildNodes { get; set; }

        public XTreeNode(string text)
        {
            Text = text;
            ChildNodes = new List<XTreeNode>();
        }

        public XTreeNode(string text, string globalId)
        {
            Text = text;
            GlobalID = globalId;
            ChildNodes = new List<XTreeNode>();
        }



        public override string ToString()
        {
            if (Text == null) return base.ToString();
            else return Text;
        }
    }
}
