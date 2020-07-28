﻿using System;
using System.Collections.Generic;
using Neo4jClient.DataAnnotations.Utils;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations //.Cypher
{
    public static class CypherFunctions
    {
        /// <summary>
        ///     The neo4j asterisk (*) wildcard.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static JRaw Star { get; } = new JRaw("*");


        /// <summary>
        ///     No Further Processing. Naming and other processing escape. This instructs the expression visitors to use as
        ///     specified.
        ///     NOTE: This method does not affect serialization. So inner complex typed properties would still serialize to
        ///     exploded properties as expected.
        ///     This method is mainly used by the expression visitors, and mostly at the top surface level (rarely deep into the
        ///     object).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T _<T>(T obj)
        {
            return obj;
        }

        /// <summary>
        ///     Casts an object to a certain type so as to use its properties directly.
        ///     Use only in expressions, especially with <see cref="CypherVariables" /> method calls (in which case it
        ///     Pseudo-casts).
        ///     NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it merely generates a default value for the return
        ///     type.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn _As<TReturn>(object obj)
        {
            //do something, just in case this method was executed
            TReturn ret = default;

            try
            {
                ret = (TReturn)obj;
            }
            catch
            {
                throw new NotImplementedException(Messages.DummyMethodInvokeError);
            }

            return ret;
        }

        /// <summary>
        ///     Casts an object to a list of a certain type so as to use IEnumerable extension methods.
        ///     Use only in expressions, especially with <see cref="CypherVariables" /> method calls (in which case it
        ///     Pseudo-casts).
        ///     Shortcut for <code>._As&lt;List&lt;T&gt;&gt;()</code>
        ///     NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it throws an error.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> _AsList<TReturn>(object obj)
        {
            //do something, just in case this method was executed
            List<TReturn> ret = null;

            try
            {
                ret = (List<TReturn>)obj;
            }
            catch
            {
                throw new NotImplementedException(Messages.DummyMethodInvokeError);
            }

            return ret;
        }

        /// <summary>
        ///     Casts an object to a list of a certain type so as to use IEnumerable extension methods.
        ///     Use only in expressions, especially with <see cref="CypherVariables" /> method calls (in which case it
        ///     Pseudo-casts).
        ///     Shortcut for <code>._As&lt;List&lt;T&gt;&gt;()</code>
        ///     NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it merely returns a list having
        ///     <paramref name="obj" /> as only item.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<TSource> _AsList<TSource>(TSource obj)
        {
            //do something, just in case this method was executed
            var ret = _AsList<TSource>(obj as object);

            //try
            //{
            //    ret = new List<TSource>() { obj };
            //}
            //catch
            //{
            //    throw new NotImplementedException(Messages.DummyMethodInvokeError);
            //}

            return ret;
        }


        /// <summary>
        ///     The neo4j <code>IS NULL</code> function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool IsNull<TSource>(TSource obj)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j <code>IS NOT NULL</code> function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool IsNotNull<TSource>(TSource obj)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j collect aggregator.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TSource> Collect<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j length function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Length<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j exists function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static bool Exists<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j size function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Size<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j id function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Id<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j type function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static string Type<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j properties function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static CypherObject Properties<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j timestamp function
        /// </summary>
        /// <returns></returns>
        public static long Timestamp()
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j avg function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Average<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j max function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Max<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j min function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Min<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j sum function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Sum<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j startNode function
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn StartNode<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j endNode function
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static TReturn EndNode<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j count function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static int Count<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j keys function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<string> Keys<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j labels function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<string> Labels<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j nodes function.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> Nodes<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j relationships function.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public static List<TReturn> Relationships<TReturn>(object source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j distinct function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static TSource Distinct<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j range function
        /// </summary>
        /// <returns></returns>
        public static List<int> Range<TStart, TEnd>(TStart start, TEnd end)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j range function
        /// </summary>
        /// <returns></returns>
        public static List<int> Range<TStart, TEnd, TStep>(TStart start, TEnd end, TStep step)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j tail function.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static List<TSource> Tail<TSource>(IEnumerable<TSource> source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j stDev function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double StandardDeviation<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j stDevP function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double StandardDeviationP<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j percentileCont function
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
        ///     The neo4j percentileDisc function
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
        ///     The neo4j none function.
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
        ///     The neo4j single function.
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
        ///     The neo4j rand function
        /// </summary>
        /// <returns></returns>
        public static double Rand()
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j e function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double E<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j cot function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Cot<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j degrees function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Degrees<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j haversin function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Haversin<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j pi function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double PI<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }

        /// <summary>
        ///     The neo4j radians function
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static double Radians<TSource>(TSource source)
        {
            throw new NotImplementedException(Messages.FunctionsInvokeError);
        }
    }
}