using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionGenerator
{
    public class Rule
    {
        [JsonProperty("condition")]
        public string Condition { get; set; }
        [JsonProperty("rules")]
        public  RuleType[] Type { get; set; }
    }
}
