using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    /// <summary>
    /// (A)-[R]-(B)
    /// </summary>
    public interface IPattern : IAnnotated
    {
        IPathExtent Path { get; }

        string AParameter { get; }
        string RParameter { get; }
        string BParameter { get; }

        Type AType { get; }
        Type RType { get; }
        Type BType { get; }

        LambdaExpression ABSelector { get; }

        LambdaExpression ARSelector { get; }
        LambdaExpression RBSelector { get; }

        Tuple<int?, int?> RHops { get; }

        IEnumerable<string> ALabels { get; }
        IEnumerable<string> RTypes { get; }
        IEnumerable<string> BLabels { get; }

        bool UseGivenALabelsOnly { get; }
        bool UseGivenRTypesOnly { get; }
        bool UseGivenBLabelsOnly { get; }

        LambdaExpression AProperties { get; }
        LambdaExpression RProperties { get; }
        LambdaExpression BProperties { get; }

        LambdaExpression AConstraints { get; }
        LambdaExpression RConstraints { get; }
        LambdaExpression BConstraints { get; }

        RelationshipDirection? Direction { get; }
        
        bool isExtension { get; }
    }
}
