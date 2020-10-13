using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    [Table("Movie")]
    public class MovieNode
    {
        public string Title { get; set; }

        public int Year { get; set; }

        /// <summary>
        ///     (movie:Movie)&lt;-[rel:WROTE]-(writer:Writer:Person)
        ///     The ForeignKeyAttribute on the inverse property (Movies) of the Writer Class indicated the start of this outgoing
        ///     relationship.
        /// </summary>
        [InverseProperty("Movies")]
        public ICollection<WriterNode> Writers { get; set; }

        /// <summary>
        ///     This inverse property points to the <see cref="MovieNode" /> property on the <see cref="MovieActorRelationship" />
        ///     class, because the relationship information itself has been abstracted to that class.
        /// </summary>
        [InverseProperty("Movie")]
        public ICollection<MovieActorRelationship> Actors { get; set; }

        public DirectorNode Director { get; set; }

        [InverseProperty("Movies")] public ICollection<MovieExtraNode> Extras { get; set; }
    }
}