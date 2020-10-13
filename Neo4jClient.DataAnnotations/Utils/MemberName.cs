using System;

namespace Neo4jClient.DataAnnotations.Utils
{
    internal class MemberName : IEquatable<MemberName>
    {
        public MemberName(string actual, string complexActual, string json, string complexJson)
        {
            Actual = actual;
            ComplexActual = complexActual;
            Json = json;
            ComplexJson = complexJson;
        }

        public static MemberName Empty { get; set; } = new MemberName("", "", "", "");

        public string Actual { get; }
        public string ComplexActual { get; }

        public string Json { get; }
        public string ComplexJson { get; }

        public bool Equals(MemberName other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return ComplexActual == other?.ComplexActual && ComplexJson == other?.ComplexJson;
        }

        public override string ToString()
        {
            try
            {
                return $"({ComplexActual}, {ComplexJson})";
            }
            catch
            {
            }

            return base.ToString();
        }
    }
}