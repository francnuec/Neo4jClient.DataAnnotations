using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    /// <summary>
    /// All-purpose class for getting parameters already declared in a Cypher query.
    /// </summary>
    public sealed partial class Params
    {
        /// <summary>
        /// Gets a parameter as <see cref="CypherObject"/>. Properties of the parameter can be accessed from the <see cref="CypherObject"/> returned.
        /// E.g. (Actor actor) => actor.Movie == Params.Get("movie")["name"].
        /// </summary>
        /// <param name="name">The name of the parameter to get</param>
        /// <returns></returns>
        public static CypherObject Get(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shortcut for getting a parameter as a certain type.
        /// For example, Params.Get&lt;User>("user").Name, is the exact same as Params.Get("user")["name"] ... assuming camel case serialization.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to return.</typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}
