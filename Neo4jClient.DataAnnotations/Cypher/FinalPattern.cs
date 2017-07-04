using System;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations.Cypher
{
    public class FinalPattern
    {
        public string AParameter { get; set; }
        public string RParameter { get; set; }
        public string BParameter { get; set; }

        public Tuple<int?, int?> RHops { get; set; }

        public List<string> ALabels { get; set; }
        public List<string> RTypes { get; set; }
        public List<string> BLabels { get; set; }

        public Dictionary<string, string> AFinalProperties { get; set; }
        public Dictionary<string, string> RFinalProperties { get; set; }
        public Dictionary<string, string> BFinalProperties { get; set; }

        public bool isExtension { get; set; }
    }
}
