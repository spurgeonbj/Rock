using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Rock.MessageBus.Messages;

namespace Rock.MessageBus.Consumers
{
    public class EntityUpdateConsumer : IConsumer<IEntityUpdate>
    {
        public Task Consume( ConsumeContext<IEntityUpdate> context )
        {
            return Task.Run( () =>
            {
                Debug.WriteLine( $"Consuming {context.Message.EntityType}" );
            } );

            
        }
    }
}
