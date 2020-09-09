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
using System.Dynamic;
using System.Web.UI;
using MassTransit;
using Rock.Attribute;
using Rock.MessageBus.Messages;
using Rock.MessageBus.Tasks;
using Rock.Model;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Stark Detail" )]
    [Category( "Utility" )]
    [Description( "Template block for developers to use to start a new detail block." )]

    #region Block Attributes

    [BooleanField(
        "Show Email Address",
        Key = AttributeKey.ShowEmailAddress,
        Description = "Should the email address be shown?",
        DefaultBooleanValue = true,
        Order = 1 )]

    [EmailField(
        "Email",
        Key = AttributeKey.Email,
        Description = "The Email address to show.",
        DefaultValue = "ted@rocksolidchurchdemo.com",
        Order = 2 )]

    #endregion Block Attributes
    public partial class StarkDetail : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string Email = "Email";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
                var bus = ( IBus ) Global.MessageBusContext;

                // Send
                //var endpoint = bus.GetSendEndpoint( new Uri( "queue:entity_updates" ) );
                //var sendEndpoint = endpoint.Result;
                //sendEndpoint.Send<IEntityUpdate>( new { EntityType = "test send" } );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion

        protected void btnFire_Click( object sender, EventArgs e )
        {
            

            var bus = (IBus) Global.MessageBusContext;

            // Send
            var endpoint = bus.GetSendEndpoint( new Uri( "queue:entity_updates" ) );
            var sendEndpoint = endpoint.Result;
            sendEndpoint.Send<IEntityUpdate>( new { EntityType = "test send 3" } );

            var endpoint2 = bus.GetSendEndpoint( new Uri( "queue:rock_tasks" ) );
            var sendEndpoint2 = endpoint2.Result;
            sendEndpoint2.Send<LaunchWorkflow>( new LaunchWorkflow { WorkflowTypeId = 1, Message = "Test 1" } );
            sendEndpoint2.Send<WriteInteraction>( new WriteInteraction { InteractionChannel = 1, InteractionComponent = 2, Operation ="Hello" } );

            // Publish
            //bus.Publish<IEntityUpdate>( new { EntityType = "test publish" } );
        }
    }
}