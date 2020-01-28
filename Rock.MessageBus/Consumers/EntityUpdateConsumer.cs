using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Rock.MessageBus.Messages;

namespace Rock.MessageBus.Consumers
{
    public class EntityUpdateConsumer : IConsumer<IEntityUpdate>
    {
        public async Task Consume( ConsumeContext<IEntityUpdate> context )
        {
            // so something
            var test = context.Message.EntityType;

            System.Threading.Thread.Sleep( 10000 );
        }
    }
}
