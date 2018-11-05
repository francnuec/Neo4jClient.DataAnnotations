using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class SerializationUtilities
    {
        public static string SerializeMetadata(Metadata metadata)
        {
            //only distinct values
            metadata.NullProperties = metadata.NullProperties.Distinct().ToList();
            return JsonConvert.SerializeObject(metadata);
        }

        public static Metadata DeserializeMetadata(string metadataJson)
        {
            return JsonConvert.DeserializeObject<Metadata>(metadataJson);
        }

        internal static void EnsureRightJObject(ref JObject valueJObject)
        {
            //the neo4jclient guys really messed things up here
            //so use heuristics to determine if we are passing the right data or not, and then get the right data
            //this is for deserialization only

            //example json received
            /*
             {
              "extensions": {},
              "metadata": {
                "id": 176,
                "labels": [
                  "IdentityUser"
                ]
              },
              "paged_traverse": "http://localhost:7474/db/data/node/176/paged/traverse/{returnType}{?pageSize,leaseTime}",
              "outgoing_relationships": "http://localhost:7474/db/data/node/176/relationships/out",
              "outgoing_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/out/{-list|&|types}",
              "labels": "http://localhost:7474/db/data/node/176/labels",
              "create_relationship": "http://localhost:7474/db/data/node/176/relationships",
              "traverse": "http://localhost:7474/db/data/node/176/traverse/{returnType}",
              "all_relationships": "http://localhost:7474/db/data/node/176/relationships/all",
              "all_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/all/{-list|&|types}",
              "property": "http://localhost:7474/db/data/node/176/properties/{key}",
              "self": "http://localhost:7474/db/data/node/176",
              "incoming_relationships": "http://localhost:7474/db/data/node/176/relationships/in",
              "properties": "http://localhost:7474/db/data/node/176/properties",
              "incoming_typed_relationships": "http://localhost:7474/db/data/node/176/relationships/in/{-list|&|types}",
              "data": {
                actual data ...
              }
            } 
             */

            var expectedProps = new Dictionary<string, JTokenType>()
            {
                //{ "data", JTokenType.Object },
                { "metadata", JTokenType.Object },
                { "self", JTokenType.String },
            };

            var jObject = valueJObject;

            if (expectedProps.All(prop => jObject[prop.Key]?.Type == prop.Value))
            {
                //hopefully we are right
                //replace the jObject with "data"
                valueJObject = jObject["data"] as JObject;
            }
        }

        internal static void EnsureSerializerInstance(ref JsonSerializer serializer)
        {
            if (serializer == null)
            {
                //this is strange, but the Neo4jClient folks forgot to pass a serializer to this method
                serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings()
                {
                    Converters = GraphClient.DefaultJsonConverters?.Reverse().ToList(),
                    ContractResolver = GraphClient.DefaultJsonContractResolver,
                    ObjectCreationHandling = ObjectCreationHandling.Auto,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                });
            }
        }

        internal static void RemoveThisConverter(Type converterType, JsonSerializer serializer, out List<Tuple<JsonConverter, int>> convertersRemoved)
        {
            convertersRemoved = new List<Tuple<JsonConverter, int>>();

            for (int i = 0, l = serializer.Converters.Count; i < l; i++)
            {
                var converter = serializer.Converters[i];
                if (converterType.IsAssignableFrom(converter.GetType()))
                {
                    convertersRemoved.Add(new Tuple<JsonConverter, int>(converter, i));
                }
            }

            foreach (var converter in convertersRemoved)
            {
                serializer.Converters.Remove(converter.Item1);
            }
        }

        internal static void RestoreThisConverter(JsonSerializer serializer, List<Tuple<JsonConverter, int>> convertersRemoved,
            bool clearConvertersRemovedList = true)
        {
            foreach (var converter in convertersRemoved)
            {
                try
                {
                    serializer.Converters.Insert(converter.Item2, converter.Item1);
                }
                catch
                {
                    serializer.Converters.Add(converter.Item1);
                }
            }

            if (clearConvertersRemovedList)
                convertersRemoved.Clear();
        }
    }
}
