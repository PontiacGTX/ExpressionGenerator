using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionGenerator
{
    public class QueryType<T> where T:Rule
    {
        public Rule Query { get; set; }

        public void SetQuery(object o)
        {

            Console.WriteLine(typeof(T));
            var type = o.GetType();
            if (type!= typeof(T) & type!=typeof(FunctionRule) & type!=typeof(ConditionalRule))
            {
                throw new ArgumentException();
            }
            Query = (Rule)o;
        }

        public bool IsInnerType(Type t) => typeof(T) == t;
    }
}
