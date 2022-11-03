using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace ExpressionGenerator
{
    public class JsonExpressionParser<T> where T: Rule
    {
        public QueryType<T> _Type { get; }
        public JsonExpressionParser(QueryType<T> type)
        {
            _Type = type;
        }
        private const string StringStr = "string";

        private readonly string BooleanStr = nameof(Boolean).ToLower();
        private readonly string Number = nameof(Number).ToLower();
        private readonly string In = nameof(In).ToLower();
        private readonly string GroupBy = nameof(GroupBy).ToLower();
        private readonly string And = nameof(And).ToLower();
        private readonly string Or = nameof(Or).ToLower();
        private readonly string Equal = nameof(Equal).ToLower();
        private readonly string Select = nameof(Select).ToLower();

        private readonly MethodInfo MethodContains = typeof(Enumerable).GetMethods(
                        BindingFlags.Static | BindingFlags.Public)
                        .Single(m => m.Name == nameof(Enumerable.Contains)
                            && m.GetParameters().Length == 2);

        private delegate Expression Binder(Expression left, Expression right);

        public Expression<Func<TSource,object>> DynamicPropertySelect<TSource>(ParameterExpression parameterExpression = null, params string[] properties)
        {
            var entityType = typeof(TSource);
            var props = properties.Select(x => entityType.GetProperty(x)).ToList();
            return Expression.Lambda<Func<TSource, object>>(Expression.Property(parameterExpression, props.First().Name), parameterExpression);
        }
        public Expression<Func<TSource, object>> DynamicLambda<TSource>
       (ParameterExpression parameterExpression = null, params string[] properties)
        {
            var entityType = typeof(TSource);
            var props = properties.Select(x => entityType.GetProperty(x)).ToList();
            var source = parameterExpression ==null? Expression.Parameter(entityType, "x"): parameterExpression;

            // create x=> new myType{ prop1 = x.prop1,...}
            var newType = CreateNewType(props);
            var binding = props.Select(p => Expression.Bind(newType.GetField(p.Name),
                          Expression.Property(source, p.Name))).ToList();
            
           


            var body = Expression.MemberInit(Expression.New(newType), binding);
            var selector = Expression.Lambda<Func<TSource, object>>(body, source);
            return selector;
        }
       // public Expression<Func<TSource, object>> DynamicGroupBy<TSource>
       //( string property,params string[] tSelect)
       // {
       //     var entityType = typeof(TSource);
       //     var props = entityType.GetProperty(property);
       //     var source = Expression.Parameter(entityType, "x");

       //     // create x=> new myType{ prop1 = x.prop1,...}
       //     var newType = CreateNewType(props);
       //     var binding = Expression.Bind(newType.GetField(props.Name),
       //                   Expression.Property(source, props.Name));

       //     var body = Expression.MemberInit(Expression.New(newType), binding);
       //     var selector = Expression.Lambda<Func<TSource, object>>(body, source);
       //     return selector;
       // }

       // public static Type CreateNewType(PropertyInfo props)
       // {
       //     AssemblyName asmName = new AssemblyName("MyAsm");
       //     AssemblyBuilder dynamicAssembly = AssemblyBuilder
       //         .DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
       //     ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("MyAsm");
       //     TypeBuilder dynamicAnonymousType = dynamicModule
       //         .DefineType("MyType", TypeAttributes.Public);

            
            
       //     dynamicAnonymousType.DefineField(props.Name, props.PropertyType, FieldAttributes.Public);
            
       //     return dynamicAnonymousType.CreateType();
       // }

        
        public static Type CreateNewType(List<PropertyInfo> props)
        {
            AssemblyName asmName =  typeof(Program).Assembly.GetName();
            AssemblyBuilder dynamicAssembly = AssemblyBuilder
                .DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("MyAsm");
            TypeBuilder dynamicAnonymousType = dynamicModule
                .DefineType("MyType", TypeAttributes.Public);

            foreach (var p in props)
            {
                dynamicAnonymousType.DefineField(p.Name, p.PropertyType, FieldAttributes.Public);
            }
            var type = dynamicAnonymousType.CreateType();
            var instance = Activator.CreateInstance(type);
            Console.WriteLine(instance.ToString());
            return type;
        }

        public void SetQueryType(JsonElement condition)
        {
            _Type.SetQuery(condition.DeserializeObject<FunctionRule>());
        }

        private Expression ParseTree<T>(
            JsonElement condition,
            ParameterExpression parm)
        {
            Expression left = null;
            var gate = condition.GetProperty(nameof(condition)).GetString();
            
            JsonElement rules = condition.GetProperty(nameof(rules));
            Console.WriteLine(nameof(rules));

            Binder binder = gate == And ? (Binder)Expression.And : Expression.Or;

            Expression bind(Expression left, Expression right) =>
                left == null ? right : binder(left, right);

            foreach (var rule in rules.EnumerateArray())
            {
                if (rule.TryGetProperty(nameof(condition), out JsonElement check))
                {
                    var right = ParseTree<T>(rule, parm);
                    left = bind(left, right);
                    continue;
                }
                
                string @operator = rule.GetProperty(nameof(@operator)).GetString();
                string type = rule.GetProperty(nameof(type)).GetString();
                string field = rule.GetProperty(nameof(field)).GetString();
                
                JsonElement value = rule.GetProperty(nameof(value));
                
                var property = Expression.Property(parm, field);
                
                if (@operator == In)
                {
                    var contains = MethodContains.MakeGenericMethod(typeof(string));
                    object val = value.EnumerateArray().Select(e => e.GetString())
                        .ToList();
                    var right = Expression.Call(
                        contains,
                        Expression.Constant(val),
                        property);
                    left = bind(left, right);
                }
                else if(@operator == And || @operator== Equal || @operator == Or)
                {
                    object val = (type == StringStr || type == BooleanStr) ?
                        (object)value.GetString() : value.GetDecimal();
                    var toCompare = Expression.Constant(val);
                    var right = Expression.Equal(property, toCompare);
                    left = bind(left, right);
                }
                else if(@operator == Select || @operator== GroupBy)
                {
                    if(@operator==GroupBy)
                    {
                        return DynamicPropertySelect<T>(parm, properties: new[] { _Type.Query.Type[0].Key });
                    }
                    var items = rule.GetProperty(nameof(value)) ;
                    var values  = items.GetEnumeratorList<string>();
                    var fields = rule.GetProperty(nameof(field)).GetString();
                    var lambda =DynamicLambda<T>(parm,properties: values.ToArray());

                    if(@operator ==Select)
                    {
                        return lambda;
                    }
                    


                }
               
            }

            return left;
        }

        public Expression<Func<T,bool>> ParseExpressionOf<T>(JsonDocument doc)
        {
            var itemExpression = Expression.Parameter(typeof(T));
            var conditions = ParseTree<T>(doc.RootElement, itemExpression);
            if (conditions.CanReduce)
            {
                conditions = conditions.ReduceAndCheck();
            }

            Console.WriteLine(conditions.ToString());
            if(_Type.IsInnerType(typeof(FunctionRule)))
            {
                return conditions as Expression<Func<T,bool>>;
            }
            var query = Expression.Lambda<Func<T, bool>>(conditions, itemExpression);
            return query;
        }
        public Expression<Func<T, object>> ParseExpression<T>(JsonDocument doc)
        {
            var itemExpression = Expression.Parameter(typeof(T));
            var conditions = ParseTree<T>(doc.RootElement, itemExpression);
            if (conditions.CanReduce)
            {
                conditions = conditions.ReduceAndCheck();
            }

            Console.WriteLine(conditions.ToString());
            if (_Type.Query.Condition == GroupBy || _Type.Query.Condition == Select)
                return conditions as Expression<Func<T, object>>;

            var query = Expression.Lambda<Func<T, object>>(conditions, itemExpression);
            return query;
        }
        public Func<T, object> ParsePredicate<T>(JsonDocument doc)
        {
            var query = ParseExpression<T>(doc);
            return query.Compile();
        }
        public Func<T, bool> ParsePredicateOf<T>(JsonDocument doc)
        {
            var query = ParseExpressionOf<T>(doc);
            return query.Compile();
        }
    }
}