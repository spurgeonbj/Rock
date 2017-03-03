﻿// <copyright>
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "AttributeMatrixTemplate" )]
    [DataContract]
    public partial class AttributeMatrixTemplate : Model<AttributeMatrixTemplate>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive
        {
            get
            {
                return _isActive;
            }

            set
            {
                _isActive = value;
            }
        }

        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the minimum rows.
        /// </summary>
        /// <value>
        /// The minimum rows.
        /// </value>
        [DataMember]
        public int? MinimumRows { get; set; }

        /// <summary>
        /// Gets or sets the maximum rows.
        /// </summary>
        /// <value>
        /// The maximum rows.
        /// </value>
        [DataMember]
        public int? MaximumRows { get; set; }

        /// <summary>
        /// Gets or sets the lava template for what is shown for 
        /// </summary>
        /// <value>
        /// The formatted lava.
        /// </value>
        [DataMember]
        public string FormattedLava { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AttributeMatrixTemplateConfiguration : EntityTypeConfiguration<AttributeMatrixTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatrixTemplateConfiguration"/> class.
        /// </summary>
        public AttributeMatrixTemplateConfiguration()
        {
            // intentionally blank
        }
    }

    #endregion
}
