using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Neo4jClient.DataAnnotations.Utils;

namespace Neo4jClient.DataAnnotations.Expressions
{
    public class MemberGroup : List<MemberInfo>
    {
        public MemberGroup(bool isInversePath)
        {
        }

        public MemberGroup(IEnumerable<MemberInfo> collection, bool isInversePath) : base(collection)
        {
        }

        public MemberGroup(int capacity, bool isInversePath) : base(capacity)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is IEnumerable<MemberInfo> otherGroup)
            {
                if (ReferenceEquals(this, otherGroup))
                    return true;

                if (otherGroup is IList<MemberInfo> otherListGroup
                    && Count != otherListGroup.Count)
                    return false;
                else if (otherGroup is MemberInfo[] otherArrayGroup
                    && Count != otherArrayGroup.Length)
                    return false;

                int i = -1;
                foreach (var otherItem in otherGroup)
                {
                    i++;

                    if (i >= Count)
                        return false;

                    var thisItem = this[i];

                    if (thisItem != otherItem && thisItem?.IsEquivalentTo(otherItem) != true)
                        return false;
                }

                return true;
            }

            return base.Equals(obj);
        }

        public bool IsInversePath { get; set; }

        public MemberInfo TargetMember
        {
            get
            {
                if (IsInversePath)
                    return this.FirstOrDefault();

                return this.LastOrDefault();
            }
        }

        public MemberInfo RootMember
        {
            get
            {
                if (IsInversePath)
                    return this.LastOrDefault();

                return this.FirstOrDefault();
            }
        }

        public string ComplexName
        {
            get
            {
                if (IsInversePath)
                {
                    return this.Select(_m => _m.Name).Reverse().Aggregate((x, y) => $"{x}{Defaults.ComplexTypeNameSeparator}{y}");
                }

                return this.Select(_m => _m.Name).Aggregate((x, y) => $"{x}{Defaults.ComplexTypeNameSeparator}{y}");
            }
        }
    }
}
