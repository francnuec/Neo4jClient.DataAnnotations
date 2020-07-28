using Neo4jClient.Cypher;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public enum PropertiesBuildStrategy
    {
        /// <summary>
        ///     Writes the properties directly into the generated pattern.
        ///     E.g. (a:Movie { title: "Grey's Anatomy", year: 2017 }
        /// </summary>
        NoParams = 0,

        /// <summary>
        ///     Stores the properties via a <see cref="ICypherFluentQuery.WithParam(string, object)" /> call, and replaces it with
        ///     a parameter.
        ///     E.g. (a:Movie { movie }), where "movie" is the parameter. This style can be used for the CREATE and CREATE UNIQUE
        ///     statements.
        ///     NOTE that if you have variables in your properties with this strategy set, <see cref="WithParamsForValues" /> is
        ///     used instead.
        /// </summary>
        WithParams,

        /// <summary>
        ///     Stores the properties via a <see cref="ICypherFluentQuery.WithParam(string, object)" /> call, and replaces each
        ///     property value with corresponding parameter property.
        ///     E.g. (a:Movie { title: {movie}.title, year: {movie}.year }), where "movie" is the parameter. This style is
        ///     applicable to the MATCH and OPTIONAL MATCH statements.
        /// </summary>
        WithParamsForValues
    }
}