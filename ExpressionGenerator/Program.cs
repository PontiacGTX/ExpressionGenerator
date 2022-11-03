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

            var doc = JsonDocument.Parse(jsonDocument.RootElement.GetProperty("functions").ToString());

            var transactionList = Transaction.GetList(1000);
            Console.WriteLine($"{doc.RootElement.GetArrayLength()} {jsonDocument.RootElement.GetProperty("functions").ToString()}");
            
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                jsonExpressionParser.SetQueryType(item);
                var lambda = jsonExpressionParser.ParsePredicate<Transaction>(JsonDocument.Parse(item.ToString()));
                var t = jsonExpressionParser.@Type;
              
                if(t.Query.Type.FirstOrDefault().Operator=="groupby")
                {
                    // var res =   transactionList.Select(x=>x).GroupBy(groupBy).ToList();
                    //foreach(var re in res.ToArray())
                    //{
                    //    Console.WriteLine($"KEY----------{re.Key}");
                    //    foreach (var registro in re.ToArray())
                    //    {
                    //        Console.WriteLine(registro);
                    //    }
                    //}
                }
                
            }

                if(lambdas.TryGetValue("select",out var lam) && lambdas.TryGetValue("groupby",out var lamGroupBy))
                {
                        try
                        {
                            var r = transactionList.Select(lam).GroupBy(lamGroupBy);
                            Console.WriteLine(r.Count());
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }
                }
            Console.WriteLine(lambdas.Count());
            var filteredTransactions = transactionList.Where(predicate).ToList();
            
            writeAndWait($"Filtered to {filteredTransactions.Count} entities.");
            
            filteredTransactions.ForEach(Console.WriteLine);
        }
    }
}
