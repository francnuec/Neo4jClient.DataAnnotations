using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public partial class FunctionExpressionVisitor : ExpressionVisitor
    {
        public static Dictionary<MemberInfo, Func<FunctionHandlerContext, Func<Expression>>>
            DefaultMemberInfoHandlers = new Dictionary<MemberInfo, Func<FunctionHandlerContext, Func<Expression>>>()
            {
                { Utilities.GetMethodInfo(() => Vars.Get(null)), FunctionHandlers.GetVariable },
                { Defaults.CypherObjectIndexerInfo, FunctionHandlers.CypherObjectIndexer },
                { Utilities.GetMethodInfo(() => Enumerable.ElementAt<object>(null, 0)), FunctionHandlers.ElementAt },
                { Utilities.GetMethodInfo(() => Math.Pow(0, 0)), FunctionHandlers.MathPower },
                { Utilities.GetMethodInfo(() => ObjectExtensions.IsNull<object>(null)), FunctionHandlers.IsNull },
                { Utilities.GetMethodInfo(() => ObjectExtensions.IsNotNull<object>(null)), FunctionHandlers.IsNotNull },
                { Utilities.GetMethodInfo(() => ObjectExtensions._AsRaw(null)), FunctionHandlers._AsRaw },

                #region ICypherResultItem Functions
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).As<object>()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "As", "", (m, cc) =>
                    {
                        //This should never be executed as "As" method should never have been called here.
                        //But just in case, we provide a simple implementation
                        cc.Continuation();
                        //then ignore current node before processing unhandled nodes
                        cc.Visitor.Ignore(m);
                        cc.Visitor.ProcessUnhandledSimpleVars(m);
                        return m;
                    }) },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Id()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Id", "id") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).CollectAs<object>()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "CollectAs", "collect") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).CollectAsDistinct<object>()),
                    (c) => FunctionHandlers.ICypherResultItemMethod(c, "CollectAsDistinct", "collect", (m, cc) =>
                    {
                        cc.Visitor.Builder.Append("DISTINCT ");
                        cc.Continuation();

                        return cc.Expression;
                    }) },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Count()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Count", "count") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).CountDistinct()),
                    (c) => FunctionHandlers.ICypherResultItemMethod(c, "CountDistinct", "count", (m, cc) =>
                    {
                        cc.Visitor.Builder.Append("DISTINCT ");
                        cc.Continuation();

                        return cc.Expression;
                    }) },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Head()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Head", "head") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Labels()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Labels", "labels") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Last()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Last", "last") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Length()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Length", "length") },
                { Utilities.GetMethodInfo(() => (null as ICypherResultItem).Type()), (c) => FunctionHandlers.ICypherResultItemMethod(c, "Type", "type") },
                #endregion

                #region String Functions
                { Utilities.GetMethodInfo(() => new object().ToString()), FunctionHandlers.ToString },
                { Utilities.GetPropertyInfo(() => "".Length), FunctionHandlers.StringLength },
                { Utilities.GetMethodInfo(() => "".ToLower()), (c) => FunctionHandlers.StringMethod(c, "ToLower", "toLower") },
                { Utilities.GetMethodInfo(() => "".ToUpper()), (c) => FunctionHandlers.StringMethod(c, "ToUpper", "toUpper") },
                { Utilities.GetMethodInfo(() => "".Trim()), (c) => FunctionHandlers.StringMethod(c, "Trim", "trim") },
                { Utilities.GetMethodInfo(() => "".TrimStart()), (c) => FunctionHandlers.StringMethod(c, "TrimStart", "lTrim") },
                { Utilities.GetMethodInfo(() => "".TrimEnd()), (c) => FunctionHandlers.StringMethod(c, "TrimEnd", "rTrim") },
                { Utilities.GetMethodInfo(() => "".Replace(null, null)),
                    (c) => FunctionHandlers.StringMethod(c, "Replace", "replace", FunctionHandlers.StringReplaceBody) },
                { Utilities.GetMethodInfo(() => "".Substring(0)),
                    (c) => FunctionHandlers.StringMethod(c, "Substring", "substring", FunctionHandlers.StringSubstringBody) },
                { Utilities.GetMethodInfo(() => Enumerable.Reverse<object>(null)), FunctionHandlers.Reverse },
                { Utilities.GetMethodInfo(() => "".StartsWith(null)),
                    (c) => FunctionHandlers.StringComparison(c, "StartsWith", "STARTS WITH") },
                { Utilities.GetMethodInfo(() => "".EndsWith(null)),
                    (c) => FunctionHandlers.StringComparison(c, "EndsWith", "ENDS WITH") },
                { Utilities.GetMethodInfo(() => "".Contains(null)),
                    (c) => FunctionHandlers.StringComparison(c, "Contains", "CONTAINS") },
                { Utilities.GetMethodInfo(() => "".Split(null)),
                    (c) => FunctionHandlers.StringMethod(c, "Split", "split", FunctionHandlers.StringSplitBody) },
                #endregion

                #region Scalar Functions
                { Utilities.GetMethodInfo(() => Enumerable.First<object>(null)),
                    (c) => FunctionHandlers.EnumerableMethod(c, "First", "head") },
                { Utilities.GetMethodInfo(() => Enumerable.FirstOrDefault<object>(null)),
                    (c) => FunctionHandlers.EnumerableMethod(c, "FirstOrDefault", "head") },
                { Utilities.GetMethodInfo(() => Enumerable.Last<object>(null)),
                    (c) => FunctionHandlers.EnumerableMethod(c, "Last", "last") },
                { Utilities.GetMethodInfo(() => Enumerable.LastOrDefault<object>(null)),
                    (c) => FunctionHandlers.EnumerableMethod(c, "LastOrDefault", "last") },
                { Utilities.GetPropertyInfo(() => new List<int>().Count), FunctionHandlers.CollectionCount },
                { Utilities.GetMethodInfo(() => Convert.ToInt32(null)),
                    (c) => FunctionHandlers.ConvertMethod(c, "ToInt32", "toInteger") },
                { Utilities.GetMethodInfo(() => Convert.ToSingle(null)),
                    (c) => FunctionHandlers.ConvertMethod(c, "ToSingle", "toFloat") },
                { Utilities.GetMethodInfo(() => Convert.ToBoolean(null)),
                    (c) => FunctionHandlers.ConvertMethod(c, "ToBoolean", "toBoolean") },
                { Utilities.GetMethodInfo(() => Funcs.Length<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Length", "length") },
                { Utilities.GetMethodInfo(() => Funcs.Size<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Size", "size") },
                { Utilities.GetMethodInfo(() => Funcs.Timestamp()),
                    (c) => FunctionHandlers.FuncsMethod(c, "Timestamp", "timestamp") },
                { Utilities.GetMethodInfo(() => Funcs.Id<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Id", "id") },
                { Utilities.GetMethodInfo(() => Funcs.Type<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Type", "type") },
                { Utilities.GetMethodInfo(() => Funcs.Properties<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Properties", "properties") },
                { Utilities.GetMethodInfo(() => Funcs.StartNode<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "StartNode", "startNode") },
                { Utilities.GetMethodInfo(() => Funcs.EndNode<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "EndNode", "endNode") },
                #endregion

                #region List Functions
                { Utilities.GetMethodInfo(() => Enumerable.Where<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "Where", "filter") },
                { Utilities.GetMethodInfo(() => Enumerable.Select<object, object>(null, (f) => f)),
                    (c) => FunctionHandlers.CypherListSelector(c, "Select", "extract") },
                { Utilities.GetMethodInfo(() => Enumerable.Aggregate<object>(null, (a, v) => a)), FunctionHandlers.Aggregate },
                { Utilities.GetMethodInfo(() => new List<int>().Contains(0)), FunctionHandlers.CollectionContains },
                //{ Utilities.GetMethodInfo(() => Enumerable.Distinct<object>(null)), VarsHandlers.Distinct },
                { Utilities.GetMethodInfo(() => Enumerable.Concat<object>(null, null)), FunctionHandlers.Concat },
                { Utilities.GetMethodInfo(() => Enumerable.Union<object>(null, null)), FunctionHandlers.Union },
                { Utilities.GetMethodInfo(() => Enumerable.Intersect<object>(null, null)), FunctionHandlers.Intersect },
                { Utilities.GetMethodInfo(() => Funcs.Labels<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Labels", "labels") },
                { Utilities.GetMethodInfo(() => Funcs.Keys<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Keys", "keys") },
                { Utilities.GetMethodInfo(() => Funcs.Nodes<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Nodes", "nodes") },
                { Utilities.GetMethodInfo(() => Funcs.Range(0, 0, 0)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Range", "range") },
                { Utilities.GetMethodInfo(() => Funcs.Relationships<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Relationships", "relationships") },
                { Utilities.GetMethodInfo(() => Funcs.Tail<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Tail", "tail") },
                #endregion

                #region Aggregate Functions
                { Utilities.GetMethodInfo(() => Funcs.Average<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Average", "avg") },
                { Utilities.GetMethodInfo(() => Funcs.Collect<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Collect", "collect") },
                { Utilities.GetMethodInfo(() => Funcs.Count<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Count", "count") },
                { Utilities.GetMethodInfo(() => Funcs.Max<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Max", "max") },
                { Utilities.GetMethodInfo(() => Funcs.Min<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Min", "min") },
                { Utilities.GetMethodInfo(() => Funcs.Sum<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Sum", "sum") },
                { Utilities.GetMethodInfo(() => Funcs.Distinct<object>(null)), FunctionHandlers.Distinct },
                { Utilities.GetMethodInfo(() => Funcs.StandardDeviation<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "StandardDeviation", "stDev") },
                { Utilities.GetMethodInfo(() => Funcs.StandardDeviationP<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "StandardDeviationP", "stDevP") },
                { Utilities.GetMethodInfo(() => Funcs.PercentileCont<object, double>(null, 0.0)),
                    (c) => FunctionHandlers.FuncsMethod(c, "PercentileCont", "percentileCont") },
                { Utilities.GetMethodInfo(() => Funcs.PercentileDisc<object, double>(null, 0.0)),
                    (c) => FunctionHandlers.FuncsMethod(c, "PercentileDisc", "percentileDisc") },
                #endregion

                #region Predicate Functions
                { Utilities.GetMethodInfo(() => Enumerable.All<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "All", "all") },
                { Utilities.GetMethodInfo(() => Enumerable.Any<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "Any", "any") },
                { Utilities.GetMethodInfo(() => Enumerable.Any<object>(null)), 
                    //any without arguments generates: randomVar IN source WHERE randomVar IS NOT NULL
                    FunctionHandlers.EmptyAny },
                { Utilities.GetMethodInfo(() => Funcs.Exists<object>(null)),
                    (c) => FunctionHandlers.FuncsMethod(c, "Exists", "exists")},
                { Utilities.GetMethodInfo(() => Enumerable.Contains<object>(null, null)), //IN
                    FunctionHandlers.EnumerableContains },
                { Utilities.GetMethodInfo(() => Funcs.None<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "None", "none", Defaults.FuncsType) },
                { Utilities.GetMethodInfo(() => Cypher.Functions.ExtensionFuncs.None<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "None", "none", Defaults.ExtensionFuncsType) },
                { Utilities.GetMethodInfo(() => Funcs.Single<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "Single", "single", Defaults.FuncsType) },
                { Utilities.GetMethodInfo(() => Cypher.Functions.ExtensionFuncs.Single<object>(null, (f) => true)),
                    (c) => FunctionHandlers.CypherListPredicate(c, "Single", "single", Defaults.ExtensionFuncsType) },
                #endregion

                #region Mathematical Functions
                //numeric
                { Utilities.GetMethodInfo(() => Math.Abs(0.0)), (c) => FunctionHandlers.MathMethod(c, "Abs", "abs") },
                { Utilities.GetMethodInfo(() => Math.Ceiling(0.0)), (c) => FunctionHandlers.MathMethod(c, "Ceiling", "ceil") },
                { Utilities.GetMethodInfo(() => Math.Floor(0.0)), (c) => FunctionHandlers.MathMethod(c, "Floor", "floor") },
                { Utilities.GetMethodInfo(() => Funcs.Rand()), (c) => FunctionHandlers.FuncsMethod(c, "Rand", "rand") },
                { Utilities.GetMethodInfo(() => Math.Round(0.0)), (c) => FunctionHandlers.MathMethod(c, "Round", "round") },
                { Utilities.GetMethodInfo(() => Math.Sign(0.0)), (c) => FunctionHandlers.MathMethod(c, "Sign", "sign") },

                //logarithmic
                { Utilities.GetMethodInfo(() => Funcs.E(0.0)), (c) => FunctionHandlers.FuncsMethod(c, "E", "e") },
                { Utilities.GetMethodInfo(() => Math.Exp(0.0)), (c) => FunctionHandlers.MathMethod(c, "Exp", "exp") },
                { Utilities.GetMethodInfo(() => Math.Log(0.0)), (c) => FunctionHandlers.MathMethod(c, "Log", "log") },
                { Utilities.GetMethodInfo(() => Math.Log10(0.0)), (c) => FunctionHandlers.MathMethod(c, "Log10", "log10") },
                { Utilities.GetMethodInfo(() => Math.Sqrt(0.0)), (c) => FunctionHandlers.MathMethod(c, "Sqrt", "sqrt") },

                //trigonometric
                { Utilities.GetMethodInfo(() => Math.Acos(0.0)), (c) => FunctionHandlers.MathMethod(c, "Acos", "acos") },
                { Utilities.GetMethodInfo(() => Math.Asin(0.0)), (c) => FunctionHandlers.MathMethod(c, "Asin", "asin") },
                { Utilities.GetMethodInfo(() => Math.Atan(0.0)), (c) => FunctionHandlers.MathMethod(c, "Atan", "atan") },
                { Utilities.GetMethodInfo(() => Math.Atan2(0.0, 0.0)), (c) => FunctionHandlers.MathMethod(c, "Atan2", "atan2") },
                { Utilities.GetMethodInfo(() => Math.Cos(0.0)), (c) => FunctionHandlers.MathMethod(c, "Cos", "cos") },
                { Utilities.GetMethodInfo(() => Funcs.Cot(0.0)), (c) => FunctionHandlers.FuncsMethod(c, "Cot", "cot") },
                { Utilities.GetMethodInfo(() => Funcs.Degrees(0.0)), (c) => FunctionHandlers.FuncsMethod(c, "Degrees", "degrees") },
                { Utilities.GetMethodInfo(() => Funcs.HalfVersine(0.0)), (c) => FunctionHandlers.FuncsMethod(c, "HalfVersine", "haversin") },
                { Utilities.GetMethodInfo(() => Funcs.PI(0.0)), (c) => FunctionHandlers.FuncsMethod(c, "PI", "pi") },
                { Utilities.GetMethodInfo(() => Funcs.Radians(0.0)), (c) => FunctionHandlers.FuncsMethod(c, "Radians", "radians") },
                { Utilities.GetMethodInfo(() => Math.Sin(0.0)), (c) => FunctionHandlers.MathMethod(c, "Sin", "sin") },
                { Utilities.GetMethodInfo(() => Math.Tan(0.0)), (c) => FunctionHandlers.MathMethod(c, "Tan", "tan") },
                #endregion

                #region Spatial Functions

                #endregion
            };

        public static Dictionary<ExpressionType, List<Func<FunctionHandlerContext, Func<Expression>>>>
            DefaultNodeTypeHandlers = new Dictionary<ExpressionType,
                List<Func<FunctionHandlerContext, Func<Expression>>>>()
            {
                { ExpressionType.Constant, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.ConstantNodeType } },
                { ExpressionType.Parameter, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.ParameterNodeType } },
                { ExpressionType.Coalesce, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.CoalesceNodeType } },
                { ExpressionType.Equal, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.NullOperandEqualNodeType } },
                { ExpressionType.NotEqual, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.NullOperandNotEqualNodeType } },
                { ExpressionType.ArrayIndex, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.ArrayIndexNodeType } },
                { ExpressionType.ArrayLength, new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.ArrayLengthNodeType } },
            };

        public static Dictionary<Type, List<Func<FunctionHandlerContext, Func<Expression>>>>
            DefaultTypeHandlers = new Dictionary<Type, List<Func<FunctionHandlerContext, Func<Expression>>>>()
            {
                { typeof(UnaryExpression), new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.UnaryType } },
                { typeof(BinaryExpression), new List<Func<FunctionHandlerContext, Func<Expression>>>() { FunctionHandlers.BinaryType } },
            };

        /// <summary>
        /// Add more as you need, as long as it is binary
        /// </summary>
        public static Dictionary<ExpressionType, string> DefaultBinaryOperators { get; }
            = new Dictionary<ExpressionType, string>()
        {
            //boolean
            { ExpressionType.AndAlso, "AND" },
            { ExpressionType.OrElse, "OR" },
            { ExpressionType.ExclusiveOr, "XOR" }, //a work around for the missing conditional XOR in C#
            { ExpressionType.Equal, "=" },
            { ExpressionType.NotEqual, "<>" },
            { ExpressionType.GreaterThan, ">" },
            { ExpressionType.GreaterThanOrEqual, ">=" },
            { ExpressionType.LessThan, "<" },
            { ExpressionType.LessThanOrEqual, "<=" },
            //arithmetic
            { ExpressionType.Add, "+" },
            { ExpressionType.AddChecked, "+" },
            { ExpressionType.Subtract, "-" },
            { ExpressionType.SubtractChecked, "-" },
            { ExpressionType.Multiply, "*" },
            { ExpressionType.MultiplyChecked, "*" },
            { ExpressionType.Divide, "/" },
            { ExpressionType.Modulo, "%" },
            { ExpressionType.Power, "^" }, //for VB users
        };

        /// <summary>
        /// Add more as you need, as long as it is left-aligned unary i.c., the operator appears on the left of the operand. c.g. NOT operand
        /// </summary>
        public static Dictionary<ExpressionType, string> DefaultLeftAlignedUnaryOperators { get; }
            = new Dictionary<ExpressionType, string>()
        {
            //boolean
            { ExpressionType.Not, "NOT" },
        };

        /// <summary>
        /// Add more as you need, as long as it is right-aligned unary i.c., the operator appears on the right of the operand. c.g. operand?
        /// Usually right-aligned operators do not have spaces between them and their operands, so no space inputed here.
        /// </summary>
        public static Dictionary<ExpressionType, string> DefaultRightAlignedUnaryOperators { get; }
            = new Dictionary<ExpressionType, string>();
    }
}
