using Neo4jClient.DataAnnotations.Expressions;
using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Cypher.Extensions;
using Neo4jClient.Serialization;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class CypherUtilities
    {
        public static JObject GetFinalProperties(
            LambdaExpression lambdaExpr, QueryContext queryContext,
            out bool hasFunctionsInProperties)
        {
            hasFunctionsInProperties = false;

            //get the properties expression
            if (lambdaExpr != null && queryContext.SerializeCallback != null)
            {
                var resolver = queryContext.Resolver;
                var serializer = queryContext.SerializeCallback;
                var annotationsContext = queryContext.AnnotationsContext;
                var entityService = annotationsContext.EntityService;

                //visit the expressions
                var entityVisitor = new EntityExpressionVisitor(queryContext);

                var instanceExpr = entityVisitor.Visit(lambdaExpr.Body);
                var predicateExpr = entityVisitor.SetPredicateNode;
                var predicateMemberAssignments = entityVisitor.SetPredicateMemberAssignments;
                var predicateDictionaryAssignments = entityVisitor.SetPredicateDictionaryAssignments;
                var usePredicateOnly = entityVisitor.SetUsePredicateOnly;

                //get the instance
                var instance = instanceExpr.ExecuteExpression<object>();

                var instanceType = instance.GetType();
                var instanceIsDictionary = instanceType.IsDictionaryType();
                var sourceIsAnonymous = entityVisitor.Source.Type.IsAnonymousType();
                var rootIsDictionary = entityVisitor.RootNode.Type.IsDictionaryType();

                var instanceInfo = entityService.GetEntityTypeInfo(instanceType);

                var dictMemberNames = entityVisitor.DictMemberNames;
                Dictionary<MemberName, MemberInfo> jsonNamePropertyMap = null;
                Dictionary<object, Expression> predicateAssignments = null;
                List<ComplexAssignmentInfo> predicateComplexAssignments = null;
                string instanceJson = null;

                if (!instanceIsDictionary)
                {
                    if (!instanceType.IsAnonymousType())
                        entityService.AddEntityType(instanceType); //just in case it was omitted

                    if (resolver != null)
                    {
                        instanceInfo.WithJsonResolver(resolver);
                    }
                    else
                    {
                        try
                        {
                            if (usePredicateOnly)
                            {
                                //initialize complex properties in case they were omitted
                                //however, avoid initilizing on behalf of the user unless it's a predicate
                                Utils.Utilities.InitializeComplexTypedProperties(instance, entityService);
                            }

                            //serialize the instance to force converter to enumerate jsonNames
                            instanceJson = serializer(instance);
                        }
                        catch
                        {

                        }
                    }

                    jsonNamePropertyMap = instanceInfo.JsonNamePropertyMap;
                }
                else if (sourceIsAnonymous)
                {
                    //get the mapping from dictionary member names
                    Func<string, DictMemberInfo, string> getMemberActualName = (baseName, value) =>
                    {
                        if (value == null || value.ComplexPath == null || value.ComplexPath.Count == 0)
                            return baseName ?? value.ComplexPath?.FirstOrDefault()?.Name ?? value.JsonName;

                        var aggregate = value.ComplexPath.AsEnumerable().Reverse()
                        .Select(c => c.Name)
                        .Aggregate((c1, c2) => $"{c1}{Defaults.ComplexTypeNameSeparator}{c2}");

                        if (baseName != null)
                            aggregate = $"{baseName}{Defaults.ComplexTypeNameSeparator}{aggregate}";

                        return aggregate;
                    };

                    jsonNamePropertyMap = entityVisitor.DictMemberNames
                        .SelectMany(item => item.Value, (item, value) => new { baseName = item.Key, value })
                        .ToDictionary(tuple => new MemberName(getMemberActualName(tuple.baseName, tuple.value), tuple.value.JsonName),
                        tuple => tuple.value.ComplexPath.FirstOrDefault());
                }

                object predicateInstance = null;
                JObject predicateJObject = null;

                //check if it has a "set" node
                if (predicateExpr != null)
                {
                    if (!usePredicateOnly)
                    {
                        //has a separate predicate instance
                        predicateInstance = predicateExpr.ExecuteExpression<object>();
                    }
                    else
                    {
                        predicateInstance = instance;
                    }

                    if (!instanceIsDictionary)
                    {
                        //initialize complex properties in case they were omitted
                        Utils.Utilities.InitializeComplexTypedProperties(predicateInstance, entityService);
                    }

                    //serialize the predicate
                    var predicateJson = instanceJson != null && predicateInstance == instance ?
                        instanceJson : serializer(predicateInstance);

                    predicateJObject = JObject.Parse(predicateJson);

                    //filter out the predicate assignments
                    var filteredProps = new List<JProperty>();

                    predicateAssignments = new Dictionary<object, Expression>();

                    if (predicateMemberAssignments != null && predicateMemberAssignments.Count > 0)
                    {
                        var assignments = predicateMemberAssignments.ToDictionary(item => (object)item.Key, item => item.Value);

                        foreach (var item in assignments)
                            predicateAssignments.Add(item.Key, item.Value);
                    }

                    if (predicateDictionaryAssignments != null && predicateDictionaryAssignments.Count > 0)
                    {
                        var assignments = predicateDictionaryAssignments.ToDictionary(item => (object)item.Key, item => item.Value);

                        foreach (var item in assignments)
                            predicateAssignments.Add(item.Key, item.Value);
                    }

                    if (predicateAssignments.Count > 0)
                    {
                        //use member assignments
                        //for each member assignment, find the corresponding jsonname, and jsonproperty
                        filteredProps.AddRange(ResolveAssignments
                            (entityService, jsonNamePropertyMap, dictMemberNames,
                            predicateAssignments, predicateJObject, instanceType.Name,
                            out predicateComplexAssignments));
                    }

                    if (filteredProps.Count > 0)
                    {
                        //create new JObject
                        predicateJObject = new JObject();

                        foreach (var prop in filteredProps)
                        {
                            predicateJObject.Add(prop.Name, prop.Value);
                        }
                    }
                }

                //now resolve instance
                JObject instanceJObject = predicateJObject; //we just assume this first

                if (predicateInstance != instance)
                {
                    instanceJson = instanceJson ?? serializer(instance);
                    instanceJObject = JObject.Parse(instanceJson);

                    if (predicateJObject != null)
                    {
                        if (predicateComplexAssignments != null && predicateComplexAssignments.Count > 0)
                        {
                            //these are complex properties of the instanceType that were directly assigned in the predicate and not in the original instance
                            //so remove those expanded properties found on instance but not on predicate
                            //this would usually happen when the complex type assigned on instance is a derived type of the complex type assigned on predicate
                            foreach (var complexAssignment in predicateComplexAssignments)
                            {
                                //get the baseMemberJsonName
                                var itemName = complexAssignment.Properties.First().Name;
                                var sepIdx = itemName.IndexOf(Defaults.ComplexTypeNameSeparator);
                                var baseMemberJsonName = sepIdx > 0 ? itemName.Substring(0, sepIdx) : new string(itemName.ToCharArray());

                                //find all jproperties on instanceJObject starting with this name
                                var complexJProps = instanceJObject.Properties().Where(jp => jp.Name.StartsWith(baseMemberJsonName)).ToArray();

                                if (complexJProps.Length > 0)
                                {
                                    foreach (var complexJProp in complexJProps)
                                    {
                                        if (!complexAssignment.Properties.Any(jp => jp.Name == complexJProp.Name))
                                        {
                                            //candidate for removal, but confirm it isn't an actual property complexly named first
                                            bool dontRemove = jsonNamePropertyMap != null
                                                && jsonNamePropertyMap.FirstOrDefault(jp => jp.Key.Json == complexJProp.Name).Value is PropertyInfo complexPropInfo //.TryGetValue(complexJProp.Name, out var complexPropInfo)
                                                && instanceInfo.AllProperties.Contains(complexPropInfo);

                                            if (!dontRemove && rootIsDictionary && dictMemberNames != null)
                                            {
                                                //check dictionary names
                                                try
                                                {
                                                    if (dictMemberNames.TryGetValue(complexJProp.Name, out var values))
                                                    {
                                                        //key was deliberately set by user in instance dictionary so keep the property
                                                        dontRemove = true;
                                                        break;
                                                    }
                                                }
                                                catch
                                                {

                                                }
                                            }

                                            if (!dontRemove)
                                            {
                                                //remove it
                                                complexJProp.Remove();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //apply the predicate values to instance
                        foreach (var predicateProp in predicateJObject)
                        {
                            instanceJObject[predicateProp.Key] = predicateProp.Value; //should generate exception if the key is not found
                        }
                    }
                }

                //now replace values with neo functions where appropriate
                var functionNodes = entityVisitor.SpecialNodePaths.Where(pair => pair.Node.Type == SpecialNodeType.Function).ToArray();
                if (functionNodes.Length > 0)
                {
                    //for vars:
                    //a member is identified by the last MemberAssignment, or the first argument of an ElementInit of the first Dictionary<string, object>
                    //title: a.title (assigned to a member)
                    //roles: [a.roles[0]] (ElementInit of an assignment to a member)
                    //roles: [a.roles[b.index]] (This scenario is same as previous, except with recursive vars)
                    //in other words, direct assignment, and arrays are supported

                    var propertyKeyToVarNodes = new List<(string PropertyKey, IEnumerable<object> PathsLeft, SpecialNode Node, object ReferenceItem)>();

                    foreach (var varNode in functionNodes)
                    {
                        //object varBuiltValue = varNode.Node.ConcreteValue; //as string;
                        object referenceItem = null;
                        string propertyKey = null;

                        var paths = varNode.Path;

                        MemberAssignment assignment = null;
                        MemberListBinding listBinding = null;
                        ElementInit dictElementInit = null;
                        ListInitExpression dictListInit = null;


                        //from the top of the list, the first memberassignment or memberlistbinding is our guy
                        foreach (var item in paths)
                        {
                            if ((assignment = item as MemberAssignment) != null
                                || (listBinding = item as MemberListBinding) != null)
                            {
                                break;
                            }

                            if (dictElementInit == null)
                                dictElementInit = item as ElementInit;

                            if (dictElementInit != null
                                && (dictListInit = item as ListInitExpression) != null
                                && dictListInit.NewExpression.Type != EntityExpressionVisitor.DictType)
                            {
                                break;
                            }
                        }

                        var memberBinding = assignment ?? (MemberBinding)listBinding;

                        referenceItem = memberBinding ?? (object)dictListInit;

                        if (referenceItem == null)
                        {
                            throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varNode.Node.ConcreteValue /*varBuiltValue*/));
                        }

                        if (memberBinding != null)
                        {
                            //find property key
                            JProperty jProperty = null;
                            try
                            {
                                jProperty = ResolveAssignments
                                    (entityService, jsonNamePropertyMap, dictMemberNames,
                                    new Dictionary<object, Expression>()
                                {
                                    { memberBinding.Member, assignment?.Expression }
                                }, instanceJObject, instanceType.Name, out var complexAssignments).FirstOrDefault();
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varNode.Node.ConcreteValue /*varBuiltValue*/), e);
                            }

                            if (jProperty == null)
                            {
                                throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varNode.Node.ConcreteValue /*varBuiltValue*/));
                            }

                            propertyKey = jProperty.Name;
                            referenceItem = memberBinding;
                        }
                        else if (dictElementInit != null && dictListInit.Initializers.Contains(dictElementInit))
                        {
                            //for dictionaries
                            propertyKey = dictElementInit.Arguments[0].ExecuteExpression<string>();
                            referenceItem = dictListInit;
                        }

                        if (propertyKey == null
                            || (varNode.Node.FoundWhileVisitingPredicate && predicateJObject[propertyKey] == null) //avoid invalid assignments
                            )
                        {
                            //trouble
                            throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, varNode.Node.ConcreteValue /*varBuiltValue*/));
                        }

                        propertyKeyToVarNodes.Add(
                                (propertyKey,
                                paths.Take(paths.IndexOf(referenceItem) + 1),
                                varNode.Node,
                                referenceItem /*varBuiltValue*/));
                    }

                    var funcsVisitor = new FunctionExpressionVisitor(entityVisitor.QueryContext, new FunctionVisitorContext());
                    var containsVisitor = new ContainsExpressionVisitor();
                    var replacerVisitor = new ReplacerExpressionVisitor();

                    foreach (var item in propertyKeyToVarNodes)
                    {
                        funcsVisitor.Clear();
                        containsVisitor.Reset();
                        replacerVisitor.ExpressionReplacements.Clear();

                        //find the value and replace with function where appropriate
                        var key = item.PropertyKey;
                        var pathsLeft = item.PathsLeft.ToArray();
                        var specialNode = item.Node;
                        //var varBuiltValue = item.BuiltValue;

                        var getValueExpr = pathsLeft[0] as MethodCallExpression;

                        var instanceJValue = instanceJObject[key];

                        string finalValue = null; //new JRaw(varBuiltValue);
                        Expression expressionToVisit = null;

                        //value should be one of two things
                        //array or normal literal
                        var enumerator = pathsLeft.GetEnumerator();

                        //test for JArray first
                        if (instanceJValue.Type == JTokenType.Array)
                        {
                            var jArray = instanceJValue as JArray;

                            //find the index of this array to set
                            int index = -1;
                            containsVisitor.Item = getValueExpr;

                            while (enumerator.MoveNext())
                            {
                                var currentItem = enumerator.Current;
                                if (currentItem == getValueExpr)
                                    continue;

                                System.Collections.ObjectModel.ReadOnlyCollection<ElementInit> initializers = null;

                                switch (currentItem)
                                {
                                    case NewArrayExpression arrayExpr:
                                        {
                                            for (int i = 0, l = arrayExpr.Expressions.Count; i < l; i++)
                                            {
                                                var expr = arrayExpr.Expressions[i];

                                                containsVisitor.Visit(expr);
                                                if (containsVisitor.IsContained)
                                                {
                                                    index = i;
                                                    expressionToVisit = expr;
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    case ListInitExpression listInit:
                                        {
                                            initializers = listInit.Initializers;
                                            break;
                                        }
                                    case MemberListBinding memberList:
                                        {
                                            initializers = memberList?.Initializers;
                                            break;
                                        }
                                }

                                if (initializers != null)
                                {
                                    for (int i = 0, l = initializers.Count; i < l; i++)
                                    {
                                        var initializer = initializers[i];

                                        foreach (var expr in initializer.Arguments)
                                        {
                                            containsVisitor.Visit(expr);
                                            if (containsVisitor.IsContained)
                                            {
                                                index = i;
                                                expressionToVisit = expr;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (expressionToVisit != null)
                                    break;
                            }

                            if (index < 0 || index >= jArray.Count)
                            {
                                //yawa don gas :)
                                throw new InvalidOperationException(string.Format(Messages.AmbiguousVarsPathError, specialNode.ConcreteValue));
                            }

                            if (expressionToVisit != null)
                            {
                                //replace the placeholder with the actual value
                                replacerVisitor.ExpressionReplacements.Add(specialNode.Placeholder, specialNode.Node);
                                expressionToVisit = replacerVisitor.Visit(expressionToVisit);
                                //now visit to write out the function
                                funcsVisitor.Visit(expressionToVisit);
                                if (funcsVisitor.Builder.ToString() is var actualValue
                                    && !string.IsNullOrWhiteSpace(actualValue))
                                {
                                    finalValue = actualValue;
                                }
                            }

                            //replace the value
                            jArray[index] = new JRaw(finalValue ?? specialNode.ConcreteValue);
                            hasFunctionsInProperties = true;
                            continue;
                        }
                        else
                        {
                            while (enumerator.MoveNext())
                            {
                                var currentItem = enumerator.Current;
                                if (currentItem == getValueExpr)
                                    continue;

                                if (currentItem is MemberAssignment assignment)
                                {
                                    expressionToVisit = assignment.Expression;
                                    break;
                                }
                            }

                            if (expressionToVisit != null)
                            {
                                //replace the placeholder with the actual value
                                replacerVisitor.ExpressionReplacements.Add(specialNode.Placeholder, specialNode.Node);
                                expressionToVisit = replacerVisitor.Visit(expressionToVisit);
                                //now visit to write out the function
                                funcsVisitor.Visit(expressionToVisit);
                                if (funcsVisitor.Builder.ToString() is var actualValue
                                    && !string.IsNullOrWhiteSpace(actualValue))
                                {
                                    finalValue = actualValue;
                                }
                            }

                            //assign
                            instanceJObject[key] = new JRaw(finalValue ?? specialNode.ConcreteValue);
                            hasFunctionsInProperties = true;
                        }
                    }
                }

                return instanceJObject;
            }

            return null;
        }

        private static string GetMemberJsonNameFromAssignment
            (Dictionary<MemberName, MemberInfo> jsonNamePropertyMap,
            Dictionary<string, List<DictMemberInfo>> dictMemberNames,
            MemberInfo assignmentInfo, string assignmentName,
            MemberInfo actual, string actualName,
            string instanceTypeName)
        {
            string memberJsonName = null;

            if (jsonNamePropertyMap != null)
            {
                var memberJsonNameMap = jsonNamePropertyMap
                .Where(item => item.Value.IsEquivalentTo(actual))
                .FirstOrDefault();

                if (memberJsonNameMap.Value != null)
                    memberJsonName = memberJsonNameMap.Key?.Json;
            }

            if (dictMemberNames != null && string.IsNullOrWhiteSpace(memberJsonName))
            {
                //try dictmembernames
                if (actual != null)
                {
                    var tuple = dictMemberNames.SelectMany(item => item.Value)
                        .FirstOrDefault(item => item.ComplexPath?.FirstOrDefault()?.IsEquivalentTo(actual) == true);

                    if (tuple != null)
                    {
                        memberJsonName = tuple.JsonName;
                    }
                }

                if (string.IsNullOrWhiteSpace(memberJsonName))
                {
                    //still empty
                    //use the name to search dict keys
                    actualName = actualName ?? actual?.Name;

                    if (dictMemberNames.TryGetValue(actualName, out var values)
                        || ((assignmentName = assignmentName ?? assignmentInfo?.Name) != null
                        && dictMemberNames.TryGetValue(assignmentName, out values)))
                    {
                        memberJsonName = values?.FirstOrDefault(v => v.JsonName == actualName)?.JsonName ?? (values?.Count == 1 ? values[0].JsonName : null);
                    }
                }
            }

            return memberJsonName;
        }

        private static JProperty ResolveJPropertyFromAssignment
            (Dictionary<MemberName, MemberInfo> jsonNamePropertyMap,
            Dictionary<string, List<DictMemberInfo>> dictMemberNames,
            JObject jObject, MemberInfo assignmentInfo, string assignmentName,
            MemberInfo actual, string actualName, string instanceTypeName)
        {
            string memberJsonName = GetMemberJsonNameFromAssignment(jsonNamePropertyMap,
                dictMemberNames, assignmentInfo, assignmentName, actual, actualName,
                instanceTypeName);

            if (memberJsonName == null)
            {
                //we have a problem
                throw new Exception(string.Format(Messages.InvalidMemberAssignmentError, assignmentInfo?.Name ?? assignmentName));
            }

            //get the jproperty
            return ResolveJPropertyFromJsonName(jObject, memberJsonName, instanceTypeName);
        }

        private static JProperty ResolveJPropertyFromJsonName
            (JObject jObject, string memberJsonName, string instanceTypeName)
        {
            //get the jproperty
            var jProp = jObject.Properties().FirstOrDefault(jp => jp.Name == memberJsonName);

            if (jProp == null)
            {
                //another problem
                throw new Exception(string.Format(Messages.JsonPropertyNotFoundError, memberJsonName, instanceTypeName));
            }

            return jProp;
        }

        private static Dictionary<string, List<DictMemberInfo>> filterDictionaryMap 
            (string _baseActualName, string _baseJsonName, 
            Dictionary<string, List<DictMemberInfo>> _dictMemberNames)
        {
            _baseActualName = _baseActualName ?? _baseJsonName;
            _baseJsonName = _baseJsonName ?? _baseJsonName;

            if (_dictMemberNames != null 
                && !string.IsNullOrWhiteSpace(_baseActualName) 
                && !string.IsNullOrWhiteSpace(_baseJsonName))
            {
                //filter for only the ones that have the base json name
                var _baseDictMemberNames = new Dictionary<string, List<DictMemberInfo>>();

                foreach (var item in _dictMemberNames)
                {
                    if (item.Key.StartsWith(_baseActualName))
                    {
                        _baseDictMemberNames[item.Key] = item.Value;

                        //if (item.Value?.Count > 0)
                        //{
                        //    foreach (var v in item.Value)
                        //    {
                        //        if (v.ComplexPath?.Count > 0)
                        //        {
                        //            //add each complex path to the main path for the sake of our resolution methods
                        //            var paths = new List<MemberInfo>(v.ComplexPath);
                        //            paths.Reverse();

                        //            var baseName = item.Key;
                        //            var baseJsonName = v.JsonName;

                        //            foreach (var c in v.ComplexPath)
                        //            {
                        //                baseName = $"{baseName}{Defaults.ComplexTypeNameSeparator}{c.Name}";
                        //                baseJsonName = $"{baseName}{Defaults.ComplexTypeSeparator}"
                        //                    _baseDictMemberNames[baseName] = new List<DictMemberInfo>() { };
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    else if (item.Value?.Where(v => v.JsonName.StartsWith(_baseJsonName)).ToList() is var filteredValues
                        && filteredValues?.Count > 0)
                    {
                        _baseDictMemberNames[item.Key] = filteredValues;
                    }
                }

                return _baseDictMemberNames.Count > 0 ? _baseDictMemberNames : null;
            }

            return null;
        }

        private static Dictionary<MemberName, MemberInfo> filterPropertyMap
            (string _baseActualName, string _baseJsonName,
            Dictionary<MemberName, MemberInfo> _jsonNamePropertyMap)
        {
            _baseActualName = _baseActualName ?? _baseJsonName;
            _baseJsonName = _baseJsonName ?? _baseJsonName;

            if (_jsonNamePropertyMap != null
                && !string.IsNullOrWhiteSpace(_baseActualName)
                && !string.IsNullOrWhiteSpace(_baseJsonName))
            {
                //filter for only the ones that have the base json name
                return _jsonNamePropertyMap
                    .Where(jm => jm.Key.Actual.StartsWith(_baseActualName)|| jm.Key.Json.StartsWith(_baseJsonName))
                    .ToDictionary(jm => jm.Key, jm => jm.Value);
            }

            return null;
        }

        private static List<JProperty> ResolveAssignments
            (IEntityService entityService,
            Dictionary<MemberName, MemberInfo> jsonNamePropertyMap,
            Dictionary<string, List<DictMemberInfo>> dictMemberNames,
            Dictionary<object, Expression> assignments, JObject jObject, string instanceTypeName,
            out List<ComplexAssignmentInfo> complexAssignments)
        {
            var filteredProps = new List<JProperty>();
            complexAssignments = null;

            foreach (var assignment in assignments)
            {
                var assignmentKey = assignment.Key;
                var assignmentKeyInfo = assignmentKey as MemberInfo;
                var assignmentKeyName = assignmentKey as string ?? assignmentKeyInfo?.Name;

                var assignmentValue = assignment.Value;
                var type = (assignmentKey as PropertyInfo)?.PropertyType
                    ?? (assignmentKey as FieldInfo)?.FieldType
                    ?? assignmentValue?.Type;

                if (type.IsComplex() && (assignmentValue == null || !Utils.Utilities.HasNfpEscape(assignmentValue)))
                {
                    var complexProps = new List<JProperty>();

                    //is complex type
                    //find the edges
                    ExpressionUtilities.ExplodeComplexTypeAndMemberAccess
                        (entityService, ref assignmentValue, type, out var inversePaths, shouldTryCast: true);

                    //get base json name
                    var baseJsonName = GetMemberJsonNameFromAssignment
                        (jsonNamePropertyMap, dictMemberNames,
                        assignmentKeyInfo, assignmentKeyName,
                        assignmentKeyInfo, assignmentKeyName, instanceTypeName) ?? assignmentKeyName;

                    var baseJsonPropMap = filterPropertyMap(assignmentKeyName, baseJsonName, jsonNamePropertyMap);
                    var baseDictMemberNames = filterDictionaryMap(assignmentKeyName, baseJsonName, dictMemberNames);
                    var baseAllDictMembers = baseDictMemberNames?.SelectMany(p => p.Value).ToArray();

                    foreach (var inversePath in inversePaths)
                    {
                        JProperty resolvedProp = null;

                        if (baseJsonPropMap != null)
                        {
                            inversePath.Reverse(); //don't make it inverse again

                            string lastActualName = assignmentKeyName;
                            string lastJsonName = baseJsonName;
                            var lastJsonPropMap = baseJsonPropMap;
                            //var lastDictMemberNames = baseDictMemberNames;

                            //start from the top
                            foreach (var actualMember in inversePath)
                            {
                                try
                                {
                                    resolvedProp = ResolveJPropertyFromAssignment
                                        (lastJsonPropMap, null, jObject, assignmentKeyInfo, assignmentKeyName,
                                        actualMember, actualMember.Name, instanceTypeName);
                                }
                                catch
                                {
                                }

                                lastActualName = $"{lastActualName}{Defaults.ComplexTypeNameSeparator}{actualMember.Name}";
                                lastJsonName = resolvedProp?.Name ?? $"{lastJsonName}{Defaults.ComplexTypeNameSeparator}{actualMember.Name}";
                                lastJsonPropMap = filterPropertyMap(lastActualName, lastJsonName, lastJsonPropMap);
                                //lastDictMemberNames = filterDictionaryMap(lastJsonName, lastDictMemberNames);
                            }

                            inversePath.Reverse(); //make it inverse again
                        }

                        if (resolvedProp == null && baseAllDictMembers != null)
                        {
                            //for dictionaries, find the one whose members matches this inversePath exactly
                            var inversePathCount = inversePath.Count;

                            var pathJsonName = baseAllDictMembers.FirstOrDefault(v =>
                            {
                                if (v.ComplexPath == null)
                                    return false;

                                if (v.ComplexPath.Count != inversePathCount)
                                    return false;

                                for (int i = 0; i < inversePathCount; i++)
                                {
                                    if (v.ComplexPath[i]?.IsEquivalentTo(inversePath[i]) != true)
                                        return false;
                                }

                                return true;
                            })?.JsonName;

                            if (pathJsonName != null)
                            {
                                resolvedProp = ResolveJPropertyFromJsonName(jObject, pathJsonName, instanceTypeName);
                            }
                        }

                        if (resolvedProp != null)
                            complexProps.Add(resolvedProp);
                    }

                    filteredProps.AddRange(complexProps);

                    if (complexAssignments == null)
                        complexAssignments = new List<ComplexAssignmentInfo>();

                    complexAssignments.Add(new ComplexAssignmentInfo
                        (assignmentKey, assignmentValue, assignmentValue?.Type ?? type, complexProps));
                }
                else
                {
                    filteredProps.Add(ResolveJPropertyFromAssignment
                        (jsonNamePropertyMap, dictMemberNames, jObject,
                        assignmentKeyInfo, assignmentKeyName,
                        assignmentKeyInfo, assignmentKeyName, instanceTypeName));
                }
            }

            return filteredProps;
        }

        public static string BuildPaths(ref ICypherFluentQuery query,
            IEnumerable<Expression<Func<IPathBuilder, IPathExtent>>> pathBuildExpressions,
            PropertiesBuildStrategy patternBuildStrategy)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var pathBuilds = new List<string>();

            foreach (var pathExpr in pathBuildExpressions)
            {
                pathBuilds.Add(new PathBuilder(query, pathExpr)
                {
                    PatternBuildStrategy = patternBuildStrategy
                }.Build(ref query));
            }

            var pathsText = pathBuilds.Aggregate((first, second) => $"{first}, {second}");

            stringBuilder.Append(pathsText);

            return stringBuilder.ToString();
        }

        internal static QueryContext GetQueryContext(ICypherFluentQuery query)
        {
            var queryContext = new QueryContext();

            var client = (query as IAttachedReference)?.Client;

            var _serializer = client?.Serializer ?? new CustomJsonSerializer()
            {
                JsonContractResolver = client?.JsonContractResolver ?? GraphClient.DefaultJsonContractResolver,
                JsonConverters = client?.JsonConverters ?? (IEnumerable<JsonConverter>)GraphClient.DefaultJsonConverters
            };

            var customJsonSerializer = _serializer as CustomJsonSerializer;

            var serializer = _serializer;

            //var resolver = client?.JsonContractResolver as EntityResolver ??
            //    (serializer as CustomJsonSerializer)?.JsonContractResolver as EntityResolver;

            //var converters = new List<JsonConverter>((IEnumerable<JsonConverter>)client?.JsonConverters ?? new JsonConverter[0]);
            //converters.AddRange((serializer as CustomJsonSerializer)?.JsonConverters ?? new JsonConverter[0]);

            //var converter = converters.FirstOrDefault(c => c is EntityConverter) as EntityConverter;

            Func<object, string> actualSerializer = null;

            if (_serializer != null)
            {
                actualSerializer = (obj) =>
                {
                    NullValueHandling? nullHandling = null;
                    if (customJsonSerializer != null)
                    {
                        nullHandling = customJsonSerializer.NullHandling; //save it.
                        customJsonSerializer.NullHandling = NullValueHandling.Include; //change it. we need all values
                    }

                    var serialized = _serializer.Serialize(obj);

                    if (customJsonSerializer != null)
                        customJsonSerializer.NullHandling = nullHandling.Value; //restore it.

                    return serialized;
                };
            }
            else
            {
                actualSerializer = null;
            }

            return new QueryContext()
            {
                Client = client,
                //Converter = converter,
                ISerializer = serializer,
                //Resolver = resolver,
                SerializeCallback = actualSerializer,
                CurrentQueryWriter = QueryContext.QueryWriterGetter(query),
                CurrentBuildStrategy = QueryContext.BuildStrategyGetter(query)
            };
        }

        internal static LambdaExpression GetConstraintsAsPropertiesLambda(LambdaExpression constraints, Type type)
        {
            //check if the constraint param type matches before proceeding
            if (constraints.Parameters.Single() is ParameterExpression p
                && p.Type != type)
            {
                var newParamExpr = Expression.Parameter(type, p.Name);
                var newParamCastExpr = Expression.Convert(newParamExpr, p.Type);

                //change the params to match
                var visitor = new ParameterReplacerVisitor(new Dictionary<string, Expression>()
                        {
                            { p.Name, newParamCastExpr }
                        });
                try
                {
                    var body = visitor.Visit(constraints.Body);
                    constraints = Expression.Lambda(body, newParamExpr);
                }
                catch
                {

                }
            }

            var setMethod = Utils.Utilities.GetGenericMethodInfo(Utils.Utilities.GetMethodInfo(() => ObjectExtensions._Set<object>(null, null, true)), type);

            return Expression.Lambda(Expression.Call(setMethod, Expression.Constant(type.GetDefaultValue(), type),
                constraints, Expression.Constant(true) //i.e, usePredicateOnly: true
                ));
        }

        internal static string BuildWithParamsForValues(JObject finalProperties, Func<object, string> serializer,
            Func<string, string> getKey, string separator, Func<string, string> getValue, out bool hasRaw, out JObject newFinalProperties)
        {
            bool _hasRaw = false;
            JObject _newFinalProperties = null;

            var value = finalProperties.Properties()
                .Select(jp =>
                {
                    if (jp.Value?.Type == JTokenType.Raw)
                    {
                        //most likely a query variable
                        //do not use a parameter in this case
                        //write directly instead
                        //and remove from properties
                        _hasRaw = true;
                        if (_newFinalProperties == null)
                            _newFinalProperties = finalProperties.DeepClone() as JObject;

                        _newFinalProperties.Remove(jp.Name);
                        return $"{getKey(jp.Name)}{separator}{serializer(jp.Value)}";
                    }

                    return $"{getKey(jp.Name)}{separator}{getValue(jp.Name)}";
                })
                .Aggregate((first, second) => $"{first}, {second}");

            hasRaw = _hasRaw;
            newFinalProperties = _newFinalProperties;

            return value;
        }

        internal static void ResolveFinalObjectProperties(
            Func<object> finalObjectGetter, Func<JObject> finalPropertiesGetter, Func<bool> finalPropsHasFuncsGetter,
            ref PropertiesBuildStrategy buildStrategy, out object finalObject, out JObject finalProperties)
        {
            finalObject = null;
            finalProperties = null;

            if (buildStrategy == PropertiesBuildStrategy.WithParams)
            {
                try
                {
                    //try to get the object first to see if we can defer its serialization
                    finalObject = finalObjectGetter();
                }
                catch
                {
                }
            }

            if (finalObject == null)
            {
                finalObject = finalProperties = finalPropertiesGetter();

                if (buildStrategy == PropertiesBuildStrategy.WithParams && finalPropsHasFuncsGetter())
                {
                    //finalObjectGetter should have returned an error so we check here
                    //if it has functions, the best way is the withParamsForValues
                    buildStrategy = PropertiesBuildStrategy.WithParamsForValues;
                }
            }
        }
    }
}
