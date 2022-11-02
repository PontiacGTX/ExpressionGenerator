using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExpressionGenerator
{
    public static class JsonElementHelper
    {
        public static List<T> GetEnumeratorList<T>(this JsonElement element)
        {
            List<T> lista = new List<T>();

            for(int i=0;i< element.GetArrayLength();i++)
            {
                lista.Add((T)Convert.ChangeType(element[i].GetString(), typeof(T)));
            }

            return lista;

        }

        public static T DeserializeObject<T>(this JsonElement el, JsonConverter converter = null)
        {
            var text = el.GetRawText();
            if(converter!=null)
            {
               return JsonConvert.DeserializeObject<T>(text,converter);

            }
            var result = JsonConvert.DeserializeObject<T>(text);
            return result;
        }
    }
}
