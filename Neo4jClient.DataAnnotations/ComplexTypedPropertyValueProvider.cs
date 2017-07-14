using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Neo4jClient.DataAnnotations
{
    public class ComplexTypedPropertyValueProvider : IValueProvider
    {
        public string Name { get; protected set; }
        public Type Type { get; protected set; }
        public IValueProvider ValueProvider { get; protected set; }
        public Type DeclaringType { get; protected set; }
        public Type ChildType { get; protected set; }
        public IValueProvider ChildValueProvider { get; protected set; }

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

        public object GetValue(object target)
        {
            var instance = ValueProvider.GetValue(target);
            Utilities.CheckIfComplexTypeInstanceIsNull(instance, Name, DeclaringType);

            return ChildValueProvider.GetValue(instance);
        }

        public void SetValue(object target, object value)
        {
            var childComplexProvider = ChildValueProvider as ComplexTypedPropertyValueProvider;
            var type = childComplexProvider?.DeclaringType ?? Type;

            var instance = ValueProvider.GetValue(target);
            bool isNew = false;

            if (instance == null)
            {
                instance = Activator.CreateInstance(type);
                isNew = true;
            }

            if (!isNew && childComplexProvider != null)
            {
                var members = instance.GetType().GetMembers(Defaults.MemberSearchBindingFlags).Where(m => m is FieldInfo || m is PropertyInfo);

                //check if the instance has the child as a member
                if (members?.Where(m => m.IsEquivalentTo(childComplexProvider.Name, childComplexProvider.DeclaringType,
                    childComplexProvider.Type)).FirstOrDefault() == null)
                {
                    //it doesn't, so create new instance from child declaring type
                    //first save old instance
                    var oldInstance = instance;
                    instance = Activator.CreateInstance(type);
                    isNew = true;

                    //now copy the values from old instance unto this new one
                    foreach (var member in members)
                    {
                        var field = member as FieldInfo;
                        var property = member as PropertyInfo;

                        try
                        {
                            if (property?.CanWrite == true)
                                property.SetValue(instance, property.GetValue(oldInstance));
                            else
                                field?.SetValue(instance, field.GetValue(oldInstance));
                        }
                        catch
                        {

                        }
                    }
                }
            }

            Utilities.CheckIfComplexTypeInstanceIsNull(instance, Name, DeclaringType);

            if (isNew)
            {
                //set the value
                ValueProvider.SetValue(target, instance);
            }

            ChildValueProvider.SetValue(instance, value);
        }
    }
}
