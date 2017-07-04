using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations
{
    //
    public class EntityExpressionVisitor : ExpressionVisitor
    {
        Dictionary<MemberInfo, Expression> memberAssignmentMap 
            = new Dictionary<MemberInfo, Expression>();

        Dictionary<MemberInfo, Tuple<int, ElementInit>> memberListMap
             = new Dictionary<MemberInfo, Tuple<int, ElementInit>>();

        List<MemberBinding> memberBindings = new List<MemberBinding>();

        Dictionary<object, Expression> dictionaryInit 
            = new Dictionary<object, Expression>();

        Dictionary<MemberInfo, Expression> newTypeArguments
            = new Dictionary<MemberInfo, Expression>();

        object lambdaResult = null;

        bool foundItems = false;

        Func<object, string> serializer;

        public EntityExpressionVisitor(Func<object, string> serializer)
        {
            this.serializer = serializer;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (!foundItems)
            {
                foundItems = true;

                var bindings = node.Bindings;
                if (bindings?.Count > 0)
                {
                    //harvest all members
                    memberBindings.AddRange(bindings);

                    //create new member expression and return
                    return Expression.MemberInit(node.NewExpression);
                }
            }

            return base.VisitMemberInit(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (!foundItems)
            {
                foundItems = true;

                if (node.Type.IsAnonymousType())
                {
                    if (node.Members?.Count > 0)
                    {
                        //transform the anonymous type expression to a dictionary expression
                        var dictType = typeof(Dictionary<string, object>);
                        List<ElementInit> dictItems = new List<ElementInit>();
                        MethodInfo dictAddMethod = Utilities.GetMethodInfo(() => new Dictionary<string, object>().Add(null, null));

                        //handle complex members
                        for (int i = 0; i < node.Arguments.Count; i++)
                        {
                            //expand members if complex type
                            var arguments = new List<Expression>() { node.Arguments[i] };
                            var members = new List<string>() { node.Members[i].Name };

                            bool hasNfpEscape = Utilities.HasNfpEscape(arguments[0]);

                            if (!hasNfpEscape)
                            {
                                bool originallyMemberAccess = arguments[0].NodeType == ExpressionType.MemberAccess
                                    && (arguments[0] as MemberExpression).Member.Name == members[0];

                                bool isComplexType = false;

                                var baseMemberName = members[0];

                                //check if its complex and resolve in the possible best way.
                                var expandedArgs = Utilities.ExpandComplexTypeAccess(arguments[0], out var argMembers);

                                if (expandedArgs.Count > 0)
                                {
                                    //has complex members
                                    //use the scalars instead
                                    arguments = expandedArgs;
                                    isComplexType = true;
                                }

                                members.Clear();

                                for (int j = 0, jl = arguments.Count; j < jl; j++)
                                {
                                    var argument = arguments[i];

                                    string memberName = null;

                                    if (originallyMemberAccess)
                                    {
                                        //try find the actual json name
                                        var newMemberName = GetAnonymousMemberName(argument);
                                        memberName = !string.IsNullOrWhiteSpace(newMemberName) ? newMemberName : null;
                                    }

                                    if (memberName == null)
                                    {
                                        if (isComplexType)
                                        {
                                            //use appended name if complex type
                                            memberName = $"{baseMemberName}_{argMembers[i].Name}";
                                        }
                                        else
                                        {
                                            //just use base name
                                            memberName = baseMemberName;
                                        }
                                    }

                                    members.Add(memberName);
                                }
                            }

                            //generate the expression entry
                            for (int j = 0, jl = arguments.Count; j < jl; j++)
                            {
                                var argument = arguments[i];
                                var member = members[i];

                                if (argument != null && member != null)
                                {
                                    dictItems.Add(Expression.ElementInit
                                        (dictAddMethod, Expression.Constant(member), 
                                        Expression.Convert(argument, Defaults.ObjectType)));
                                }
                            }
                        }

                        //generate dictionary expression
                        var dictExpr = Expression.ListInit(Expression.New(dictType), dictItems);

                        return dictExpr;
                    }
                }
            }

            return base.VisitNew(node);
        }

        //protected override Expression VisitNew(NewExpression node)
        //{
        //    if (!foundItems)
        //    {
        //        foundItems = true;

        //        if (node.Type.IsAnonymousType())
        //        {
        //            if (node.Members?.Count > 0)
        //            {
        //                for (int i = 0; i < node.Arguments.Count; i++)
        //                {
        //                    //expand members if complex type
        //                    var arg = node.Arguments[i];
        //                    var member = node.Members[i];

        //                    memberValueMap[member] = arg; //harvest all arguments so we can get an object out first
        //                }

        //                return Expression.New(node.Constructor,
        //                    node.Arguments
        //                    .Select(a => memberValueMap.Values.Contains(a) ?
        //                    Expression.Constant(a.Type.GetDefaultValue(), a.Type) : a),
        //                    node.Members);
        //            }
        //        }
        //    }

        //    return base.VisitNew(node);
        //}

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return base.VisitListInit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda(node);
        }

        private void SortBinding(MemberBinding binding, out MemberBinding newBinding)
        {
            newBinding = null;

            Type type = (binding.Member as PropertyInfo)?.PropertyType ??
                (binding.Member as FieldInfo)?.FieldType ??
                (binding.Member as MethodInfo)?.ReturnType;

            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    {
                        var assignment = binding as MemberAssignment;
                        memberAssignmentMap[binding.Member] = assignment.Expression;

                        binding = Expression.Bind(binding.Member, Expression.Constant(type.GetDefaultValue(), type));
                        break;
                    }
                case MemberBindingType.ListBinding:
                    {
                        var listBinding = binding as MemberListBinding;
                        for (int i = 0, l = listBinding.Initializers.Count; i < l; i++)
                        {
                            var init = listBinding.Initializers[i];
                            memberListMap[binding.Member] = new Tuple<int, ElementInit>(i, init);
                        }

                        binding = Expression.ListBind(binding.Member); //no list bindings
                        break;
                    }
                case MemberBindingType.MemberBinding:
                    {
                        var memberBinding = binding as MemberMemberBinding;
                        break;
                    }
            }
        }

        private string GetAnonymousMemberName(Expression argument)
        {
            string name = null;

            var retrievedExprs = Utilities.GetValidSimpleAccessStretch(argument);

            //get the access stretch without method call
            var subRetrieved = retrievedExprs.AsEnumerable().Reverse().TakeWhile(ex =>
            {
                switch (ex.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.TypeAs:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Unbox:
                        {
                            return true;
                        }
                }

                return false;
            }).Reverse().FirstOrDefault();

            //get the entity object
            var entityExpr = (subRetrieved as MemberExpression)?.Expression ??
                (subRetrieved as UnaryExpression)?.Operand;

            object entity = null;

            try
            {
                entity = entityExpr.ExecuteExpression<object>();
            }
            catch
            {
                //something went wrong.
                //that shouldn't deter us now to get memberName
                //try activating manually

                try
                {
                    entity = Activator.CreateInstance(entityExpr.Type);
                }
                catch
                {

                }
            }

            if (entity != null)
            {
                //get the names
                var currentIndex = retrievedExprs.IndexOf(subRetrieved);
                var memberNames = Utilities.GetEntityPathNames
                    (entity, retrievedExprs, ref currentIndex, serializer,
                    out var entityMembers, out var lastType, useResolvedJsonName: true);

                name = memberNames?.LastOrDefault(); //we are only interested in the last member name.
            }

            return name;
        }
    }
}
