using System;
using Neo4jClient.DataAnnotations.Utils;
using System.Collections.Generic;
using System.Text;

namespace Neo4jClient.DataAnnotations
{
    public interface IHaveEntityService
    {
        EntityService EntityService { get; }
    }
}
