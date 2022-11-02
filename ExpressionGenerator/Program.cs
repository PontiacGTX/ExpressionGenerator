using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
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
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var groupBy = jsonExpressionParser.ParsePredicate<Transaction>(JsonDocument.Parse(item.ToString()));
                var r =  transactionList.Select(groupBy).ToList();
            }

            
            var filteredTransactions = transactionList.Where(predicate).ToList();
            
            writeAndWait($"Filtered to {filteredTransactions.Count} entities.");
            
            filteredTransactions.ForEach(Console.WriteLine);
        }
    }
}
