using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionGenerator
{

    public class Rootobject
    {
        public Function[] functions { get; set; }
    }

    public class Function
    {
        public string Condition { get; set; }
        public Rule[] Rules { get; set; }
    }

    

}
