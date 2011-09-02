using System;
using System.Linq;
using System.Linq.Expressions;

namespace LambdaExpressionsFun
{
    class Program
    {
        static void Main(string[] args)
        {
            DoStuff();

            Console.ReadLine();
        }

        private static void DoStuff()
        {
            var thing = new Thing
            {
                Bizzle = "Roaaar!",
                Bozzli = "Ring ring.",
                Wang = new Thing.ThingTwo
                {
                    Boogle = "Baaaaaah",
                    Bring = new Thing.ThingTwo.Thingy
                    {
                        Spring = "Wing",
                    },
                },
            };

            Expression<Func<Thing, object>> bizzleExpr = t => t.Bizzle;
            var bizzleName = GetPropName(bizzleExpr);
            var bizzleValue = GetValue(bizzleExpr, thing);

            Console.WriteLine("Bizzle name: {0}", bizzleName);
            Console.WriteLine("Bizzle value: {0}", bizzleValue);
            Console.WriteLine();

            Expression<Func<Thing, object>> wangBoogleExpr = t => t.Wang.Boogle;
            var wangBoogleName = GetPropName(wangBoogleExpr);
            var wangBoogleValue = GetValue(wangBoogleExpr, thing);

            Console.WriteLine("Wang.Boogle name: {0}", wangBoogleName);
            Console.WriteLine("Wang.Boogle name: {0}", wangBoogleValue);
            Console.WriteLine();

            Console.WriteLine("Method body for t.Wang.Boogle: {0}", wangBoogleExpr.Body);
            Console.WriteLine();

            Console.WriteLine("Type of Wang in t.Wang.Boogle: {0}", GetContainerType(thing, wangBoogleExpr));

            Console.WriteLine("Type of Wang.Bring in t.Wang.Bring.Spring: {0}",
                              GetContainerType(thing, t => t.Wang.Bring.Spring));
        }

        private static object GetValue(Expression<Func<Thing, object>> expression, Thing thing)
        {
            return expression.Compile()(thing);
        }

        private static string GetPropName(Expression<Func<Thing, object>> expression)
        {
            return GetName(expression);
        }
        
        public static string GetName<T1, T2>(Expression<Func<T1, T2>> expression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }

        public static Type GetContainerType<T1>(T1 model, Expression<Func<T1, object>> expression)
        {
            var bodyParts = expression.Body.ToString().Split('.');
            var bodyPartsMinusFirstAndLastProperties =
                bodyParts.Except(new[] {bodyParts.First(), bodyParts.Last()});
            var bodyExpressionString = "x." + string.Join(".", bodyPartsMinusFirstAndLastProperties);
            
            if(bodyPartsMinusFirstAndLastProperties.Count() == 0)
            {
                return typeof (T1);
            }

            var x = Expression.Parameter(typeof (T1), "x");

            var containerExpression = System.Linq.Dynamic.DynamicExpression
                .ParseLambda(new[] {x}, typeof(object), bodyExpressionString);

            var compiledExpression = containerExpression.Compile();

            var thing = compiledExpression.DynamicInvoke(model);

            return thing.GetType();
        }

    }

    public class Thing
    {
        public string Bizzle { get; set; }
        public string Bozzli { get; set; }

        public ThingTwo Wang { get; set; }

        public class ThingTwo
        {
            public string Boogle { get; set; }
            public Thingy Bring { get; set; }

            public class Thingy
            {
                public string Spring { get; set; }
            }
        }
    }
}
