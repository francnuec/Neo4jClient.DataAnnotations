using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    public class Messages
    {
        public const string NullARBParametersError = "A, R, and B are null. You should provide at least one parameter name.";

        public const string PathableNotRecognizedError = "Cannot process this IPathable type as it is not recognized. Expected IPathBuilder or Path types.";

        public const string PropsAndConstraintsClashError = "You can only provide either constraints or properties for entity {0}, but not both as we have in this case.";

        public const string NullComplexTypePropertyError = "ComplexType properties cannot be null. An instance must be provided always. Property: {0}, Class: {1}.";

        public const string ComplexTypeSerializationError = "Error serializing ComplexType Property: (0), from Class: {1}.";

        public const string NestedComplexTypesError = "Complex types cannot be nested. Class: {0}";

        public const string NoEntityParamaterlessConstructorError = "Entity '{0}' does not have a parameterless constrcutor. All entities are expected to have an accessible parameterless constrcutor.";

        public const string NoContractResolvedError = "Could not resolve contract, or received bad contract for '{0}'. Please ensure you have a DefaultContractResolver object set on your JsonSerializers.";

        public const string ExtendLastBNewAMismatchError = "The last B node of Type: {0} does not match this new A of Type: {1}. Since you are extending the last pattern, this two should match.";

        public const string ParamsGetError = "Please do not call this method directly. Use only in expressions.";

        public const string ComplexTypedPropertyMatchingTypeNotFoundError = "No matching type found for Complex Typed Property: {0}, in Class: {1}.";

        public const string InvalidMemberAssignmentError = "Invalid member assignment. Member: {0}";

        public const string JsonPropertyNotFoundError = "Json Property '{0}' not found for Type '{1}'";

        public const string AmbiguousParamsPathError = "Expression path to Neo4j parameter is ambiguous, and cannot be translated. Params: {0}.";

        public const string NoResolverOrConverterError = "You must enable either the EntityResolver or EntityConverter for Neo4jClient.DataAnnotations to work.";

        public const string BothResolverAndConverterError = "You cannot enable both EntityResolver and EntityConverter.";
    }
}
