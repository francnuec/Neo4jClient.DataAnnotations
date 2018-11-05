using System;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Utils
{
    internal class MemberName : IEquatable<MemberName>
    {
        public static MemberName Empty { get; set; } = new MemberName("", "");

        public string Actual { get; }
        public string Json { get; }

        public MemberName(string actual, string json)
        {
            Actual = actual;
            Json = json;
        }

        public bool Equals(MemberName other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return Actual == other?.Actual && Json == other?.Json;
        }

        public override string ToString()
        {
            try
            {
                return $"({Actual}, {Json})";
            }
            catch
            {

            }
            return base.ToString();
        }
    }
}
