using Neo4jClient.DataAnnotations.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    public class Defaults
    {
        #region Types
        public static readonly Type CypherObjectType = typeof(CypherObject);
        public static readonly Type ForeignKeyType = typeof(ForeignKeyAttribute);
        public static readonly Type InversePropertyType = typeof(InversePropertyAttribute);
        public static readonly Type ColumnType = typeof(ColumnAttribute);
        public static readonly Type KeyType = typeof(KeyAttribute);
        public static readonly Type ComplexType = typeof(ComplexTypeAttribute);
        public static readonly Type VarsType = typeof(Vars);
        public static readonly Type ExtensionsType = typeof(Extensions);
        public static readonly Type ObjectExtensionsType = typeof(ObjectExtensions);
        public static readonly Type NeoScalarType = typeof(NeoScalarAttribute);
        public static readonly Type NeoNonScalarType = typeof(NeoNonScalarAttribute);
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type DictionaryType = typeof(IDictionary<,>);
        public static readonly Type UtilitiesType = typeof(Utilities);
        #endregion

        public static string ComplexTypeNameSeparator = "_";

        public static Serialization.EntityResolver EntityResolver { get; internal set; }
        public static Serialization.EntityConverter EntityConverter { get; internal set; }
        public static Serialization.ResolverDummyConverter ResolverDummyConverter { get; internal set; }

        public const BindingFlags MemberSearchBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        public const string MetadataPropertyName = "__ncdannotationsmeta__"; //this value should never change

        public const string MetadataNullPropertiesPropertyName = "null_props"; //this value should never change

        [JsonProperty(PropertyName = MetadataPropertyName)]
        public static string DummyMetadataProperty { get; set; }
    }
}
