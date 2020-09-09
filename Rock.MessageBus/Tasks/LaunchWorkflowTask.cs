using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Rock.MessageBus.Tasks
{
    public class LaunchWorkflowTask : IRockTask, IConsumer<LaunchWorkflow>
    {
        public Task Consume( ConsumeContext<LaunchWorkflow> context )
        {
            return Task.Run( () =>
            {
                Debug.WriteLine( $"Consuming {context.Message.Message}" );
            } );
        }
    }

    public class LaunchWorkflow
    {
        public int WorkflowTypeId { get; set; }
        public string Message { get; set; }
    }
}
