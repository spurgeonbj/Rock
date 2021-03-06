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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 99, "1.10.0" )]
    class MigrationRollupsFor10_2_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
            Commented these out as they are going to be included in the EF migration.
             */
            //ShowCameraSupportAttributeOnGrid();
            //MobileRollups();
            //IPadCameraCheckinUpdate();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// JE/DH - Set Supports Camera Attribute to Show on Grid
        /// </summary>
        private void ShowCameraSupportAttributeOnGrid()
        {
            Sql( $@"UPDATE [Attribute]
                SET[IsGridColumn] = 1
                WHERE[Guid] = '{Rock.SystemGuid.Attribute.DEFINED_VALUE_DEVICE_TYPE_SUPPORTS_CAMERAS}'" );
        }

        /// <summary>
        /// JE/DH - Mobile Roll-ups
        /// </summary>
        private void MobileRollups()
        {
             RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Group Member List",
                "",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Group Member View",
                "",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Group View",
                "",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW );

            // Fix description of attribute.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "iPad Camera Barcode Configuration", "CameraBarcodeConfiguration", "iPad Camera Barcode Configuration", @"Specifies if a camera on the device should be used for barcode scanning.", 7, @"Available", "B7BD8FDE-479E-4450-8DE7-F53A6C37F19F" );

            // Enable Show On Grid for Block Template attribute.
            Sql( "UPDATE [Attribute] SET [IsGridColumn] = 1 WHERE [Guid] = '0AAFF537-7EC6-4AA9-A987-68DA1FF8511D'" );
        }

        /// <summary>
        /// JE/DH - iPad Camera Check-in Update
        /// </summary>
        private void IPadCameraCheckinUpdate()
        {
             string barcodeScanButtonTemplate = @"
<div class='checkin-search-actions'>
    {% if BarcodeScanEnabled == true %}
    <a class='btn btn-default btn-barcode js-camera-button'>
         <span>{{ BarcodeScanButtonText }}</span>
    </a>
    {% endif %}
</div>
";

            Sql( $@"UPDATE [Attribute]
                SET [DefaultValue] = [DefaultValue] + '{barcodeScanButtonTemplate.Replace( "'", "''" )}'
                WHERE [Guid] = '5F242D2A-FD01-4508-9F4C-ED01124309E7'" );

            Sql( $@"UPDATE AV
                SET AV.[Value] = AV.[Value] + '{barcodeScanButtonTemplate.Replace( "'", "''" )}'
                FROM [AttributeValue] AV
                INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
                WHERE A.[Guid] = '5F242D2A-FD01-4508-9F4C-ED01124309E7'" );
        }

    }
}
