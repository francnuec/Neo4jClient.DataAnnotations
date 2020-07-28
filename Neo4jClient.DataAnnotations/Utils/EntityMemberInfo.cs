using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4jClient.DataAnnotations.Cypher;
using Neo4jClient.DataAnnotations.Serialization;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class EntityMemberInfo : IHaveEntityService
    {
        private string complexJsonName;

        private string complexName;

        private string jsonName;

        private Type reflectedType;

        public EntityMemberInfo(EntityService entityService,
            string memberName, Type memberType,
            EntityMemberInfo complexParent = null, Type reflectedType = null)
        {
            EntityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            Name = memberName ?? throw new ArgumentNullException(nameof(memberName));
            MemberFinalType = memberType ?? throw new ArgumentNullException(nameof(memberType));

            ComplexParent = complexParent;
            //ComplexName = ComplexParent == null ? Name : $"{ComplexParent.ComplexName}{Defaults.ComplexTypeNameSeparator}{Name}";

            IsDictionaryKey = true; //assume this first

            ReflectedType = reflectedType;

            complexParent?.MemberInfo?.GetMemberType();

            HasComplexParent = ComplexParent != null;

            var root = this;

            do
            {
                ComplexRoot = root;
                root = ComplexRoot.ComplexParent;
            } while (root != null);
        }

        public EntityMemberInfo(EntityService entityService, MemberInfo member,
            EntityMemberInfo complexParent = null, Type reflectedType = null)
            : this(entityService, member?.Name ?? throw new ArgumentNullException(nameof(member)),
                member.GetMemberType(), complexParent, reflectedType)
        {
            MemberInfo = member;

            DeclaringType = member.DeclaringType;

            IsPropertyInfo = member is PropertyInfo;
            IsFieldInfo = member is FieldInfo;
            IsDictionaryKey = false;

            HasComplexParent = ComplexParent != null;

            var root = this;

            do
            {
                ComplexRoot = root;
                root = ComplexRoot.ComplexParent;
            } while (root != null);
        }

        public string Name { get; }

        public string JsonName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(jsonName)) return Name;

                return jsonName;
            }
            internal set => jsonName = value;
        }

        public string ComplexName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(complexName))
                    return ComplexParent == null
                        ? Name
                        : $"{ComplexParent.ComplexName}{Defaults.ComplexTypeNameSeparator}{Name}";

                return complexName;
            }
            internal set => complexName = value;
        }

        public string ComplexJsonName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(complexJsonName))
                    return ComplexParent == null
                        ? JsonName
                        : $"{ComplexParent.ComplexJsonName}{Defaults.ComplexTypeNameSeparator}{JsonName}";

                return complexJsonName;
            }
            internal set => complexJsonName = value;
        }

        public MemberInfo MemberInfo { get; }

        public EntityMemberInfo ComplexParent { get; }

        public Type ReflectedType
        {
            get
            {
                if (reflectedType == null)
                {
                    reflectedType = MemberInfo?.ReflectedType;

                    if (ComplexParent?.MemberFinalType is Type memType
                        && reflectedType?.IsGenericAssignableFrom(memType) == true)
                        //choose the more specific type
                        reflectedType = memType;

                    if (ComplexParent?.MemberInfo?.GetMemberType() is Type memType2
                        && reflectedType?.IsGenericAssignableFrom(memType2) == true)
                        reflectedType = memType2;

                    if (DeclaringType != null
                        && reflectedType?.IsGenericAssignableFrom(DeclaringType) == true)
                        reflectedType = DeclaringType;
                }

                return reflectedType;
            }
            private set => reflectedType = value;
        }

        public Type DeclaringType { get; }

        public Type MemberFinalType { get; internal set; }

        public bool IsPropertyInfo { get; }

        public bool IsFieldInfo { get; }

        public bool HasComplexParent { get; }

        public bool IsComplex => MemberFinalType.IsComplex();

        public bool IsDictionaryKey { get; internal set; }

        public EntityMemberInfo ComplexRoot { get; }

        public EntityService EntityService { get; }

        public override bool Equals(object obj)
        {
            if (obj is EntityMemberInfo otherComplexMemberInfo)
            {
                var equals = MemberInfo?.Equals(otherComplexMemberInfo.MemberInfo) ??
                             ComplexName.Equals(otherComplexMemberInfo.ComplexName)
                             && (ComplexName == Name
                                 ? MemberFinalType?.Equals(otherComplexMemberInfo.MemberFinalType) ?? false
                                 : true);

                return equals && (ComplexParent?.Equals(otherComplexMemberInfo.ComplexParent) ?? true);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (MemberInfo?.GetHashCode()
                    ?? ComplexName?.GetHashCode()
                    ?? ComplexJsonName?.GetHashCode()
                    ?? base.GetHashCode()) | (ComplexParent?.GetHashCode() ?? 0);
        }

        public void ResolveNames(EntityResolver resolver, Func<object, string> serializer)
        {
            if (!string.IsNullOrWhiteSpace(jsonName))
                return;

            //first resolve all parents
            ComplexParent?.ResolveNames(resolver, serializer);

            var parentType = ReflectedType ?? ComplexParent?.MemberFinalType ?? DeclaringType;
            if (parentType != null && !parentType.IsAnonymousType())
            {
                ReflectedType = parentType;

                //add it to entity types
                EntityService.AddEntityType(parentType);

                //we do this because MemberInfo.ReflectedType is not public yet in the .NET Core API.
                var typeInfo = EntityService.GetEntityTypeInfo(parentType);

                if (typeInfo.JsonNamePropertyMap.Count == 0)
                {
                    if (resolver != null)
                    {
                        typeInfo.ResolveJsonPropertiesUsing(resolver);
                    }
                    else
                    {
                        //force a serialization to fill the json name map
                        //create new instance
                        var entityType = typeInfo.Type;
                        var entity = Utils.Utilities.CreateInstance(entityType);

                        //take care of the entity's complex properties and those of its children
                        Utils.Utilities.InitializeComplexTypedProperties(entity, EntityService);

                        if (!EntityService.EntityTypes.Contains(entityType))
                            EntityService.AddEntityType(entityType); //just in case it wasn't already added.

                        //serialize the entity so the jsonnames would be set
                        var serializedEntity = serializer(entity);
                    }
                }

                var nonJsonName = Name;

                var allJsonPropMap = typeInfo.JsonNamePropertyMap; //infos.SelectMany(info => info.JsonNamePropertyMap);

                var jsonPropMap = CypherUtilities.FilterPropertyMap(nonJsonName, null, allJsonPropMap);

                var memberInfo = MemberInfo;
                //var complexName = ComplexName;

                var memberJsonNameMapItem = jsonPropMap
                    .Where(item =>
                        memberInfo != null
                            ? item.Value.IsEquivalentTo(memberInfo)
                            : item.Key.Actual == nonJsonName &&
                              item.Value.GetMemberType().IsGenericAssignableFrom(MemberFinalType))
                    .FirstOrDefault();

                if (memberJsonNameMapItem.Value != null)
                {
                    JsonName = memberJsonNameMapItem.Key.Json;
                    ComplexJsonName = null; //clear it so it is auto-generated. //memberJsonNameMapItem.Key.ComplexJson;
                    ComplexName = null; //memberJsonNameMapItem.Key.ComplexActual;
                }
            }
        }

        public override string ToString()
        {
            try
            {
                return $"({ComplexName}, {ComplexJsonName})";
            }
            catch
            {
            }

            return base.ToString();
        }

        public List<string> GetAllPossibleComplexNames()
        {
            var result = new List<string> { ComplexName, Name };

            var parentNames = ComplexParent?.GetAllPossibleComplexNames();

            if (parentNames?.Count > 0)
                foreach (var parentName in parentNames)
                    result.Add($"{parentName}{Defaults.ComplexTypeNameSeparator}{Name}");

            result = result.Distinct().ToList();
            return result;
        }

        public List<string> GetAllPossibleComplexJsonNames()
        {
            var result = new List<string> { ComplexJsonName, JsonName };

            var parentNames = ComplexParent?.GetAllPossibleComplexJsonNames();

            if (parentNames?.Count > 0)
                foreach (var parentName in parentNames)
                    result.Add($"{parentName}{Defaults.ComplexTypeNameSeparator}{Name}");

            //make distinct and arrange from most complex to least complex
            result = result.Distinct().OrderByDescending(n => n.Length).ToList();
            return result;
        }
    }
}