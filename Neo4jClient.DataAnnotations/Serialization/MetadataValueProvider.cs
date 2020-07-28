﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.DataAnnotations.Serialization
{
    public class MetadataValueProvider : IValueProvider
    {
        public MetadataValueProvider
            (Type entityType, List<JsonProperty> properties)
        {
            EntityType = entityType;
            Properties = properties;
        }

        public Type EntityType { get; protected set; }
        public List<JsonProperty> Properties { get; }

        public object GetValue(object target)
        {
            //serialize metadata
            var ret = SerializationUtilities.SerializeMetadata(BuildMetadata(target));
            return ret;
        }

        public void SetValue(object target, object value)
        {
            string metadataJson = null;

            if (value != null && (metadataJson = value as string) != null)
            {
                //deserialize
                var metadata = SerializationUtilities.DeserializeMetadata(metadataJson);

                if (metadata?.NullProperties?.Count > 0 && Properties?.Count > 0)
                    //call each property and set a null value
                    foreach (var nullProp in metadata.NullProperties)
                    {
                        var jsonProperty = Properties.FirstOrDefault(p => p.PropertyName == nullProp);

                        if (jsonProperty != null)
                        {
                            //first try to get its value
                            //this makes sure that we don't overwrite a valid value
                            object propValue = null;
                            var getSuccess = false;
                            try
                            {
                                propValue = jsonProperty.ValueProvider?.GetValue(target);
                                getSuccess = true;
                            }
                            catch
                            {
                            }

                            if (!getSuccess && propValue == null)
                                jsonProperty.ValueProvider?.SetValue(target, null);
                        }
                    }
            }
        }

        public Metadata BuildMetadata(object target)
        {
            //create new metadata and go through props (which should be complex props) to find null values
            var metadata = new Metadata();

            //we really need to stop the null properties only when the value returned is a derived type.

            if (target != null && Properties?.Count > 0)
                foreach (var prop in Properties)
                    try
                    {
                        var value = prop.ValueProvider.GetValue(target);
                        if (value == null)
                        {
                            var complexValueProvider = prop.ValueProvider as ComplexTypedPropertyValueProvider;

                            if (complexValueProvider == null ||
                                complexValueProvider.Type !=
                                complexValueProvider.ValueProvider.GetValue(target)?.GetType())
                                metadata.NullProperties.Add(prop.PropertyName);
                        }
                    }
                    catch (JsonSerializationException e) when (e.InnerException is InvalidCastException)
                    {
                    }

            return metadata;
        }
    }
}