using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rock.Security.ExternalAuthentication
{
    public static class ClaimExtensionMethods
    {
        public static string GetClaimValue( this JwtSecurityToken idToken, string claim )
        {
            if ( idToken == null )
            {
                return string.Empty;
            }

            if ( claim.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return idToken.Claims.Where( c => c.Type == claim ).Select( c => c.Value ).FirstOrDefault();
        }
    }

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
            userName = string.Empty;
            returnUrl = HttpContext.Current.Session["oidc-url"].ToString();
            string redirectUri = GetRedirectUrl( request );
            var code = request.QueryString["code"];

            var state = request.QueryString["state"];

            //if ( HttpContext.Current.Session["oidc-nonce"].ToString() != nonce )
            //{
            //    ExceptionLogService.LogException( new Exception( "Open Id Connect nonce did not match." ), HttpContext.Current );
            //    return false;
            //}

            try
            {
                var client = new TokenClient( GetTokenUrl(), GetAttributeValue( AttributeKey.ApplicationId ), GetAttributeValue( AttributeKey.ApplicationSecret ) );
                var response = client.RequestAuthorizationCodeAsync( code, redirectUri ).GetAwaiter().GetResult();

                if ( response.IsError )
                {
                    throw new Exception( response.Error );
                }

                var nonce = HttpContext.Current.Session["oidc-nonce"].ToString();
                var idToken = GetValidatedIdToken( response.IdentityToken, nonce );
                userName = HandleOidcUserAddUpdate( idToken, response.AccessToken );

                HttpContext.Current.Session["oidc-nonce"] = string.Empty;
                HttpContext.Current.Session["oidc-url"] = string.Empty;
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
            }

            return !string.IsNullOrWhiteSpace( userName );
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

            var nonce = EncodeBcrypt( System.Guid.NewGuid().ToString() );
            var state = EncodeBcrypt( System.Guid.NewGuid().ToString() );

            HttpContext.Current.Session["oidc-nonce"] = nonce;
            HttpContext.Current.Session["oidc-state"] = state;
            HttpContext.Current.Session["oidc-url"] = returnUrl;
            
            // TODO: add requested scopes
            return new Uri( GetLoginUrl( GetRedirectUrl( request ), nonce, state ) );
        }

        public override string ImageUrl()
        {
            return ""; /*~/Assets/Images/facebook-login.png*/
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override bool IsReturningFromAuthentication( HttpRequest request )
        {
            // TODO: Add xrf validation
            return !String.IsNullOrWhiteSpace( request.QueryString["code"] ) && !String.IsNullOrWhiteSpace( request.QueryString["state"] );
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
            return GetAttributeValue( AttributeKey.RedirectUri );

            //Uri uri = new Uri( request.Url.ToString() );
            //return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }

        private string GetLoginUrl( string redirectUrl, string nonce, string state )
        {
            var config = GetOpenIdConnectConfiguration();

            var requestUrl = new RequestUrl( config.AuthorizationEndpoint );

            return requestUrl.CreateAuthorizeUrl( GetAttributeValue( AttributeKey.ApplicationId ),
                OidcConstants.ResponseTypes.Code,
                "openid email profile",
                $"{redirectUrl}",
                state,
                nonce );
        }

        private string GetTokenUrl()
        {
            var config = GetOpenIdConnectConfiguration();
            return config.TokenEndpoint;
        }

        private JwtSecurityToken GetValidatedIdToken( string idToken, string masterNonce )
        {
            var authServer = GetAttributeValue( AttributeKey.AuthenticationServer ).EnsureTrailingForwardslash();
            var config = GetOpenIdConnectConfiguration();

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = GetAttributeValue( AttributeKey.ApplicationId ),
                ValidateIssuer = true,
                ValidIssuer = authServer,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

            _ = tokendHandler.ValidateToken( idToken, validationParameters, out var token );

            var validatedToken = token as JwtSecurityToken;

            if ( validatedToken == null || masterNonce.IsNullOrWhiteSpace() || masterNonce != validatedToken.GetClaimValue( JwtClaimTypes.Nonce ) )
            {
                throw new Exception( "Id Token failed to validate." );
            }

            return token as JwtSecurityToken;
        }

        private OpenIdConnectConfiguration GetOpenIdConnectConfiguration()
        {
            var authServer = GetAttributeValue( AttributeKey.AuthenticationServer ).EnsureTrailingForwardslash();
            string stsDiscoveryEndpoint = $"{authServer}.well-known/openid-configuration";

            ConfigurationManager<OpenIdConnectConfiguration> configManager =
                 new ConfigurationManager<OpenIdConnectConfiguration>( stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever() );

            return configManager.GetConfigurationAsync().Result;
        }

        public static string HandleOidcUserAddUpdate( JwtSecurityToken idToken, string accessToken = "" )
        {
            // accessToken is required
            if ( accessToken.IsNullOrWhiteSpace() )
            {
                //return null;
            }

            string username = string.Empty;
            string oidcId = idToken.GetClaimValue( JwtClaimTypes.Subject );
            //string facebookLink = facebookUser.link;

            string userName = "OIDC_" + oidcId;
            UserLogin user = null;

            using ( var rockContext = new RockContext() )
            {

                // Query for an existing user
                var userLoginService = new UserLoginService( rockContext );
                user = userLoginService.GetByUserName( userName );

                // If no user was found, see if we can find a match in the person table
                if ( user == null )
                {
                    // Get name/email from Facebook login
                    string lastName = idToken.GetClaimValue( JwtClaimTypes.FamilyName ).ToStringSafe();
                    string firstName = idToken.GetClaimValue( JwtClaimTypes.GivenName ).ToStringSafe();
                    string email = string.Empty;
                    try
                    { email = idToken.GetClaimValue( JwtClaimTypes.Email ).ToStringSafe(); }
                    catch { }

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if ( !string.IsNullOrWhiteSpace( email ) )
                    {
                        var personService = new PersonService( rockContext );
                        person = personService.FindPerson( firstName, lastName, email, true );
                    }

                    var personRecordTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                    rockContext.WrapTransaction( () =>
                    {
                        if ( person == null )
                        {
                            person = new Person();
                            person.IsSystem = false;
                            person.RecordTypeValueId = personRecordTypeId;
                            person.RecordStatusValueId = personStatusPending;
                            person.FirstName = firstName;
                            person.LastName = lastName;
                            person.Email = email;
                            person.IsEmailActive = true;
                            person.EmailPreference = EmailPreference.EmailAllowed;
                            try
                            {
                                var gender = idToken.GetClaimValue( JwtClaimTypes.Gender );
                                if ( gender == "male" )
                                {
                                    person.Gender = Gender.Male;
                                }
                                else if ( gender == "female" )
                                {
                                    person.Gender = Gender.Female;
                                }
                                else
                                {
                                    person.Gender = Gender.Unknown;
                                }
                            }
                            catch { }

                            if ( person != null )
                            {
                                PersonService.SaveNewPerson( person, rockContext, null, false );
                            }
                        }

                        if ( person != null )
                        {
                            int typeId = EntityTypeCache.Get( typeof( OidcClient ) ).Id;
                            user = UserLoginService.Create( rockContext, person, AuthenticationServiceType.External, typeId, userName, "oidc", true );
                        }

                    } );
                }

                if ( user != null )
                {
                    username = user.UserName;

                    if ( user.PersonId.HasValue )
                    {
                        var converter = new ExpandoObjectConverter();

                        var personService = new PersonService( rockContext );
                        var person = personService.Get( user.PersonId.Value );
                        if ( person != null )
                        {
                            // TODO: Handle person photo.
                            // If person does not have a photo, try to get their Facebook photo
                            if ( !person.PhotoId.HasValue )
                            {
                                //var restClient = new RestClient( string.Format( "https://graph.facebook.com/v3.3/{0}/picture?redirect=false&type=square&height=400&width=400", oidcId ) );
                                //var restRequest = new RestRequest( Method.GET );
                                //restRequest.RequestFormat = DataFormat.Json;
                                //restRequest.AddHeader( "Accept", "application/json" );
                                //var restResponse = restClient.Execute( restRequest );
                                //if ( restResponse.StatusCode == HttpStatusCode.OK )
                                //{
                                //    dynamic picData = JsonConvert.DeserializeObject<ExpandoObject>( restResponse.Content, converter );
                                //    bool isSilhouette = picData.data.is_silhouette;
                                //    string url = picData.data.url;

                                //    // If Facebook returned a photo url
                                //    if ( !isSilhouette && !string.IsNullOrWhiteSpace( url ) )
                                //    {
                                //        // Download the photo from the URL provided
                                //        restClient = new RestClient( url );
                                //        restRequest = new RestRequest( Method.GET );
                                //        restResponse = restClient.Execute( restRequest );
                                //        if ( restResponse.StatusCode == HttpStatusCode.OK )
                                //        {
                                //            var bytes = restResponse.RawBytes;

                                //            // Create and save the image
                                //            BinaryFileType fileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
                                //            if ( fileType != null )
                                //            {
                                //                var binaryFileService = new BinaryFileService( rockContext );
                                //                var binaryFile = new BinaryFile();
                                //                binaryFileService.Add( binaryFile );
                                //                binaryFile.IsTemporary = false;
                                //                binaryFile.BinaryFileType = fileType;
                                //                binaryFile.MimeType = "image/jpeg";
                                //                binaryFile.FileName = user.Person.NickName + user.Person.LastName + ".jpg";
                                //                binaryFile.FileSize = bytes.Length;
                                //                binaryFile.ContentStream = new MemoryStream( bytes );

                                //                rockContext.SaveChanges();

                                //                person.PhotoId = binaryFile.Id;
                                //                rockContext.SaveChanges();
                                //            }
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }

                return username;
            }
        }

        private string EncodeBcrypt( string input )
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt( 12 );
            return BCrypt.Net.BCrypt.HashPassword( input, salt );
        }
    }
}
