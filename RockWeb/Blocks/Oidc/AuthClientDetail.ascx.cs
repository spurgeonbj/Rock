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
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Oidc
{
    [DisplayName( "Open Id Connect Client Detail" )]
    [Category( "Oidc" )]
    [Description( "Displays the details of the given Open Id Connect Client." )]
    public partial class AuthClientDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        private class PageParameterKeys
        {
            public const string AuthClientId = "AuthClientId";
        }

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            AddClaimsCheckboxes();
            base.OnInit( e );
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
                var clientId = PageParameter( PageParameterKeys.AuthClientId ).AsIntegerOrNull();
                if ( clientId == null )
                {
                    DisplayErrorMessage( "No Auth Scope Id was specified." );
                    return;
                }

                ShowDetail( clientId.Value );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var clientId = PageParameter( PageParameterKeys.AuthClientId ).AsIntegerOrNull();
            if ( clientId == null )
            {
                DisplayErrorMessage( "No Auth Client Id was specified." );
                return;
            }

            SaveAuthClient( clientId.Value );
            NavigateToParentPage();
        }

        private void SaveAuthClient( int authScopeId )
        {
            var isNew = authScopeId.Equals( 0 );

            var authClient = new AuthClient();

            var editAllowed = authClient.IsAuthorized( Authorization.EDIT, CurrentPerson );
            if ( !editAllowed )
            {
                DisplayErrorMessage( "The current user is not authorized to make changes." );
                return;
            }

            var rockContext = new RockContext();
            var authClientService = new AuthClientService( rockContext );
            if ( isNew )
            {
                authClientService.Add( authClient );
            }
            else
            {
                authClient = authClientService.Get( authScopeId );
            }

            if ( authClient == null )
            {
                DisplayErrorMessage( "The Auth Client with the specified Id was found." );
                return;
            }

            authClient.Name = tbName.Text;
            authClient.IsActive = cbActive.Checked;

            var activeClaims = GetActiveClaims( rockContext ).Select( ac => ac.ScopeName ).Distinct();
            var selectedClaims = new List<string>( activeClaims.Count() );
            var selectedScopes = new List<string>( activeClaims.Count() );
            foreach ( var scope in activeClaims )
            {
                var checkboxList = litClaims.FindControl( scope ) as RockCheckBoxList;
                if ( checkboxList == null )
                {
                    continue;
                }
                var selectedScopeClaims = checkboxList.SelectedValues;
                selectedClaims.AddRange( selectedScopeClaims );
                if ( selectedScopeClaims.Any() )
                {
                    selectedScopes.Add( scope );
                }
            }

            authClient.AllowedClaims = selectedClaims.ToJson();
            authClient.AllowedScopes = selectedScopes.ToJson();
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        #endregion

        #region Internal Methods
        private void DisplayErrorMessage( string message )
        {
            nbWarningMessage.Text = message;
            nbWarningMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
            nbWarningMessage.Visible = true;
            pnlEditDetails.Visible = false;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="clientId">The rest user identifier.</param>
        public void ShowDetail( int clientId )
        {
            var rockContext = new RockContext();

            AuthClient authClient = null;
            var isNew = clientId.Equals( 0 );
            if ( !isNew )
            {
                authClient = new AuthClientService( rockContext ).Get( clientId );
                lTitle.Text = ActionTitle.Edit( "Client" ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Client" ).FormatAsHtmlTitle();
            }

            if ( authClient == null )
            {
                if ( !isNew )
                {
                    DisplayErrorMessage( "The Auth Client with the specified Id was found." );
                    return;
                }

                authClient = new AuthClient { Id = 0 };
            }

            hfRestUserId.Value = authClient.Id.ToString();

            tbName.Text = authClient.Name;
            cbActive.Checked = authClient.IsActive;
            tbClientId.Text = authClient.ClientId;
            tbClientSecret.Text = authClient.ClientSecretHash;
            tbRedirectUri.Text = authClient.RedirectUri;
            tbPostLogoutRedirectUri.Text = authClient.PostLogoutRedirectUri;

            SetClaimsCheckboxValues( authClient.AllowedClaims.FromJsonOrNull<List<string>>() );
            var editAllowed = authClient.IsAuthorized( Authorization.EDIT, CurrentPerson );
            lbSave.Visible = editAllowed;
        }

        private void AddClaimsCheckboxes()
        {
            IEnumerable<ClaimsModel> availableClaims = null;
            using ( var rockContext = new RockContext() )
            {
                availableClaims = GetActiveClaims( rockContext ).ToList();
            }

            var currentScope = string.Empty;
            RockCheckBoxList checkboxList = null;
            foreach ( var claim in availableClaims )
            {
                if ( !currentScope.Equals( claim.ScopeName, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( checkboxList != null )
                    {
                        litClaims.Controls.Add( checkboxList );
                    }

                    currentScope = claim.ScopeName;
                    checkboxList = new RockCheckBoxList();
                    checkboxList.ID = claim.ScopeName;
                    checkboxList.Label = claim.ScopePublicName + " (" + claim.ScopeName + ")";
                }
                checkboxList.Items.Add( new ListItem
                {
                    Text = claim.ClaimPublicName + " (" + claim.ClaimName + ")",
                    Value = claim.ClaimName,
                } );
            }

            if ( checkboxList != null )
            {
                litClaims.Controls.Add( checkboxList );
            }
        }

        private void SetClaimsCheckboxValues( IEnumerable<string> allowedClaims )
        {
            if ( allowedClaims == null )
            {
                return;
            }

            // Get the checkboxs
            var checkBoxes = litClaims.ControlsOfTypeRecursive<RockCheckBoxList>();

            foreach ( var checkbox in checkBoxes )
            {
                foreach ( ListItem item in checkbox.Items )
                {
                    item.Selected = allowedClaims.Contains( item.Value );
                }
            }
        }

        private IEnumerable<ClaimsModel> GetActiveClaims( RockContext rockContext )
        {
            var authClaimService = new AuthClaimService( rockContext );
            return authClaimService
                .Queryable()
                .AsNoTracking()
                .Where( ac => ac.IsActive )
                .Where( ac => ac.Scope.IsActive )
                .Select( ac => new ClaimsModel
                {
                    ClaimName = ac.Name,
                    ClaimPublicName = ac.PublicName,
                    ScopeName = ac.Scope.Name,
                    ScopePublicName = ac.Scope.PublicName
                } )
                .OrderBy( ac => ac.ScopePublicName )
                .ThenBy( ac => ac.ClaimName );
        }

        #endregion

        private class ClaimsModel
        {
            public string ScopeName { get; set; }
            public string ScopePublicName { get; set; }
            public string ClaimName { get; set; }
            public string ClaimPublicName { get; set; }
        }
    }
}