using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations.Cypher.Functions;
using Neo4jClient.DataAnnotations.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.DataAnnotations.Utils
{
    public class Defaults
    {
        public const BindingFlags MemberSearchBindingFlags =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        public const string MetadataPropertyName = "__ncdannotationsmeta__"; //this value should never change

        public const string MetadataNullPropertiesPropertyName = "null_props"; //this value should never change

        public const string QueryBuildStrategyKey = "querypropbuildstrategy";

        public const string BoltMetadataPropertyName = "__bolt_metadata__";

        public static readonly PropertyInfo DummyMetadataPropertyInfo = typeof(Defaults).GetProperty(
            "DummyMetadataProperty",
            BindingFlags.Static | BindingFlags.Public);

        internal static readonly Type PropertyDummyType = typeof(PropertyDummy);
        internal static readonly PropertyInfo PropertyDummyPropertyInfo = PropertyDummyType.GetProperty("Property");

        //public static readonly MethodInfo AsMethodInfo = Utils.Utilities.GetMethodInfo(() => HelperExtensions._As<object>(null));

        //public static readonly MethodInfo AsListMethodInfo = Utils.Utilities.GetMethodInfo(() => HelperExtensions._AsList<object>(null));

        public static readonly MethodInfo IsNullExtMethodInfo =
            Utilities.GetMethodInfo(() => CypherExtensionFunctions.IsNull<object>(null));

        public static readonly MethodInfo IsNotNullExtMethodInfo =
            Utilities.GetMethodInfo(() => CypherExtensionFunctions.IsNotNull<object>(null));

        public static readonly Type CypherObjectType = typeof(CypherObject);

        public static readonly MethodInfo CypherObjectIndexerInfo =
            CypherObjectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetIndexParameters().Any())
                .Select(p => p.GetGetMethod()).First();

        public static readonly Type DictionaryType = typeof(IDictionary<,>);

        public static readonly MethodInfo DictionaryIndexerInfo =
            DictionaryType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetIndexParameters().Any())
                .Select(p => p.GetGetMethod()).First();

        public static readonly MethodInfo DictStringObjectAddMethod = Utilities.GetMethodInfo
            (() => new Dictionary<string, object>().Add(null, null));

        public static readonly MethodInfo NfpExtMethodInfo =
            Utilities.GetMethodInfo(() => CypherExtensionFunctions._<object>(null));

        public static readonly FieldInfo QueryWriterInfo = typeof(CypherFluentQuery).GetField("QueryWriter",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly FieldInfo QueryWriterParamsInfo = typeof(QueryWriter).GetField("queryParameters",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


        public static string ComplexTypeNameSeparator = "_";

        [JsonProperty(PropertyName = MetadataPropertyName)]
        public static string DummyMetadataProperty { get; set; }

        #region Types

        public static readonly Type ForeignKeyType = typeof(ForeignKeyAttribute);
        public static readonly Type InversePropertyType = typeof(InversePropertyAttribute);
        public static readonly Type ColumnType = typeof(ColumnAttribute);
        public static readonly Type KeyType = typeof(KeyAttribute);
        public static readonly Type ComplexType = typeof(ComplexTypeAttribute);
        public static readonly Type VarsType = typeof(CypherVariables);

        public static readonly Type ExtensionsType = typeof(Extensions);

        //public static readonly Type HelperExtensionsType = typeof(HelperExtensions);
        public static readonly Type NeoScalarType = typeof(NeoScalarAttribute);
        public static readonly Type NeoNonScalarType = typeof(NeoNonScalarAttribute);
        public static readonly Type ObjectType = typeof(object);

        public static readonly Type UtilitiesType = typeof(Utilities);
        public static readonly Type StringType = typeof(string);
        public static readonly Type EnumerableType = typeof(Enumerable);
        public static readonly Type MathType = typeof(Math);

        public static readonly Type CypherFuncsType = typeof(CypherFunctions);
        public static readonly Type CypherExtensionFuncsType = typeof(CypherExtensionFunctions);
        public static readonly Type JRawType = typeof(JRaw);
        public static readonly Type ICypherResultItemType = typeof(ICypherResultItem);
        public static readonly Type EntitySetType = typeof(EntitySet<>);
        public static readonly Type ConcreteEntitySetType = typeof(ConcreteEntitySet<>);
        public static readonly Type DictStringObjectType = typeof(Dictionary<string, object>);
        public static readonly Type NotMappedType = typeof(NotMappedAttribute);
        public static readonly Type JsonIgnoreType = typeof(JsonIgnoreAttribute);

        #endregion
    }
}