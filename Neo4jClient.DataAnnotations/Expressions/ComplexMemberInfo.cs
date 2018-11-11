using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public interface IComplexMemberInfo
    {
        string Name { get; }

        MemberInfo Member { get; }

        MemberInfo Parent { get; }

        Type ComplexMemberType { get; }

        bool IsPropertyInfo { get; }

        bool IsFieldInfo { get; }
    }

    //public class ComplexPropertyInfo : PropertyInfo, IComplexMemberInfo
    //{
    //    public MemberInfo Member { get; }

    //    public PropertyInfo Property { get; }

    //    public MemberInfo Parent { get; }

    //    public Type ComplexMemberType { get; }

    //    public bool IsPropertyInfo { get; }

    //    public bool IsFieldInfo { get; }

    //    public ComplexPropertyInfo(PropertyInfo member, MemberInfo parent, Type reflectedType = null)
    //    {
    //        Member = member ?? throw new ArgumentNullException(nameof(member));
    //        Parent = parent ?? throw new ArgumentNullException(nameof(parent));
    //        Property = member as PropertyInfo;

    //        ComplexMemberType = member.PropertyType;
    //        IsPropertyInfo = true;
    //        IsFieldInfo = false;

    //        ReflectedType = reflectedType ?? Parent.GetMemberType() ?? Member.ReflectedType;
    //        Name = $"{Parent.Name}{Defaults.ComplexTypeNameSeparator}{Member.Name}";
    //        CanRead = member.CanRead;
    //        CanWrite = member.CanWrite;
    //        PropertyType = Property.PropertyType;
    //    }

    //    public override Type DeclaringType => Member.DeclaringType;

    //    public override MemberTypes MemberType => Member.MemberType;

    //    public override string Name { get; }

    //    public override Type ReflectedType { get; }

    //    public override object[] GetCustomAttributes(bool inherit)
    //    {
    //        return Member.GetCustomAttributes(inherit);
    //    }

    //    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    //    {
    //        return Member.GetCustomAttributes(attributeType, inherit);
    //    }

    //    public override bool IsDefined(Type attributeType, bool inherit)
    //    {
    //        return Member.IsDefined(attributeType, inherit);
    //    }

    //    public override IEnumerable<CustomAttributeData> CustomAttributes => Member.CustomAttributes;

    //    public override IList<CustomAttributeData> GetCustomAttributesData()
    //    {
    //        return Member.GetCustomAttributesData();
    //    }

    //    public override int MetadataToken => Member.MetadataToken;

    //    public override Module Module => Member.Module;

    //    public override PropertyAttributes Attributes { get; }

    //    public override bool CanRead { get; }

    //    public override bool CanWrite { get; }

    //    public override Type PropertyType { get; }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj is ComplexPropertyInfo otherComplexMemberInfo)
    //        {
    //            return Member.Equals(otherComplexMemberInfo.Member) && Parent.Equals(otherComplexMemberInfo.Parent);
    //        }

    //        return base.Equals(obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return Member.GetHashCode() | Parent.GetHashCode();
    //    }

    //    public override MethodInfo[] GetAccessors(bool nonPublic)
    //    {
    //        return Property.GetAccessors(nonPublic);
    //    }

    //    public override MethodInfo GetGetMethod(bool nonPublic)
    //    {
    //        return Property.GetGetMethod(nonPublic);
    //    }

    //    public override ParameterInfo[] GetIndexParameters()
    //    {
    //        return Property.GetIndexParameters();
    //    }

    //    public override MethodInfo GetSetMethod(bool nonPublic)
    //    {
    //        return Property.GetSetMethod(nonPublic);
    //    }

    //    public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
    //    {
    //        return Property.GetValue(obj, invokeAttr, binder, index, culture);
    //    }

    //    public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
    //    {
    //        Property.SetValue(obj, value, invokeAttr, binder, index, culture);
    //    }
    //}
}
