using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class NodeWrapper : BaseWrapper<INode>, INode
    {
        public NodeWrapper(INode node) : base(node)
        {
        }

        public object this[string key] => Properties[key];

        public IReadOnlyList<string> Labels => WrappedItem.Labels;

        public IReadOnlyDictionary<string, object> Properties
        {
            get
            {
                var props = WrappedItem.Properties ?? new Dictionary<string, object>();

                if (!props.ContainsKey(Defaults.BoltMetadataPropertyName))
                {
                    var propsDict = props as IDictionary<string, object> ??
                                    props.ToDictionary(p => p.Key, p => p.Value);
                    //the following is required for proper deserialization
                    propsDict[Defaults.BoltMetadataPropertyName] = new
                    {
                        id = WrappedItem.Id,
                        labels = WrappedItem.Labels?.ToArray() ?? Array.Empty<string>()
                    };

                    props = propsDict as IReadOnlyDictionary<string, object>;
                }

                return props;
            }
        }

        public long Id => WrappedItem.Id;
    }
}