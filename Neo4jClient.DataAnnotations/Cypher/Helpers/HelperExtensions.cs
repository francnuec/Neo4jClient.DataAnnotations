//using System;
//using Neo4jClient.DataAnnotations.Utils;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;
//using System.Linq;
//using Newtonsoft.Json.Linq;

//namespace Neo4jClient.DataAnnotations.Cypher.Helpers
//{
//    public static class HelperExtensions
//    {
//        /// <summary>
//        /// No Further Processing. Naming and other processing escape. This instructs the expression visitors to use as specified.
//        /// NOTE: This method does not affect serialization. So inner complex typed properties would still serialize to exploded properties as expected.
//        /// This method is mainly used by the expression visitors, and mostly at the top surface level (rarely deep into the object).
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="obj"></param>
//        /// <returns></returns>
//        public static T _<T>(this T obj)
//        {
//            return obj;
//        }

//        /// <summary>
//        /// Casts an object to a certain type so as to use its properties directly.
//        /// Use only in expressions, especially with <see cref="CypherVariables"/> method calls (in which case it Pseudo-casts).
//        /// NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it merely generates a default value for the return type.
//        /// </summary>
//        /// <typeparam name="TReturn"></typeparam>
//        /// <returns></returns>
//        public static TReturn _As<TReturn>(this object obj)
//        {
//            //do something, just in case this method was executed
//            TReturn ret = default(TReturn);

//            try
//            {
//                ret = (TReturn)obj;
//            }
//            catch
//            {
//                throw new NotImplementedException(Messages.DummyMethodInvokeError);
//            }

//            return ret;
//        }

//        /// <summary>
//        /// Casts an object to a list of a certain type so as to use IEnumerable extension methods.
//        /// Use only in expressions, especially with <see cref="CypherVariables"/> method calls (in which case it Pseudo-casts).
//        /// Shortcut for <code>._As&lt;List&lt;T&gt;&gt;()</code>
//        /// NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it throws an error.
//        /// </summary>
//        /// <typeparam name="TReturn"></typeparam>
//        /// <returns></returns>
//        public static List<TReturn> _AsList<TReturn>(this object obj)
//        {
//            //do something, just in case this method was executed
//            List<TReturn> ret = null;

//            try
//            {
//                ret = (List<TReturn>)obj;
//            }
//            catch
//            {
//                throw new NotImplementedException(Messages.DummyMethodInvokeError);
//            }

//            return ret;
//        }

//        /// <summary>
//        /// Casts an object to a list of a certain type so as to use IEnumerable extension methods.
//        /// Use only in expressions, especially with <see cref="CypherVariables"/> method calls (in which case it Pseudo-casts).
//        /// Shortcut for <code>._As&lt;List&lt;T&gt;&gt;()</code>
//        /// NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it merely returns a list having <paramref name="obj"/> as only item.
//        /// </summary>
//        /// <typeparam name="TSource"></typeparam>
//        /// <returns></returns>
//        public static List<TSource> _AsList<TSource>(this TSource obj)
//        {
//            //do something, just in case this method was executed
//            List<TSource> ret = _AsList<TSource>(obj as object);

//            //try
//            //{
//            //    ret = new List<TSource>() { obj };
//            //}
//            //catch
//            //{
//            //    throw new NotImplementedException(Messages.DummyMethodInvokeError);
//            //}

//            return ret;
//        }

//        ///// <summary>
//        ///// Returns this string as a raw value, and not a string value. So instead of outputing a value with quotation marks, it outputs the value directly.
//        ///// </summary>
//        ///// <returns></returns>
//        //public static JRaw _AsRaw(this string value)
//        //{
//        //    throw new NotImplementedException(Messages.DummyMethodInvokeError); //return new JRaw(value);
//        //}

//        ///// <summary>
//        ///// Allows you to expressively assign new values (especially <see cref="CypherVariables"/>) to select properties of an existing instance without cloning or modifying the original instance.
//        ///// These new values would be used in place of the old ones on the instance.
//        ///// E.g. () =&gt; ellenPompeo._Set(actor =&gt; actor.Born == CypherVariables.Get&lt;Actor&gt;("shondaRhimes").Born);
//        ///// NOTE: THIS IS NOT CYPHER SET CLAUSE. EXPECTED USE IS WITHIN A PATTERN EXPRESSION. The member selection must always be on the left, and new values on the right of a logical equal-to ('==') operation. Use '&amp;&amp;' for more properties.
//        ///// Also note that this method does not modify this instance, but overrides its final properties written to cypher.
//        ///// </summary>
//        ///// <typeparam name="T"></typeparam>
//        ///// <param name="instance"></param>
//        ///// <param name="predicate"></param>
//        ///// <returns></returns>
//        //public static T _Set<T>(this T instance, Expression<Func<T, bool>> predicate)
//        //{
//        //    return _Set(instance, predicate, usePredicateOnly: false);
//        //}

//        //internal static T _Set<T>(this T instance, Expression<Func<T, bool>> predicate, bool usePredicateOnly)
//        //{
//        //    return instance;
//        //}
//    }
//}
