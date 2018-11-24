using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher.Functions
{
    /// <summary>
    /// To avoid method signature conflicts and clogging our intellisence, these were separated into another class.
    /// You can still use the <see cref="CypherFunctions"/> class as an aternative.
    /// </summary>
    public static class CypherExtensionFunctions
    {
        /// <summary>
        /// The neo4j <code>IS NULL</code> function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool IsNull<TSource>(this TSource obj)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError); //return obj == null;
        }

        /// <summary>
        /// The neo4j <code>IS NOT NULL</code> function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool IsNotNull<TSource>(this TSource obj)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError); //return !IsNull(obj);
        }

        /// <summary>
        /// The neo4j collect aggregator.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<TSource> Collect<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError); //return new List<TSource>() { }; //doing our best to return something asides null so our methods compile
        }

        /// <summary>
        /// The neo4j length function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Length<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError); //return 0;
        }

        /// <summary>
        /// The neo4j exists function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool Exists<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError); //return false;
        }

        /// <summary>
        /// The neo4j size function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Size<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError); //return 0;
        }

        /// <summary>
        /// The neo4j id function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Id<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j type function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static string Type<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j properties function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static CypherObject Properties<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j avg function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Average<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j max function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Max<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j min function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Min<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j sum function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Sum<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j startNode function
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn StartNode<TReturn>(this object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j endNode function
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn EndNode<TReturn>(this object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j count function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Count<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j keys function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<string> Keys<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j labels function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<string> Labels<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j nodes function.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> Nodes<TReturn>(this object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j relationships function.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> Relationships<TReturn>(this object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j distinct function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Distinct<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j stDev function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double StandardDeviation<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j stDevP function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double StandardDeviationP<TSource>(this TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j percentileCont function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double PercentileCont<TSource>(this TSource source, double percentile)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j percentileDisc function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int PercentileDisc<TSource>(this TSource source, double percentile)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        /// The neo4j tail function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<TSource> Tail<TSource>(this IEnumerable<TSource> source)
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
        public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
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
        public static bool Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }
    }
}
