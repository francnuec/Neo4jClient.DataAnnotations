﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    [Table("Extra")]
    public class MovieExtraNode : PersonNode
    {
        /// <summary>
        ///     This inverse property points to the <see cref="MovieExtraNode" /> property on the <see cref="MovieNode" /> class
        /// </summary>
        [InverseProperty("Extras")]
        public ICollection<MovieNode> Movies { get; set; }
    }
}