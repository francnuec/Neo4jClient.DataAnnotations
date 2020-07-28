﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    [Table("Director")]
    public class DirectorNode : PersonNode
    {
        private ActorNode what;

        private string whatever;

        public DirectorNode(string whatever, ActorNode what)
        {
            this.whatever = whatever;
            this.what = what;
        }

        /// <summary>
        ///     (director:Director:Person)-[rel:DIRECTED]->(movie:Movie)
        ///     We are assuming a sole director for each movie for the purpose of testing. That is a one-to-many relationship.
        ///     The ColumnAttribute or ForeignKeyAttribute determines the start of an outgoing relationship
        /// </summary>
        [Column("DIRECTED")]
        [InverseProperty("Director")]
        public ICollection<MovieNode> Movies { get; set; }
    }
}