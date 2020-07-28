﻿using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;

namespace Neo4jClient.DataAnnotations.Extensions.Driver
{
    public class RecordWrapper : BaseWrapper<IRecord>, IRecord
    {
        private IReadOnlyDictionary<string, object> values;

        public RecordWrapper(IRecord record) : base(record)
        {
        }

        public object this[int index] => GetValue(WrappedItem[index]);

        public object this[string key] => GetValue(WrappedItem[key]);

        public IReadOnlyDictionary<string, object> Values
        {
            get
            {
                if (values == null && WrappedItem.Values is IReadOnlyDictionary<string, object> recordValues)
                {
                    if (recordValues is IDictionary<string, object> _values)
                        foreach (var v in _values)
                            _values[v.Key] = GetValue(v.Value);
                    else
                        recordValues = recordValues.ToDictionary(v => v.Key, v => GetValue(v.Value));

                    values = recordValues;
                }

                return values;
            }
        }

        public IReadOnlyList<string> Keys => WrappedItem.Keys;

        protected static object GetValue(object value)
        {
            if (value is INode node && !(node is NodeWrapper))
                return new NodeWrapper(node);
            if (value is IRelationship relationship && !(relationship is RelationshipWrapper))
                return new RelationshipWrapper(relationship);

            return value;
        }
    }
}