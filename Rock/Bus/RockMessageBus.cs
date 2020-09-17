// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using MassTransit;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using System;
using System.Threading.Tasks;

namespace Rock.Bus
{
    /// <summary>
    /// Rock Bus Process Controls: Start the bus
    /// </summary>
    public static class RockMessageBus
    {
        private static IBusControl _bus = null;

        /// <summary>
        /// Config
        /// </summary>
        private static class Config
        {
            // https://api.cloudamqp.com/console/8455ad06-9a01-421a-bdab-2a22f920aa4a/details
            internal static readonly string User = "njfbxtoz";
            internal static readonly string Password = "*** REDACTED ***";
            internal static readonly string Host = "coyote.rmq.cloudamqp.com";
        }

        /// <summary>
        /// Queue
        /// </summary>
        private static class Queue
        {
            internal static readonly string EntityUpdates = "rock-entity-updates";
            internal static readonly string Tasks = "rock-tasks";
        }

        /// <summary>
        /// Starts this bus.
        /// </summary>
        public static async Task Start()
        {
            _bus = MassTransit.Bus.Factory.CreateUsingRabbitMq( configurator =>
            {
                var url = $"amqps://{Config.User}:{Config.Password}@{Config.Host}/{Config.User}";

                configurator.Host( new Uri( url ), host => { } );

                configurator.ReceiveEndpoint( Queue.EntityUpdates, e =>
                {
                    e.Consumer<EntityWasUpdatedConsumer>();
                } );

                configurator.ReceiveEndpoint( Queue.Tasks, e =>
                {
                    e.Consumer<StartTaskConsumer>();
                } );
            } );

            await _bus.StartAsync();
        }

        /// <summary>
        /// Publishes the entity update.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task PublishEntityUpdate( IEntityWasUpdatedMessage message )
        {
            await _bus.Publish( message );
        }

        /// <summary>
        /// Publishes the entity update.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task SendStartTask( IStartTaskMessage message )
        {
            var url = $"rabbitmq://{Config.Host}:5671/{Config.User}/{Queue.Tasks}";
            var endpoint = await _bus.GetSendEndpoint( new Uri( url ) );
            await endpoint.Send( message );
        }
    }
}
