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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Reporting;
using Rock.Security;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// </summary>
    [DisplayName( "Data View Results" )]
    [Category( "Reporting" )]
    [Description( "Shows the details of the given data view." )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]


    public partial class DataViewResults : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        #endregion Attribute Keys

        #region View State Keys

        private static class ViewStateKey
        {
            public const string ShowResults = "ShowResults";
        }

        #endregion View State Keys

        #region UserPreference Keys

        private static class UserPreferenceKey
        {
            public const string ShowResults = "data-view-show-results";
        }

        #endregion UserPreference Keys

        #region PageParameterKey

        private static class PageParameterKey
        {
            public const string DataViewId = "DataViewId";
        }

        #endregion PageParameterKey


        /// <summary>
        /// Gets or sets the visibility of the Results Grid for the Data View.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Results Grid is visible; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowResults
        {
            get
            {
                return ViewState[ViewStateKey.ShowResults].ToStringSafe().AsBoolean();
            }

            set
            {
                if ( this.ShowResults != value )
                {
                    ViewState[ViewStateKey.ShowResults] = value;

                    SetBlockUserPreference( UserPreferenceKey.ShowResults, value.ToString() );
                }

                pnlResultsGrid.Visible = this.ShowResults;

                if ( this.ShowResults )
                {
                    btnToggleResults.Text = "Hide Results <i class='fa fa-chevron-up'></i>";
                    btnToggleResults.ToolTip = "Hide Results";
                    btnToggleResults.RemoveCssClass( "btn-primary" );
                    btnToggleResults.AddCssClass( "btn-default" );
                }
                else
                {
                    btnToggleResults.Text = "Show Results <i class='fa fa-chevron-down'></i>";
                    btnToggleResults.RemoveCssClass( "btn-default" );
                    btnToggleResults.AddCssClass( "btn-primary" );
                    btnToggleResults.ToolTip = "Show Results";
                }

                if ( !this.ShowResults )
                {
                    return;
                }

                // Execute the Data View and show the results.
                var dataViewService = new DataViewService( new RockContext() );

                var dataViewId = hfDataViewId.Value.AsIntegerOrNull();
                BindGrid( dataViewId );
            }
        }

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
            gReport.GridRebind += gReport_GridRebind;

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }

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
                this.ShowResults = GetBlockUserPreference( UserPreferenceKey.ShowResults ).AsBoolean( true );
                var dataViewId = this.PageParameter( PageParameterKey.DataViewId ).AsIntegerOrNull();
                BindGrid( dataViewId );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            var service = new DataViewService( new RockContext() );
            var dataViewId = hfDataViewId.Value.AsIntegerOrNull();
            ShowReport( dataViewId );
        }

        /// <summary>
        /// Handles the Click event of the btnToggleResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnToggleResults_Click( object sender, EventArgs e )
        {
            this.ShowResults = !this.ShowResults;
        }

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

        /// <summary>
        /// Shows the report.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void ShowReport( int? dataViewId )
        {
            pnlResultsGrid.Visible = false;

            if ( !dataViewId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );
            if ( dataView == null )
            {
                return;
            }

            if ( !dataView.EntityTypeId.HasValue )
            {
                return;
            }

            if ( !dataView.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            bool isPersonDataSet = dataView.EntityTypeId == EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            if ( isPersonDataSet )
            {
                gReport.PersonIdField = "Id";
                gReport.DataKeyNames = new string[] { "Id" };
            }
            else
            {
                gReport.PersonIdField = null;
            }

            if ( dataView.EntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Get( dataView.EntityTypeId.Value, rockContext );
                if ( entityTypeCache != null )
                {
                    gReport.RowItemText = entityTypeCache.FriendlyName;
                }
            }

            pnlResultsGrid.Visible = true;

            BindGrid( dataView.Id );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="dataView">The data view.</param>
        /// <returns></returns>
        private void BindGrid( int? dataViewId )
        {
            if ( !dataViewId.HasValue )
            {
                return;
            }

            var dataView = new DataViewService( new RockContext() ).Get( dataViewId.Value );
            if ( dataView == null )
            {
                return;
            }

            gReport.DataSource = null;

            // Only respect the ShowResults option if fetchRowCount is null
            if ( !this.ShowResults )
            {
                return;
            }

            // Making an unsaved copy of the DataView so the runs do not get counted.
            var dv = new DataView
            {
                Name = dataView.Name,
                TransformEntityTypeId = dataView.TransformEntityTypeId,
                TransformEntityType = dataView.TransformEntityType,
                EntityTypeId = dataView.EntityTypeId,
                EntityType = dataView.EntityType,
                DataViewFilterId = dataView.DataViewFilterId,
                DataViewFilter = dataView.DataViewFilter,
                IncludeDeceased = dataView.IncludeDeceased
            };

            if ( !dv.EntityTypeId.HasValue )
            {
                return;
            }

            var cachedEntityType = EntityTypeCache.Get( dv.EntityTypeId.Value );

            if ( cachedEntityType == null || cachedEntityType.AssemblyName == null )
            {
                return;
            }

            Type entityType = cachedEntityType.GetEntityType();
            if ( entityType == null )
            {
                return;
            }

            try
            {
                gReport.CreatePreviewColumns( entityType );
                var dbContext = dv.GetDbContext();
                var dataViewGetQueryArgs = new DataViewGetQueryArgs
                {
                    SortProperty = gReport.SortProperty,
                    DbContext = dbContext,
                    DatabaseTimeoutSeconds = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180
                };

                var qry = dv.GetQuery( dataViewGetQueryArgs );

                gReport.SetLinqDataSource( qry.AsNoTracking() );
                gReport.DataBind();
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                var errorBox = nbGridError;

                if ( sqlTimeoutException != null )
                {
                    errorBox.NotificationBoxType = NotificationBoxType.Warning;
                    errorBox.Text = "This dataview did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                    return;
                }
                else
                {
                    if ( ex is RockDataViewFilterExpressionException )
                    {
                        RockDataViewFilterExpressionException rockDataViewFilterExpressionException = ex as RockDataViewFilterExpressionException;
                        errorBox.Text = rockDataViewFilterExpressionException.GetFriendlyMessage( dataView );
                    }
                    else
                    {
                        errorBox.Text = "There was a problem with one of the filters for this data view.";
                    }

                    errorBox.NotificationBoxType = NotificationBoxType.Danger;

                    errorBox.Details = ex.Message;
                    errorBox.Visible = true;
                    return;
                }
            }

            if ( dv.EntityTypeId.HasValue )
            {
                gReport.RowItemText = EntityTypeCache.Get( dv.EntityTypeId.Value ).FriendlyName;
            }

            if ( gReport.DataSource != null )
            {
                gReport.ExportFilename = dv.Name;
            }
        }

        #endregion
    }
}