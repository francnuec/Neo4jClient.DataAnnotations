using System;
using System.Linq;
using System.Reflection;
using Neo4jClient.DataAnnotations.Utils;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class ComplexTypedPropertyValueProvider : IValueProvider
    {
        public ComplexTypedPropertyValueProvider
        (string name, Type type, Type declaringType, IValueProvider valueProvider,
            Type childType, IValueProvider childValueProvider)
        {
            Name = name;
            Type = type;
            DeclaringType = declaringType;

            ValueProvider = valueProvider;

            ChildType = childType;
            ChildValueProvider = childValueProvider;
        }

        public string Name { get; protected set; }
        public Type Type { get; protected set; }
        public IValueProvider ValueProvider { get; protected set; }
        public Type DeclaringType { get; protected set; }
        public Type ChildType { get; protected set; }
        public IValueProvider ChildValueProvider { get; protected set; }

        public object GetValue(object target)
        {
            var instance = ValueProvider.GetValue(target);
            Utils.Utilities.CheckIfComplexTypeInstanceIsNull(instance, Name, DeclaringType);

            return ChildValueProvider.GetValue(instance);
        }

        public void SetValue(object target, object value)
        {
            var childComplexProvider = ChildValueProvider as ComplexTypedPropertyValueProvider;
            var type = Type;

            var instance = ValueProvider.GetValue(target);

            instance = GetComplexTypeInstance(Name, ref type, DeclaringType, instance, out var isNew,
                childComplexProvider != null,
                childComplexProvider?.Name, childComplexProvider?.Type,
                childComplexProvider?.DeclaringType);

            Utils.Utilities.CheckIfComplexTypeInstanceIsNull(instance, Name, DeclaringType);

            if (isNew)
                //set the value
                ValueProvider.SetValue(target, instance);

            ChildValueProvider.SetValue(instance, value);
        }

        public static object GetComplexTypeInstance(
            string name, ref Type type, Type declaringType,
            object existingInstance, out bool isNew,
            bool hasComplexChild = false, string childName = null,
            Type childType = null, Type childDeclaringType = null)
        {
            type = childDeclaringType ?? type;

            isNew = false;

            var instance = existingInstance;

            if (instance == null)
            {
                instance = Utils.Utilities.CreateInstance(type);
                isNew = true;
            }

            if (!isNew && hasComplexChild)
            {
                var members = instance.GetType().GetMembers(Defaults.MemberSearchBindingFlags)
                    .Where(m => m is FieldInfo || m is PropertyInfo);

                //check if the instance has the child as a member
                if (members?.Where(m => m.IsEquivalentTo(childName, childDeclaringType,
                    childType)).FirstOrDefault() == null)
                {
                    //it doesn't, so create new instance from child declaring type
                    existingInstance = instance;
                    instance = Utils.Utilities.CreateInstance(type);
                    isNew = true;

                    //now copy the values from old instance unto this new one
                    foreach (var member in members)
                    {
                        var field = member as FieldInfo;
                        var property = member as PropertyInfo;

                        try
                        {
                            if (property?.CanWrite == true)
                                property.SetValue(instance, property.GetValue(existingInstance));
                            else
                                field?.SetValue(instance, field.GetValue(existingInstance));
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return instance;
        }
    }
}