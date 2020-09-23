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

using Humanizer.Configuration;
using MassTransit;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Transport;
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Rock.Bus
{
    /// <summary>
    /// Rock Bus Process Controls: Start the bus
    /// </summary>
    public static class RockMessageBus
    {
        /// <summary>
        /// The of an entity that will cause publishing a message on the <see cref="RockMessageBus"/>
        /// </summary>
        private static readonly HashSet<EntityState> _statesToPublishOnBus = new HashSet<EntityState> {
            EntityState.Added,
            EntityState.Modified,
            EntityState.Deleted
        };

        /// <summary>
        /// The send endpoints
        /// </summary>
        private static Dictionary<string, ISendEndpoint> _sendEndpoints = new Dictionary<string, ISendEndpoint>();

        /// <summary>
        /// The bus
        /// </summary>
        private static IBusControl _bus = null;

        /// <summary>
        /// The transport component
        /// </summary>
        private static TransportComponent _transportComponent = null;

        /// <summary>
        /// Queue Names
        /// </summary>
        public static class QueueName
        {
            /// <summary>
            /// The entity updates
            /// </summary>
            public static readonly string EntityUpdates = "rock-entity-updates";

            /// <summary>
            /// The tasks
            /// </summary>
            public static readonly string Tasks = "rock-tasks";
        }

        /// <summary>
        /// Starts this bus.
        /// </summary>
        public static async Task Start()
        {
            _transportComponent = TransportContainer.Instance.Components.First().Value.Value;

            _bus = _transportComponent.GetBusControl( configurator => {
                configurator.ReceiveEndpoint( QueueName.EntityUpdates, e =>
                {
                    e.Consumer<EntityWasUpdatedConsumer>();
                } );

                configurator.ReceiveEndpoint( QueueName.Tasks, e =>
                {
                    e.Consumer<StartTaskConsumer>();
                } );
            } );

            await _bus.StartAsync();
        }

        /// <summary>
        /// Should entity updates be published for this entity type.
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static bool ShouldPublishEntityUpdate( EntityState entityState, int entityTypeId )
        {
            return
                _statesToPublishOnBus.Contains( entityState ) &&
                ( EntityTypeCache.Get( entityTypeId )?.IsMessageBusEventPublishEnabled ?? false );
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
        public static async Task SendStartTask( IEventBusTransaction message )
        {
            var endpoint = _sendEndpoints.GetValueOrNull( QueueName.Tasks );

            if (endpoint == null)
            {
                endpoint = _transportComponent.GetSendEndpoint( _bus, QueueName.Tasks );
                _sendEndpoints[QueueName.Tasks] = endpoint;
            }

            await endpoint.Send( message );
        }
    }
}
