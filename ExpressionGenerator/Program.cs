using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace ExpressionGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            static void writeAndWait(string str)
            {
                Console.WriteLine(str);
                Console.ReadLine();
            }

            var jsonStr = File.ReadAllText("rules.json");
            var jsonDocument = JsonDocument.Parse(jsonStr);
            
            var jsonExpressionParser = new JsonExpressionParser<ConditionalRule>(new QueryType<ConditionalRule>());
            var predicate = jsonExpressionParser
                .ParsePredicateOf<Transaction>(JsonDocument.Parse(jsonDocument.RootElement.GetProperty("conditions").ToString()));

            var functions = JsonDocument.Parse(jsonDocument.RootElement.GetProperty("functions").ToString());

            var selectJson= functions.RootElement.GetProperty("select");
            var gropuByJson= functions.RootElement.GetProperty("groupby");
            var expressionParser = new JsonExpressionParser<FunctionRule>(new QueryType<FunctionRule>());
            var transactionList = Transaction.GetList(1000);
            expressionParser.SetQueryType(selectJson);
            var select = expressionParser.ParsePredicate<Transaction>(JsonDocument.Parse(selectJson.ToString()));

            var sel = transactionList.Select(select).ToList();
            expressionParser.SetQueryType(gropuByJson);
           
            //var groupBy = sel.Select(x => x.ConvertToExpando()).GroupBy(x => ((IDictionary<string,object>)x)[expressionParser.Query_Type.Query.Type[0].Key]);
            //foreach(var gp in groupBy)
            //{
            //    Console.WriteLine($"KEY----"+ gp.Key);
            //    foreach(var item in gp.ToList())
            //    {
            //        Console.WriteLine(item);
            //    }
            //}
            var tSel = sel.FirstOrDefault().GetType();
            expressionParser.SetFuncType(tSel);
            var grp = expressionParser.ParsePredicateObject(JsonDocument.Parse(gropuByJson.ToString()));
            

            var filteredTransactions = transactionList.Where(predicate).ToList();
            
            writeAndWait($"Filtered to {filteredTransactions.Count} entities.");
            
            filteredTransactions.ForEach(Console.WriteLine);
        }
    }
}
