using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Neo4jClient.DataAnnotations.Tests.Models
{
    [Table("Actor")]
    public class ActorNode : PersonNode
    {
        public string[] Roles { get; set; }

        public int TestForeignKeyId { get; set; }

        public object TestForeignKey { get; set; }

        public int TestMarkedFK { get; set; }

        [ForeignKey("TestMarkedFK")]
        public object TestMarkedForeignKey { get; set; }

        /// <summary>
        /// This inverse property points to the <see cref="ActorNode"/> property on the <see cref="MovieActorRelationship"/> class, because the relationship information itself has been abstracted to that class.
        /// </summary>
        [InverseProperty("Actor")]
        public virtual ICollection<MovieActorRelationship> Movies { get; set; }
    }

    public class ActorNode<T> : ActorNode where T : struct
    {
        public T? TestGenericForeignKeyId { get; set; }

        public object TestGenericForeignKey { get; set; }
    }
}
