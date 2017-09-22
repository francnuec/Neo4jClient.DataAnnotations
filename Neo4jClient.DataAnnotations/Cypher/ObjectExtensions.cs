using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// No Further Processing. Naming and other processing escape. This instructs the expression visitors to use as specified.
        /// NOTE: This method does not affect serialization. So inner complex typed properties would still serialize to exploded properties as expected.
        /// This method is mainly used by the expression visitors, and mostly at the top surface level (rarely deep into the object).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T _<T>(this T obj)
        {
            return obj;
        }

        /// <summary>
        /// Casts an object to a certain type so as to use its properties directly.
        /// Use only in expressions, especially with <see cref="Vars"/> method calls (in which case it Pseudo-casts).
        /// NOTE: THIS METHOD IS NOT SAFE TO EXECUTE. If the cast fails, it merely generates a default value for the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TReturn _As<TReturn>(this object obj)
        {
            //do something, just in case this method was executed
            TReturn ret = default(TReturn);

            try
            {
                ret = (TReturn)obj;
            }
            catch
            {

            }

            return ret;
        }

        /// <summary>
        /// Allows you to expressively assign new values (especially <see cref="Vars"/>) to select properties of an existing instance without cloning or modifying the original instance.
        /// These new values would be used in place of the old ones on the instance.
        /// E.g. () =&gt; ellenPompeo._Set(actor =&gt; actor.Born == Vars.Get&lt;Actor&gt;("shondaRhimes").Born);
        /// NOTE: THIS IS NOT CYPHER SET CLAUSE. EXPECTED USE IS WITHIN A PATTERN EXPRESSION. The member selection must always be on the left, and new values on the right of a logical equal-to ('==') operation. Use '&amp;&amp;' for more properties.
        /// Also note that this method does not modify this instance, but overrides its final properties written to cypher.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static T _Set<T>(this T instance, Expression<Func<T, bool>> predicate)
        {
            return _Set(instance, predicate, usePredicateOnly: false);
        }

        internal static T _Set<T>(this T instance, Expression<Func<T, bool>> predicate, bool usePredicateOnly)
        {
            return instance;
        }
    }
}
