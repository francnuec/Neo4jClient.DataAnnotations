using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class ComplexAssignmentInfo
    {
        public object Key { get; set; }
        public Expression Value { get; set; }
        public Type Type { get; set; }
        public List<JProperty> Properties { get; set; }

        public ComplexAssignmentInfo(object key, Expression value, Type type, List<JProperty> props)
        {
            Key = key;
            Value = value;
            Type = type;
            Properties = props;
        }
    }
}
