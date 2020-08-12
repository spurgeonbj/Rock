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

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Owin;
using Owin.Security.OpenIdConnect.Server;
using Rock.Oidc.Authorization;
using Rock.Oidc.Configuration;
using Rock.Web.Cache;

namespace Rock.Oidc
{
    public static class Startup
    {
        /// <summary>
        /// Method that will be run at Rock Owin startup
        /// </summary>
        /// <param name="app"></param>
        public static void OnStartup( IAppBuilder app )
        {
            // TODO: Update to pull from system settings.
            var rockOidcSettings = RockOidcSettings.GetDefaultSettings();
            app.UseOAuthValidation();

            // Insert a new cookies middleware in the pipeline to store the user
            // identity after he has been redirected from the identity provider.
            app.UseCookieAuthentication( new CookieAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AuthenticationType = "ClientCookie",
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "ClientCookie",
                ExpireTimeSpan = TimeSpan.FromMinutes( 5 )
            } );

            // Insert a new OIDC client middleware in the pipeline.
            

            app.SetDefaultSignInAsAuthenticationType( "ClientCookie" );

            app.UseOpenIdConnectServer( options =>
            {
                options.Provider = new AuthorizationProvider();

                options.Issuer = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash() );

                // TODO: Perhaps configurable in in v2?
                options.AuthorizationEndpointPath = new PathString( Paths.AuthorizePath );
                options.LogoutEndpointPath = new PathString( Paths.LogoutPath );
                options.TokenEndpointPath = new PathString( Paths.TokenPath );
                options.UserinfoEndpointPath = new PathString( Paths.UserInfo );

                options.AccessTokenLifetime = TimeSpan.FromSeconds( rockOidcSettings.AccessTokenLifetime );
                options.IdentityTokenLifetime = TimeSpan.FromSeconds( rockOidcSettings.IdentityTokenLifetime );
                options.RefreshTokenLifetime = TimeSpan.FromSeconds( rockOidcSettings.RefreshTokenLifetime );

                options.ApplicationCanDisplayErrors = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;
                options.AllowInsecureHttp = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;

                var rockSigningCredentials = new RockOidcSigningCredentials( rockOidcSettings );

                foreach ( var key in rockSigningCredentials.SigningKeys )
                {
                    options.SigningCredentials.AddKey( new RsaSecurityKey( key ) );
                }
            } );

            app.UseOpenIdConnectAuthentication( new OpenIdConnectAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AuthenticationType = "OpenIdConnectClient",
                // Note: setting the Authority allows the OIDC client middleware to automatically
                // retrieve the identity provider's configuration and spare you from setting
                // the different endpoints URIs or the token validation parameters explicitly.
                Authority = "http://localhost:54541/",

                // Note: these settings must match the application details inserted in
                // the database at the server level (see ApplicationContextInitializer.cs).
                ClientId = "myRockClient",
                ClientSecret = "secret_secret_secret",
                RedirectUri = "http://localhost:6229/oidc",
                PostLogoutRedirectUri = "http://localhost:6229/",

                Scope = "openid profile",

                // Note: by default, the OIDC client throws an OpenIdConnectProtocolException
                // when an error occurred during the authentication/authorization process.
                // To prevent a YSOD from being displayed, the response is declared as handled.
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = notification =>
                    {
                        if ( string.Equals( notification.ProtocolMessage.Error, "access_denied", StringComparison.Ordinal ) )
                        {
                            notification.HandleResponse();

                            notification.Response.Redirect( "/" );
                        }

                        return Task.FromResult( 0 );
                    },

                    // Retrieve an access token from the remote token endpoint
                    // using the authorization code received during the current request.
                    AuthorizationCodeReceived = async notification =>
                    {
                        using ( var client = new HttpClient() )
                        {
                            var configuration = await notification.Options.ConfigurationManager.GetConfigurationAsync( notification.Request.CallCancelled );

                            var request = new HttpRequestMessage( HttpMethod.Post, configuration.TokenEndpoint );
                            request.Content = new FormUrlEncodedContent( new Dictionary<string, string>
                            {
                                [OpenIdConnectParameterNames.ClientId] = notification.Options.ClientId,
                                [OpenIdConnectParameterNames.ClientSecret] = notification.Options.ClientSecret,
                                [OpenIdConnectParameterNames.Code] = notification.ProtocolMessage.Code,
                                [OpenIdConnectParameterNames.GrantType] = "authorization_code",
                                [OpenIdConnectParameterNames.RedirectUri] = notification.Options.RedirectUri
                            } );

                            var response = await client.SendAsync( request, notification.Request.CallCancelled );
                            response.EnsureSuccessStatusCode();

                            var payload = JObject.Parse( await response.Content.ReadAsStringAsync() );

                            // Add the access token to the returned ClaimsIdentity to make it easier to retrieve.
                            notification.AuthenticationTicket.Identity.AddClaim( new Claim(
                                type: OpenIdConnectParameterNames.AccessToken,
                                value: payload.Value<string>( OpenIdConnectParameterNames.AccessToken ) ) );

                            // Add the identity token to the returned ClaimsIdentity to make it easier to retrieve.
                            var idToken = payload.Value<string>( OpenIdConnectParameterNames.IdToken );
                            notification.AuthenticationTicket.Identity.AddClaim( new Claim(
                                type: OpenIdConnectParameterNames.IdToken,
                                value:  idToken) );

                            var jwtTokenHandler = new JwtSecurityTokenHandler();
                            var jwtToken = jwtTokenHandler.ReadJwtToken( idToken );

                            var userName = jwtToken.Claims.Where( c => c.Type == "sub" || c.Type == "name" ).FirstOrDefault()?.Value;
                            if ( userName.IsNotNullOrWhiteSpace() )
                            {
                                Rock.Security.Authorization.SetAuthCookie( notification.Response, userName, false, false );
                            }
                        }
                    },
                    // Attach the id_token stored in the authentication cookie to the logout request.
                    RedirectToIdentityProvider = notification =>
                    {
                        if ( notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest )
                        {
                            var token = notification.OwinContext.Authentication.User?.FindFirst( OpenIdConnectParameterNames.IdToken );
                            if ( token != null )
                            {
                                notification.ProtocolMessage.IdTokenHint = token.Value;
                            }
                        }

                        return Task.FromResult( 0 );
                    }
                }
            } );
        }
    }
}