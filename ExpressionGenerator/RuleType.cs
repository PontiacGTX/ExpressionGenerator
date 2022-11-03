using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionGenerator
{
    public class RuleType
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("field")]
        public string Field { get; set; }
        [JsonProperty("operator")]
        public string Operator { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public object Value { get; set; }
        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
