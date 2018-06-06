using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStudioMeasuresViewer
{
    public class taxonomy
    {
        public Term[] term { get; set; }
    }

    public class Term
    {
        public string tid { get; set; }
        public string vid { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string format { get; set; }
        public string weight { get; set; }
        public string uuid { get; set; }
        public int depth { get; set; }
        public string[] parents { get; set; }
        public Term[] term { get; set; }
    }
    

}
