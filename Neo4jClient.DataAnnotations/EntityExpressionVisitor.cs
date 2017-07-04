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
        public List<List<Expression>> Params { get; } = new List<List<Expression>>();

        public List<List<object>> ParamsPaths { get; } = new List<List<object>>();

        Func<object, string> serializer;

        Expression rootNode;

        public EntityExpressionVisitor(Func<object, string> serializer)
        {
            this.serializer = serializer;
        }


        public override Expression Visit(Expression node)
        {
            if (rootNode == null)
                rootNode = node;

            List<Expression> filtered = null;

            if ((filtered = Utilities.GetValidSimpleAccessStretch(node)) != null
                && Utilities.HasParams(filtered))
            {
                //found our params call.
                //store and replace with marker
                Params.Add(filtered);

                var getParamsExpr = Expression.Call(typeof(Utilities), "GetParams", new[] { node.Type }, Expression.Constant(Params.Count - 1));

                ParamsPaths.Add(new List<object>() { getParamsExpr });

                return getParamsExpr;
            }

            return base.Visit(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (rootNode == node)
            {
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
                                var expandedArgs = Utilities.ExpandComplexTypeAccess(arguments[0], out var argPaths);

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
                                            memberName = $"{baseMemberName}_{argPaths[i].First().Name}";
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
                        return VisitListInit(dictExpr);
                    }
                }
            }

            var index = ParamsPaths.Count; //do this to know which Paths to add to

            var newNode = base.VisitNew(node);

            if (newNode.Type.IsAnonymousType())
            {
                AddToParamsPaths(newNode, index);
            }

            return newNode;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitNewArray(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitListInit(node);
            AddToParamsPaths(newNode, index);

            return newNode;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberInit(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            //we can only handle unescaped assignment bindings
            if (!Utilities.HasNfpEscape(node.Expression))
            {
                //check if its complex and resolve in the possible best way.
                var expanded = Utilities.ExpandComplexTypeAccess(node.Expression, out var paths);

                if (expanded.Count > 0)
                {
                    //create child bindings for the complex type scalars

                    //first sort the paths
                    paths.ForEach((p) => p.Reverse());

                    Dictionary<MemberInfo, List<MemberInfo>> combindPath
                        = new Dictionary<MemberInfo, List<MemberInfo>>();

                    foreach (var path in paths)
                    {
                        foreach (var member in path)
                        {
                            if (!combindPath.TryGetValue(member, out var list))
                            {
                                list = new List<MemberInfo>();
                                combindPath[member] = list;
                            }

                            var addition = path.Skip(path.IndexOf(member) + 1).FirstOrDefault();
                            if (addition != null)
                                list.Add(addition);
                        }
                    }

                    var newExpr = Expression.New(node.Expression.Type);
                    var init = Expression.MemberInit(newExpr, GetPathBindings(combindPath.Keys, combindPath, expanded));

                    node = Expression.Bind(node.Member, init); //replace the node with a new one
                }
            }

            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberAssignment(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberListBinding(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitMemberMemberBinding(node);
            AddToParamsPaths(newNode, index);
            return newNode;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            var index = ParamsPaths.Count; //do this to know which Paths to add to
            var newNode = base.VisitElementInit(node);
            AddToParamsPaths(newNode, index);
            return newNode;
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

        private List<MemberBinding> GetPathBindings(IEnumerable<MemberInfo> members,
            Dictionary<MemberInfo, List<MemberInfo>> combinedPath, List<Expression> expressions)
        {
            List<MemberBinding> bindings = new List<MemberBinding>();

            foreach (var member in members)
            {
                MemberBinding binding = null;

                var children = combinedPath[member];

                if (children == null || children.Count == 0)
                {
                    binding = Expression.Bind(member,
                        expressions.FirstOrDefault(ex =>
                        member.IsEquivalentTo((ex as MemberExpression).Member)));
                }

                if (binding == null)
                {
                    var childBindings = GetPathBindings(children, combinedPath, expressions);

                    var propInfo = member as PropertyInfo;
                    var fieldInfo = member as FieldInfo;

                    if (propInfo?.CanWrite == true || fieldInfo != null)
                    {
                        //assign a new instance
                        try
                        {
                            binding = Expression.Bind(member, Expression.MemberInit
                            (Expression.New(propInfo?.PropertyType ?? fieldInfo.FieldType), childBindings));
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    if (binding == null)
                    {
                        //maybe readonly property.
                        //take a wild shot at membermemberbinding
                        //if it then fails (90%), user must change his code.

                        binding = Expression.MemberBind(member, childBindings);
                    }
                }

                bindings.Add(binding);
            }

            return bindings;
        }

        private void AddToParamsPaths(object node, int index)
        {
            for (int i = index, l = ParamsPaths.Count; i < l; i++)
            {
                ParamsPaths[i].Add(node);
            }
        }
    }
}
