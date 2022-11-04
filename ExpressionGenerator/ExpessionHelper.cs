using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionGenerator
{
    public static class ExpessionHelper
    {
        public static ExpandoObject AddField(this ExpandoObject o, string fieldName, object value)
        {
            var result = o as IDictionary<string, object>;
            if (result.TryGetValue(fieldName, out object obj))
            {
                obj = value;
                result[fieldName] = obj;
            }
            else
            {
                result.Add(fieldName, value);
            }
            return result as ExpandoObject;
        }
        public static ExpandoObject ConvertToExpando(this object obj)
        {
            //Get Properties Using Reflections
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            FieldInfo[] properties = obj.GetType().GetFields(flags);

            //Add Them to a new Expando
            ExpandoObject expando = new ExpandoObject();
            foreach (FieldInfo property in properties)
            {
                expando.AddField(property.Name, property.GetValue(obj));
            }

            return expando;
        }
       
        public static IEnumerable<object> ToGroup(this List<object> collection, Func<object, object> expression)
        {
            return collection.GroupBy(expression);
        }
        public static IEnumerable<object> ToGroup<T>(List<T> collection,Func<T,object> expression)
        {
            return collection.GroupBy(expression);
        }
    }
}
