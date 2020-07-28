﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    public class WriterNode : PersonNode
    {
        /// <summary>
        ///     (writer:Writer:Person)-[rel:WROTE]->(movie:Movie)
        ///     The ColumnAttribute or ForeignKeyAttribute determines the start of an outgoing relationship
        /// </summary>
        [ForeignKey("WROTE")]
        [InverseProperty("Writers")]
        public ICollection<MovieNode> Movies { get; set; }
    }
}