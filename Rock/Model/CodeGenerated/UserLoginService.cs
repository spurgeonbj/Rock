//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// UserLogin Service class
    /// </summary>
    public partial class UserLoginService : Service<UserLogin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginService"/> class
        /// </summary>
        public UserLoginService()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginService"/> class
        /// </summary>
        /// <param name="repository">The repository.</param>
        public UserLoginService(IRepository<UserLogin> repository) : base(repository)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public UserLoginService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( UserLogin item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class UserLoginExtensionMethods
    {
        /// <summary>
        /// Clones this UserLogin object to a new UserLogin object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static UserLogin Clone( this UserLogin source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as UserLogin;
            }
            else
            {
                var target = new UserLogin();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another UserLogin object to this UserLogin object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this UserLogin target, UserLogin source )
        {
            target.ServiceType = source.ServiceType;
            target.EntityTypeId = source.EntityTypeId;
            target.UserName = source.UserName;
            target.Password = source.Password;
            target.IsConfirmed = source.IsConfirmed;
            target.LastActivityDateTime = source.LastActivityDateTime;
            target.LastLoginDateTime = source.LastLoginDateTime;
            target.LastPasswordChangedDateTime = source.LastPasswordChangedDateTime;
            target.CreationDateTime = source.CreationDateTime;
            target.IsOnLine = source.IsOnLine;
            target.IsLockedOut = source.IsLockedOut;
            target.LastLockedOutDateTime = source.LastLockedOutDateTime;
            target.FailedPasswordAttemptCount = source.FailedPasswordAttemptCount;
            target.FailedPasswordAttemptWindowStartDateTime = source.FailedPasswordAttemptWindowStartDateTime;
            target.LastPasswordExpirationWarningDateTime = source.LastPasswordExpirationWarningDateTime;
            target.ApiKey = source.ApiKey;
            target.PersonId = source.PersonId;
            target.Id = source.Id;
            target.Guid = source.Guid;

        }
    }
}
