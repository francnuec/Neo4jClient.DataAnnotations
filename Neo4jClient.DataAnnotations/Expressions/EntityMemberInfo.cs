using System;
using System.Reflection;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class EntityMemberInfo
    {
        public EntityMemberInfo(string memberName, Type memberType, 
            EntityMemberInfo complexParent = null, Type reflectedType = null)
        {
            Name = memberName ?? throw new ArgumentNullException(nameof(memberName));
            MemberFinalType = memberType ?? throw new ArgumentNullException(nameof(memberType));

            ComplexParent = complexParent;
            ComplexName = ComplexParent == null ? Name : $"{ComplexParent.ComplexName}{Defaults.ComplexTypeNameSeparator}{Name}";

            ReflectedType = reflectedType ?? complexParent?.MemberFinalType ?? complexParent?.MemberInfo?.GetMemberType();

            HasComplexParent = ComplexParent != null;

            var root = this;

            do
            {
                ComplexRoot = root;
                root = ComplexRoot.ComplexParent;
            } while (root != null);
        }

        public EntityMemberInfo(MemberInfo member, EntityMemberInfo complexParent = null, Type reflectedType = null)
            : this(member?.Name ?? throw new ArgumentNullException(nameof(member)),
                  member.GetMemberType(), complexParent, reflectedType)
        {
            MemberInfo = member;

            DeclaringType = member.DeclaringType;
            ReflectedType = ReflectedType ?? MemberInfo.ReflectedType;

            IsPropertyInfo = member is PropertyInfo;
            IsFieldInfo = member is FieldInfo;
            HasComplexParent = ComplexParent != null;

            var root = this;

            do
            {
                ComplexRoot = root;
                root = ComplexRoot.ComplexParent;
            } while (root != null);
        }

        public string Name { get; }

        public string ComplexName { get; }

        public MemberInfo MemberInfo { get; }

        public EntityMemberInfo ComplexParent { get; }

        public Type ReflectedType { get; }

        public Type DeclaringType { get; }

        public Type MemberFinalType { get; internal set; }

        public bool IsPropertyInfo { get; }

        public bool IsFieldInfo { get; }

        public bool HasComplexParent { get; }

        public bool IsComplex => MemberFinalType.IsComplex();

        public bool IsDictionaryKey { get; internal set; }

        public EntityMemberInfo ComplexRoot { get; }

        public override bool Equals(object obj)
        {
            if (obj is EntityMemberInfo otherComplexMemberInfo)
            {
                var equals = MemberInfo?.Equals(otherComplexMemberInfo.MemberInfo) ??
                    (ComplexName.Equals(otherComplexMemberInfo.ComplexName) 
                    && (ComplexName == Name ? MemberFinalType?.Equals(otherComplexMemberInfo.MemberFinalType) ?? false : true));

                return equals && (ComplexParent?.Equals(otherComplexMemberInfo.ComplexParent) ?? true);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return MemberInfo.GetHashCode() | (ComplexParent?.GetHashCode() ?? 0);
        }
    }
}
