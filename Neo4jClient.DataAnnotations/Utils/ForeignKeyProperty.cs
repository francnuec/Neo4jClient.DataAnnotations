using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Neo4jClient.DataAnnotations.Utils
{
    public class ForeignKeyProperty
    {
        public ForeignKeyAttribute Attribute { get; protected internal set; }

        public PropertyInfo ScalarProperty { get; protected internal set; }

        public PropertyInfo NavigationProperty { get; protected internal set; }

        public bool IsAttributePointingToProperty { get; protected internal set; }

        public bool IsAttributeAutoCreated { get; protected internal set; }
    }
}