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
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using MassTransit;

namespace Rock.Bus.Transport
{
    /// <summary>
    /// Bus Transport using RabbitMQ
    /// </summary>
    [Description( "Use RabbitMq as the bus transport" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "RabbitMQ" )]

    public class RabbitMq : TransportComponent
    {
        /// <summary>
        /// Config (TODO move to attributes)
        /// </summary>
        private static class Config
        {
            // https://api.cloudamqp.com/console/a0c463fd-c62c-463e-8903-849d9e99cacf/details
            internal static readonly string User = "njfbxtoz";
            internal static readonly string Password = "*** REDACTED ***";
            internal static readonly string Host = "coyote.rmq.cloudamqp.com";
        }

        /// <summary>
        /// Gets the bus control.
        /// </summary>
        /// <param name="configureEndpoints">Call this within your configuration function to add the
        /// endpoints with appropriate queues.</param>
        /// <returns></returns>
        public override IBusControl GetBusControl( Action<IBusFactoryConfigurator> configureEndpoints )
        {
            return MassTransit.Bus.Factory.CreateUsingRabbitMq( configurator =>
            {
                var url = $"amqps://{Config.User}:{Config.Password}@{Config.Host}/{Config.User}";
                configurator.Host( new Uri( url ), host => { } );
                configureEndpoints( configurator );
            } );
        }

        /// <summary>
        /// Gets the send endpoint.
        /// </summary>
        /// <param name="bus">The bus.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public override ISendEndpoint GetSendEndpoint( IBusControl bus, string queueName )
        {
            var url = $"rabbitmq://{Config.Host}:5671/{Config.User}/{queueName}";
            return bus.GetSendEndpoint( new Uri( url ) ).Result;
        }
    }
}
