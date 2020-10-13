using System;
using System.Linq.Expressions;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations //.Cypher
{
    /// <summary>
    ///     All-purpose class for getting variables already declared in a Cypher query.
    /// </summary>
    public sealed class CypherVariables
    {
        /// <summary>
        ///     Gets a variable as <see cref="CypherObject" />. Properties of the variable can be accessed from the
        ///     <see cref="CypherObject" /> returned.
        ///     E.g. (Actor actor) =&gt; actor.Movie == CypherVariables.Get("movie")["name"].
        /// </summary>
        /// <param name="name">The name of the variable or expression to get</param>
        /// <returns></returns>
        public static CypherObject Get(string name)
        {
            throw new NotImplementedException(Messages.VarsGetError);
        }

        /// <summary>
        ///     Shortcut for getting a variable as a certain type.
        ///     For example, CypherVariables.Get&lt;User&gt;("user").Name, is the exact same as CypherVariables.Get("user")["name"]
        ///     ... assuming camel case serialization.
        /// </summary>
        /// <typeparam name="TResult">The type of variable or expression to return.</typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static TResult Get<TResult>(string name)
        {
            throw new NotImplementedException(Messages.VarsGetError);
        }

        /// <summary>
        ///     Shortcut for getting a variable or expression as a certain type.
        ///     For example, CypherVariables.Get&lt;User, string&gt;(user => user.Address.City).Name, is the exact same as
        ///     CypherVariables.Get&lt;string&gt;("user.address_city")... assuming camel case serialization.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult">The type of variable or expression to return.</typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TResult Get<TSource, TResult>(Expression<Func<TSource, TResult>> selector)
        {
            throw new NotImplementedException(Messages.VarsGetError);
        }
    }
}