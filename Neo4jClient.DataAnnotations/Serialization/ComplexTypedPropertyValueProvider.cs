using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Neo4jClient.DataAnnotations.Serialization
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
            var type = Type;

            var instance = ValueProvider.GetValue(target);

            instance = Utilities.GetComplexTypeInstance(Name, ref type, DeclaringType, instance, out var isNew,
                hasComplexChild: childComplexProvider != null, 
                childName: childComplexProvider?.Name, childType: childComplexProvider?.Type,
                childDeclaringType: childComplexProvider?.DeclaringType);

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
