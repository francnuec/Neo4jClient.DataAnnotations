using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public partial class FunctionExpressionVisitor : ExpressionVisitor
    {
        public static Dictionary<MemberInfo, Func<FunctionHandlerContext, Func<Expression>>>
            DefaultMemberInfoHandlers = new Dictionary<MemberInfo, Func<FunctionHandlerContext, Func<Expression>>>
            {
                {Utils.Utilities.GetMethodInfo(() => CypherVariables.Get(null)), FunctionHandlers.GetVariable},
                {Defaults.CypherObjectIndexerInfo, FunctionHandlers.CypherObjectIndexer},
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.ElementAt<object>(null, 0)),
                    FunctionHandlers.ElementAt
                },
                {Utils.Utilities.GetMethodInfo(() => Math.Pow(0, 0)), FunctionHandlers.MathPower},
                {Utils.Utilities.GetMethodInfo(() => CypherFunctions.IsNull<object>(null)), FunctionHandlers.IsNull},
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.IsNotNull<object>(null)),
                    FunctionHandlers.IsNotNull
                },
                //{ Utils.Utilities.GetMethodInfo(() => ObjectExtensions._AsRaw(null)), FunctionHandlers._AsRaw },
                {Defaults.NfpExtMethodInfo, FunctionHandlers.NoFurtherProcessing},

                #region ICypherResultItem Functions

                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).As<object>()), c =>
                        FunctionHandlers.ICypherResultItemMethod(c, "As", "", (m, cc) =>
                        {
                            //This should never be executed as "As" method should never have been called here.
                            //But just in case, we provide a simple implementation
                            cc.Continuation();
                            //then ignore current node before processing unhandled nodes
                            cc.Visitor.Ignore(m);
                            cc.Visitor.ProcessUnhandledSimpleVars(m);
                            return m;
                        })
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Id()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Id", "id")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).CollectAs<object>()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "CollectAs", "collect")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).CollectAsDistinct<object>()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "CollectAsDistinct", "collect", (m, cc) =>
                    {
                        cc.Visitor.Builder.Append("DISTINCT ", m);
                        cc.Continuation();

                        return cc.Expression;
                    })
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Count()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Count", "count")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).CountDistinct()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "CountDistinct", "count", (m, cc) =>
                    {
                        cc.Visitor.Builder.Append("DISTINCT ", m);
                        cc.Continuation();

                        return cc.Expression;
                    })
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Head()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Head", "head")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Labels()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Labels", "labels")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Last()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Last", "last")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Length()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Length", "length")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => (null as ICypherResultItem).Type()),
                    c => FunctionHandlers.ICypherResultItemMethod(c, "Type", "type")
                },

                #endregion

                #region String Functions

                {Utils.Utilities.GetMethodInfo(() => new object().ToString()), FunctionHandlers.ToString},
                {Utils.Utilities.GetPropertyInfo(() => "".Length), FunctionHandlers.StringLength},
                {
                    Utils.Utilities.GetMethodInfo(() => "".ToLower()),
                    c => FunctionHandlers.StringMethod(c, "ToLower", "toLower")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".ToUpper()),
                    c => FunctionHandlers.StringMethod(c, "ToUpper", "toUpper")
                },
                {Utils.Utilities.GetMethodInfo(() => "".Trim()), c => FunctionHandlers.StringMethod(c, "Trim", "trim")},
                {
                    Utils.Utilities.GetMethodInfo(() => "".TrimStart()),
                    c => FunctionHandlers.StringMethod(c, "TrimStart", "lTrim")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".TrimEnd()),
                    c => FunctionHandlers.StringMethod(c, "TrimEnd", "rTrim")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".Replace(null, null)),
                    c => FunctionHandlers.StringMethod(c, "Replace", "replace", FunctionHandlers.StringReplaceBody)
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".Substring(0)),
                    c => FunctionHandlers.StringMethod(c, "Substring", "substring",
                        FunctionHandlers.StringSubstringBody)
                },
                {Utils.Utilities.GetMethodInfo(() => Enumerable.Reverse<object>(null)), FunctionHandlers.Reverse},
                {
                    Utils.Utilities.GetMethodInfo(() => "".StartsWith(null)),
                    c => FunctionHandlers.StringComparison(c, "StartsWith", "STARTS WITH")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".EndsWith(null)),
                    c => FunctionHandlers.StringComparison(c, "EndsWith", "ENDS WITH")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".Contains(null)),
                    c => FunctionHandlers.StringComparison(c, "Contains", "CONTAINS")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => "".Split(null)),
                    c => FunctionHandlers.StringMethod(c, "Split", "split", FunctionHandlers.StringSplitBody)
                },

                #endregion

                #region Scalar Functions

                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.First<object>(null)),
                    c => FunctionHandlers.EnumerableMethod(c, "First", "head")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.FirstOrDefault<object>(null)),
                    c => FunctionHandlers.EnumerableMethod(c, "FirstOrDefault", "head")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Last<object>(null)),
                    c => FunctionHandlers.EnumerableMethod(c, "Last", "last")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.LastOrDefault<object>(null)),
                    c => FunctionHandlers.EnumerableMethod(c, "LastOrDefault", "last")
                },
                {Utils.Utilities.GetPropertyInfo(() => new List<int>().Count), FunctionHandlers.CollectionCount},
                {
                    Utils.Utilities.GetMethodInfo(() => Convert.ToInt32(null)),
                    c => FunctionHandlers.ConvertMethod(c, "ToInt32", "toInteger")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Convert.ToSingle(null)),
                    c => FunctionHandlers.ConvertMethod(c, "ToSingle", "toFloat")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Convert.ToBoolean(null)),
                    c => FunctionHandlers.ConvertMethod(c, "ToBoolean", "toBoolean")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Length<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Length", "length")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Size<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Size", "size")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Timestamp()),
                    c => FunctionHandlers.FuncsMethod(c, "Timestamp", "timestamp")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Id<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Id", "id")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Type<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Type", "type")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Properties<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Properties", "properties")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.StartNode<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "StartNode", "startNode")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.EndNode<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "EndNode", "endNode")
                },

                #endregion

                #region List Functions

                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Where<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "Where", "filter")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Select<object, object>(null, f => f)),
                    c => FunctionHandlers.CypherListSelector(c, "Select", "extract")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Aggregate<object>(null, (a, v) => a)),
                    FunctionHandlers.Aggregate
                },
                {Utils.Utilities.GetMethodInfo(() => new List<int>().Contains(0)), FunctionHandlers.CollectionContains},
                //{ Utilities.GetMethodInfo(() => Enumerable.Distinct<object>(null)), VarsHandlers.Distinct },
                {Utils.Utilities.GetMethodInfo(() => Enumerable.Concat<object>(null, null)), FunctionHandlers.Concat},
                {Utils.Utilities.GetMethodInfo(() => Enumerable.Union<object>(null, null)), FunctionHandlers.Union},
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Intersect<object>(null, null)),
                    FunctionHandlers.Intersect
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Labels<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Labels", "labels")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Keys<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Keys", "keys")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Nodes<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Nodes", "nodes")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Range(0, 0, 0)),
                    c => FunctionHandlers.FuncsMethod(c, "Range", "range")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Relationships<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Relationships", "relationships")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Tail<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Tail", "tail")
                },

                #endregion

                #region Aggregate Functions

                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Average<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Average", "avg")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Collect<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Collect", "collect")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Count<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Count", "count")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Max<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Max", "max")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Min<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Min", "min")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Sum<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Sum", "sum")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Distinct<object>(null)),
                    FunctionHandlers.Distinct
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.StandardDeviation<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "StandardDeviation", "stDev")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.StandardDeviationP<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "StandardDeviationP", "stDevP")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.PercentileCont<object, double>(null, 0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "PercentileCont", "percentileCont")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.PercentileDisc<object, double>(null, 0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "PercentileDisc", "percentileDisc")
                },

                #endregion

                #region Predicate Functions

                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.All<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "All", "all")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Any<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "Any", "any")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Any<object>(null)),
                    //any without arguments generates: randomVar IN source WHERE randomVar IS NOT NULL
                    FunctionHandlers.EmptyAny
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Exists<object>(null)),
                    c => FunctionHandlers.FuncsMethod(c, "Exists", "exists")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Enumerable.Contains<object>(null, null)), //IN
                    FunctionHandlers.EnumerableContains
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.None<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "None", "none", Defaults.CypherFuncsType)
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherExtensionFunctions.None<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "None", "none", Defaults.CypherExtensionFuncsType)
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Single<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "Single", "single", Defaults.CypherFuncsType)
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherExtensionFunctions.Single<object>(null, f => true)),
                    c => FunctionHandlers.CypherListPredicate(c, "Single", "single", Defaults.CypherExtensionFuncsType)
                },

                #endregion

                #region Mathematical Functions

                //numeric
                {Utils.Utilities.GetMethodInfo(() => Math.Abs(0.0)), c => FunctionHandlers.MathMethod(c, "Abs", "abs")},
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Ceiling(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Ceiling", "ceil")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Floor(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Floor", "floor")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Rand()),
                    c => FunctionHandlers.FuncsMethod(c, "Rand", "rand")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Round(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Round", "round")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Sign(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Sign", "sign")
                },

                //logarithmic
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.E(0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "E", "e")
                },
                {Utils.Utilities.GetMethodInfo(() => Math.Exp(0.0)), c => FunctionHandlers.MathMethod(c, "Exp", "exp")},
                {Utils.Utilities.GetMethodInfo(() => Math.Log(0.0)), c => FunctionHandlers.MathMethod(c, "Log", "log")},
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Log10(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Log10", "log10")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Sqrt(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Sqrt", "sqrt")
                },

                //trigonometric
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Acos(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Acos", "acos")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Asin(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Asin", "asin")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Atan(0.0)),
                    c => FunctionHandlers.MathMethod(c, "Atan", "atan")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => Math.Atan2(0.0, 0.0)),
                    c => FunctionHandlers.MathMethod(c, "Atan2", "atan2")
                },
                {Utils.Utilities.GetMethodInfo(() => Math.Cos(0.0)), c => FunctionHandlers.MathMethod(c, "Cos", "cos")},
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Cot(0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "Cot", "cot")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Degrees(0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "Degrees", "degrees")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Haversin(0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "Haversin", "haversin")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.PI(0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "PI", "pi")
                },
                {
                    Utils.Utilities.GetMethodInfo(() => CypherFunctions.Radians(0.0)),
                    c => FunctionHandlers.FuncsMethod(c, "Radians", "radians")
                },
                {Utils.Utilities.GetMethodInfo(() => Math.Sin(0.0)), c => FunctionHandlers.MathMethod(c, "Sin", "sin")},
                {Utils.Utilities.GetMethodInfo(() => Math.Tan(0.0)), c => FunctionHandlers.MathMethod(c, "Tan", "tan")},

                #endregion

                #region Spatial Functions

                #endregion
            };

        public static Dictionary<ExpressionType, List<Func<FunctionHandlerContext, Func<Expression>>>>
            DefaultNodeTypeHandlers = new Dictionary<ExpressionType,
                List<Func<FunctionHandlerContext, Func<Expression>>>>
            {
                {
                    ExpressionType.MemberInit,
                    new List<Func<FunctionHandlerContext, Func<Expression>>>
                        {FunctionHandlers.New_Dictionary_MemberInitType}
                },
                {
                    ExpressionType.ListInit,
                    new List<Func<FunctionHandlerContext, Func<Expression>>>
                        {FunctionHandlers.New_Dictionary_MemberInitType}
                },
                {
                    ExpressionType.New,
                    new List<Func<FunctionHandlerContext, Func<Expression>>>
                        {FunctionHandlers.New_Dictionary_MemberInitType}
                },
                {
                    ExpressionType.Constant,
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.ConstantNodeType}
                },
                {
                    ExpressionType.Parameter,
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.ParameterNodeType}
                },
                {
                    ExpressionType.Coalesce,
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.CoalesceNodeType}
                },
                {
                    ExpressionType.Equal,
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.NullOperandEqualNodeType}
                },
                {
                    ExpressionType.NotEqual,
                    new List<Func<FunctionHandlerContext, Func<Expression>>>
                        {FunctionHandlers.NullOperandNotEqualNodeType}
                },
                {
                    ExpressionType.ArrayIndex,
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.ArrayIndexNodeType}
                },
                {
                    ExpressionType.ArrayLength,
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.ArrayLengthNodeType}
                }
            };

        public static Dictionary<Type, List<Func<FunctionHandlerContext, Func<Expression>>>>
            DefaultTypeHandlers = new Dictionary<Type, List<Func<FunctionHandlerContext, Func<Expression>>>>
            {
                {
                    typeof(UnaryExpression),
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.UnaryType}
                },
                {
                    typeof(BinaryExpression),
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.BinaryType}
                },
                {
                    typeof(NewArrayExpression),
                    new List<Func<FunctionHandlerContext, Func<Expression>>> {FunctionHandlers.NewArrayType}
                }
            };

        /// <summary>
        ///     Add more as you need, as long as it is binary
        /// </summary>
        public static Dictionary<ExpressionType, string> DefaultBinaryOperators { get; }
            = new Dictionary<ExpressionType, string>
            {
                //boolean
                {ExpressionType.AndAlso, "AND"},
                {ExpressionType.OrElse, "OR"},
                {ExpressionType.ExclusiveOr, "XOR"}, //a work around for the missing conditional XOR in C#
                {ExpressionType.Equal, "="},
                {ExpressionType.NotEqual, "<>"},
                {ExpressionType.GreaterThan, ">"},
                {ExpressionType.GreaterThanOrEqual, ">="},
                {ExpressionType.LessThan, "<"},
                {ExpressionType.LessThanOrEqual, "<="},
                //arithmetic
                {ExpressionType.Add, "+"},
                {ExpressionType.AddChecked, "+"},
                {ExpressionType.Subtract, "-"},
                {ExpressionType.SubtractChecked, "-"},
                {ExpressionType.Multiply, "*"},
                {ExpressionType.MultiplyChecked, "*"},
                {ExpressionType.Divide, "/"},
                {ExpressionType.Modulo, "%"},
                {ExpressionType.Power, "^"} //for VB users
            };

        /// <summary>
        ///     Add more as you need, as long as it is left-aligned unary i.c., the operator appears on the left of the operand.
        ///     c.g. NOT operand
        /// </summary>
        public static Dictionary<ExpressionType, string> DefaultLeftAlignedUnaryOperators { get; }
            = new Dictionary<ExpressionType, string>
            {
                //boolean
                {ExpressionType.Not, "NOT"}
            };

        /// <summary>
        ///     Add more as you need, as long as it is right-aligned unary i.c., the operator appears on the right of the operand.
        ///     c.g. operand?
        ///     Usually right-aligned operators do not have spaces between them and their operands, so no space inputed here.
        /// </summary>
        public static Dictionary<ExpressionType, string> DefaultRightAlignedUnaryOperators { get; }
            = new Dictionary<ExpressionType, string>();
    }
}