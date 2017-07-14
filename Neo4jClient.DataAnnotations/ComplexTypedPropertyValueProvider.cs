using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

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
            var instance = ValueProvider.GetValue(target);
            Utilities.CheckIfComplexTypeInstanceIsNull(instance, Name, DeclaringType);

            ChildValueProvider.SetValue(instance, value);
        }
    }
}
