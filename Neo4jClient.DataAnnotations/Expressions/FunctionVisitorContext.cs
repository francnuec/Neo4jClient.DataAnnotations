using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class FunctionVisitorContext : IHaveAnnotationsContext
    {
        protected internal FunctionExpressionVisitor Visitor { get; set; }

        public string GetBuiltText() { return Visitor?.Builder.ToString(); }

        public EntityResolver GetResolver() { return Visitor?.Resolver; }
        public Func<object, string> GetSerializer() { return Visitor?.Serializer; }

        public bool WriteParameters { get; set; } = true;
        public bool WriteOperators { get; set; } = true;
        public bool WriteConstants { get; set; } = true;
        public bool RewriteNullEqualityComparisons { get; set; } = true;

        public bool? UseResolvedJsonName { get; set; } = null;

        public Dictionary<Type, List<Func<FunctionHandlerContext, Func<Expression>>>> TypeHandlers { get; set; } = FunctionExpressionVisitor.DefaultTypeHandlers;
        public Dictionary<ExpressionType, List<Func<FunctionHandlerContext, Func<Expression>>>> NodeTypeHandlers { get; set; } = FunctionExpressionVisitor.DefaultNodeTypeHandlers;
        public Dictionary<MemberInfo, Func<FunctionHandlerContext, Func<Expression>>> MemberInfoHandlers { get; set; } = FunctionExpressionVisitor.DefaultMemberInfoHandlers;

        public Dictionary<ExpressionType, string> BinaryOperators { get; set; } = FunctionExpressionVisitor.DefaultBinaryOperators;
        public Dictionary<ExpressionType, string> LeftAlignedUnaryOperators { get; set; } = FunctionExpressionVisitor.DefaultLeftAlignedUnaryOperators;
        public Dictionary<ExpressionType, string> RightAlignedUnaryOperators { get; set; } = FunctionExpressionVisitor.DefaultRightAlignedUnaryOperators;

        public Func<object, string> CreateParameter
        {
            get
            {
                if (Visitor?.QueryContext?.CurrentQueryWriter is var qw && qw != null)
                    return qw.CreateParameter;

                return null;
            }
        }

        public Action<string, object> CreateParameterWithKey
        {
            get
            {
                if (Visitor?.QueryContext?.CurrentQueryWriter is var qw && qw != null)
                    return qw.CreateParameter;

                return null;
            }
        }

        public PropertiesBuildStrategy BuildStrategy => Visitor?.QueryContext?.CurrentBuildStrategy ?? 
            (CreateParameter != null ? PropertiesBuildStrategy.WithParams : PropertiesBuildStrategy.NoParams);

        /// <summary>
        /// Any other information/options that may be needed by methods should be added here.
        /// </summary>
        public Dictionary<string, object> Misc { get; set; } = new Dictionary<string, object>();

        public IAnnotationsContext AnnotationsContext => Visitor?.AnnotationsContext;

        public IEntityService EntityService => AnnotationsContext?.EntityService;
    }
}
