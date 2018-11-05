using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    /// <summary>
    /// (actor:Actor:Person)-[rel:ACTED_IN]->(movie:Movie)
    /// The higher column order among the keys determine the start of an outgoing relationship
    /// </summary>
    [Table("ACTED_IN")]
    public class MovieActorRelationship
    {
        [Key]
        [Column(Order = 1)]
        public string MovieId { get; set; }

        public MovieNode Movie { get; set; }

        [Key]
        [Column(Order = 2)]
        public string Actorid { get; set; }

        [ForeignKey("Actorid")]
        public ActorNode Actor { get; set; }

        public string[] Roles { get; set; }
    }
}
