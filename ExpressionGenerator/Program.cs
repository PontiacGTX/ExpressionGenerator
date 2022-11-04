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
            var tSel = sel.FirstOrDefault().GetType();
            expressionParser.SetQueryType(gropuByJson);
            expressionParser.SetFuncType(tSel);
            var groupBy = expressionParser.ParsePredicateObject(JsonDocument.Parse(gropuByJson.ToString()));
            var groupBySel = sel.GroupBy(groupBy); 
            //foreach (var item in functions.RootElement.EnumerateArray())
            //{
            //    var it = item;
            //    jsonExpressionParser.SetQueryType(it);
            //    var lambda = jsonExpressionParser.ParsePredicate<Transaction>(JsonDocument.Parse(it.ToString()));
            //    var t = jsonExpressionParser.Query_Type;

            //    if(t.Query.Type.FirstOrDefault().Operator=="groupby")
            //    {
            //        // var res =   transactionList.Select(x=>x).GroupBy(groupBy).ToList();
            //        //foreach(var re in res.ToArray())
            //        //{
            //        //    Console.WriteLine($"KEY----------{re.Key}");
            //        //    foreach (var registro in re.ToArray())
            //        //    {
            //        //        Console.WriteLine(registro);
            //        //    }
            //        //}
            //    }
            //    //i++;
            //}


            var filteredTransactions = transactionList.Where(predicate).ToList();
            
            writeAndWait($"Filtered to {filteredTransactions.Count} entities.");
            
            filteredTransactions.ForEach(Console.WriteLine);
        }
    }
}
