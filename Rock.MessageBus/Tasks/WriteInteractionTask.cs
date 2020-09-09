using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Rock.MessageBus.Tasks
{
    public class WriteInteractionTask : IRockTask, IConsumer<WriteInteraction>
    {
        public Task Consume( ConsumeContext<WriteInteraction> context )
        {
            return Task.Run( () =>
            {
                Debug.WriteLine( $"Consuming {context.Message.Operation}" );
            } );
        }
    }

    public class WriteInteraction
    {
        public int InteractionChannel { get; set; }
        public int InteractionComponent { get; set; }
        public string Operation { get; set; }
    }
}
