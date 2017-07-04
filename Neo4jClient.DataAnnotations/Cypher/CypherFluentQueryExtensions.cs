using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public static partial class CypherFluentQueryExtensions
    {
        ///// <summary>
        ///// Converts this <see cref="ICypherFluentQuery"/> to an <see cref="IAnnonatedQuery"/>.
        ///// This may also resolve method signature conflicts between <see cref="ICypherFluentQuery"/> and <see cref="IAnnotatedBuilder"/> methods.
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //public static IAnnotatedBuilder AsAnnonated(this ICypherFluentQuery query)
        //{
        //    return new DummyAnnonatedQuery(query);
        //}

        ///// <summary>
        ///// Converts this <see cref="IAnnotatedBuilder"/> back to its underlying <see cref="ICypherFluentQuery"/>.
        ///// This is particularly useful in cases where some <see cref="ICypherFluentQuery"/> methods are missing from the <see cref="IAnnotatedBuilder"/> extension methods.
        ///// </summary>
        ///// <returns><see cref="ICypherFluentQuery"/></returns>
        //public static ICypherFluentQuery AsCypherFluent(this IAnnotatedBuilder query)
        //{
        //    return query.CypherFluentQuery;
        //}

        public static ICypherFluentQuery Match(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathable>>[] patterns)
        {
            query.Match((p) => p.Pattern<string>("").Label(null).Constrain((a) => a == (object)12).Shortest()
            //.Extend(RelationshipDirection.Outgoing).Label(null).Constrain(null)
            //.ThenExtend(RelationshipDirection.Incoming).Label(null).Constrain(null)
            //.Assign()
            );

            return null;
        }

        //public static IPathBuilder Match(this IAnnotatedBuilder query)
        //{
        //    return null;
        //}

        public static ICypherFluentQuery OptionalMatch(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathable>>[] patterns)
        {
            return null;
        }

        //public static IPathBuilder OptionalMatch(this IAnnotatedBuilder query)
        //{
        //    return null;
        //}

        public static ICypherFluentQuery Merge(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathable>>[] patterns)
        {
            return null;
        }

        //public static IPathBuilder Merge(this IAnnotatedBuilder query)
        //{
        //    return null;
        //}

        public static ICypherFluentQuery Create(this ICypherFluentQuery query, params Expression<Func<IPathBuilder, IPathable>>[] patterns)
        {
            //query.Create()
            //    .Pattern("")
            //    .Label(null)
            //    .Constrain((a) => a["yes"] == "ok")
            //    .ExtendedByPath(RelationshipDirection.Outgoing)
            //    .ThenByPath(RelationshipDirection.Outgoing)
            //    .AndThen()
            //    .Pattern()

            //query.Create().Pattern((p) => p.Label(null).Constrain((a)));

            return null;
        }
    }
}
