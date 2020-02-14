using Neo4j.Driver.V1;
using Neo4jClient.DataAnnotations.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class RelationshipWrapper : BaseWrapper<IRelationship>, IRelationship
    {
        public RelationshipWrapper(IRelationship relationship) : base(relationship) { }

        public object this[string key] => Properties[key];

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                var props = WrappedItem.Properties ?? new Dictionary<string, object>();

                if (!props.ContainsKey(Defaults.BoltMetadataPropertyName))
                {
                    var propsDict = props as IDictionary<string, object> ?? props.ToDictionary(p => p.Key, p => p.Value);
                    //the following is required for proper deserialization
                    propsDict[Defaults.BoltMetadataPropertyName] = new
                    {
                        id = WrappedItem.Id,
                        type = WrappedItem.Type,
                        startNodeId = WrappedItem.StartNodeId,
                        endNodeId = WrappedItem.EndNodeId
                    };

                    props = propsDict as IReadOnlyDictionary<string, object>;
                }

                return props;
            }
        }

        public long Id => WrappedItem.Id;

        public string Type => WrappedItem.Type;

        public long StartNodeId => WrappedItem.StartNodeId;

        public long EndNodeId => WrappedItem.EndNodeId;
    }
}
