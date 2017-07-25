using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    /// <summary>
    /// All-purpose class for getting variables already declared in a Cypher query.
    /// </summary>
    public sealed class Vars
    {
        /// <summary>
        /// Gets a variable as <see cref="CypherObject"/>. Properties of the variable can be accessed from the <see cref="CypherObject"/> returned.
        /// E.g. (Actor actor) => actor.Movie == Vars.Get("movie")["name"].
        /// </summary>
        /// <param name="name">The name of the variable to get</param>
        /// <returns></returns>
        public static CypherObject Get(string name)
        {
            throw new NotImplementedException(Messages.VarsGetError);
        }

        /// <summary>
        /// Shortcut for getting a variable as a certain type.
        /// For example, Vars.Get&lt;User>("user").Name, is the exact same as Vars.Get("user")["name"] ... assuming camel case serialization.
        /// </summary>
        /// <typeparam name="T">The type of variable to return.</typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Get<T>(string name)
        {
            throw new NotImplementedException(Messages.VarsGetError);
        }
    }
}
