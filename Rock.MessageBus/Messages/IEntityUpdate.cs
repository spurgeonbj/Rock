using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.MessageBus.Messages
{
    public interface IEntityUpdate
    {
        int Id { get; }
        int EntityTypeId { get; }
        int EntityId { get; }
        string EntityType { get; }
        string Entity { get; }
    }
}
