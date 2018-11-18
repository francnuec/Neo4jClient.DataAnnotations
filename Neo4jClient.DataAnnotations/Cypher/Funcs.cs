using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class Funcs
    {
        /// <summary>
        /// The neo4j asterisk (*) wildcard.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static JRaw Star { get; } = new JRaw("*");

        /// <summary>
        /// The neo4j collect aggregator.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TSource> Collect<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j length function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Length<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j exists function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool Exists<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j size function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Size<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j id function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Id<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j type function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static string Type<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j properties function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static CypherObject Properties<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j timestamp function
        /// </summary>
        /// <returns></returns>
        public static long Timestamp()
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j avg function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Average<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j max function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Max<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j min function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Min<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j sum function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Sum<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j startNode function
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn StartNode<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j endNode function
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn EndNode<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j count function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Count<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j keys function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<string> Keys<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j labels function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<string> Labels<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j nodes function.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> Nodes<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j relationships function.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> Relationships<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j distinct function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Distinct<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j range function
        /// </summary>
        /// <returns></returns>
        public static List<int> Range<TStart, TEnd>(TStart start, TEnd end)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j range function
        /// </summary>
        /// <returns></returns>
        public static List<int> Range<TStart, TEnd, TStep>(TStart start, TEnd end, TStep step)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j tail function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<TSource> Tail<TSource>(IEnumerable<TSource> source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j stDev function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double StandardDeviation<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j stDevP function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double StandardDeviationP<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j percentileCont function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="source"></param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public static double PercentileCont<TSource, TP>(TSource source, TP percentile)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j percentileDisc function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TP"></typeparam>
        /// <param name="source"></param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public static int PercentileDisc<TSource, TP>(TSource source, TP percentile)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j none function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool None<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j single function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool Single<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j rand function
        /// </summary>
        /// <returns></returns>
        public static double Rand()
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j e function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double E<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j cot function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Cot<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j degrees function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Degrees<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j haversin function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double HalfVersine<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j pi function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double PI<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j radians function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Radians<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }
    }
}
