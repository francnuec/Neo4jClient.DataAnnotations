using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neo4jClient.DataAnnotations
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
