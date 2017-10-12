using System;
using System.Linq.Expressions;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class FunctionHandlerContext
    {
        public Expression Expression { get; internal set; }
        public FunctionVisitorContext VisitorContext { get; internal set; }

        /// <summary>
        /// The Visitor is only available when the handler is finally executed, and not at the test phase.
        /// </summary>
        public FunctionExpressionVisitor Visitor { get; internal set; }

        /// <summary>
        /// The Continuation delegate is only available when the handler is finally executed, and not at the test phase.
        /// </summary>
        public Func<Expression> Continuation { get; internal set; }

        protected internal Func<Expression> Handler { get; set; }
    }
}
