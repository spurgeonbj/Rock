using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json.Linq;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Security.ExternalAuthentication
{
    /// <summary>
    /// Authenticates a user using the specified OIDC Client.
    /// </summary>
    [Description( "OIDC Authentication Provider" )]
    [Export( typeof( AuthenticationComponent ) )]
    [ExportMetadata( "ComponentName", "OidcClient" )]

    [TextField( "App ID",
        Description = "The OIDC Client App ID",
        Key = AttributeKey.ApplicationId,
        Order = 1 )]
    [TextField( "App Secret",
        Description = "The OIDC Client Secret",
        Key = AttributeKey.ApplicationSecret,
        Order = 2 )]
    [TextField( "Authentication Server",
        Description = "The OIDC Server",
        Key = AttributeKey.AuthenticationServer,
        Order = 3 )]
    [TextField( "Redirect URI",
        Description = "The URI that the authentication server should redirect to once authentication is complete.",
        Key = AttributeKey.RedirectUri,
        Order = 4 )]
    [TextField( "Post Logout Redirect URI",
        Description = "The URI that the authentication server should redirect to once the user has been logged out.",
        Key = AttributeKey.PostLogoutRedirectUri,
        Order = 5 )]
    // TODO: Figure out how to handle requested claims.
    //[EnumsField(
    //    "Requested Claims",
    //    Description = "The Claims you wish to request from the server.",
    //    EnumSourceType = typeof( ContentChannelItemStatus ),
    //    IsRequired = false,
    //    DefaultValue = "2",
    //    Category = "CustomSetting",
    //    Key = AttributeKey.Status )]
    public class OidcClient : AuthenticationComponent
    {
        /// <summary>
        /// Attribute Keys
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The application identifier
            /// </summary>
            public const string ApplicationId = "AppId";
            /// <summary>
            /// The application secret
            /// </summary>
            public const string ApplicationSecret = "AppSecret";
            /// <summary>
            /// The authentication server
            /// </summary>
            public const string AuthenticationServer = "AuthServer";
            /// <summary>
            /// The requested claims
            /// </summary>
            public const string RequestedClaims = "RequestedClaims";
            /// <summary>
            /// The redirect URI
            /// </summary>
            public const string RedirectUri = "RedirectUri";
            /// <summary>
            /// The post logout redirect URI
            /// </summary>
            public const string PostLogoutRedirectUri = "PostLogoutRedirectUri";

        }
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType => AuthenticationServiceType.External;

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication => true;

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsChangePassword => false;

        public override bool Authenticate( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        public override bool Authenticate( HttpRequest request, out string userName, out string returnUrl )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns></returns>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            warningMessage = "not supported";
            return false;
        }

        public override string EncodePassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        public override Uri GenerateLoginUrl( HttpRequest request )
        {
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl( request );

            // TODO: add requested scopes
            return new Uri( $"{GetLoginUrl()}?client_id={GetAttributeValue( AttributeKey.ApplicationId )}&redirect_uri={HttpUtility.UrlEncode( redirectUri )}&state={HttpUtility.UrlEncode( returnUrl ?? FormsAuthentication.DefaultUrl )}&scope=profile,email");
        }

        public override string ImageUrl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override bool IsReturningFromAuthentication( HttpRequest request )
        {
            return ( !String.IsNullOrWhiteSpace( request.QueryString["code"] ) &&
                !String.IsNullOrWhiteSpace( request.QueryString["state"] ) );
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SetPassword( UserLogin user, string password )
        {
            throw new NotImplementedException();
        }

        // TODO: this is copied and pasted from facebook should probably standardize.
        private string GetRedirectUrl( HttpRequest request )
        {
            Uri uri = new Uri( request.Url.ToString() );
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }

        private string GetLoginUrl()
        {
            var authServer = GetAttributeValue( AttributeKey.AuthenticationServer ).EnsureTrailingForwardslash();

            // TODO: Cache config so we don't have to make multiple calls.
            var request = new HttpRequestMessage( HttpMethod.Get, $"{authServer}.well-known/openid-configuration" );
            JObject payload = null;
            using ( var client = new HttpClient() )
            {
                var response = client.SendAsync( request ).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                payload = JObject.Parse( response.Content.ReadAsStringAsync().GetAwaiter().GetResult() );
            }

            if ( payload == null )
            {
                return string.Empty;
            }

            var issuer = payload.Value<string>( "issuer" );
            if ( !string.Equals( issuer.EnsureTrailingForwardslash(), authServer, StringComparison.InvariantCultureIgnoreCase ) )
            {
                return string.Empty;
            }

            return payload.Value<string>( "authorization_endpoint" );
        }
    }
}
