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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Add run-once job to update PublicApplicationRoot attributes.
    /// </summary>
    public partial class AddUpdatePublicApplicationRootAttributeJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddJobToUpdatePublicApplicationRootAttribute();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveJobToUpdateUpdatePublicApplicationRootAttribute();
        }

        private void AddJobToUpdatePublicApplicationRootAttribute()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV120DataMigrationsUpdatePublicApplicationRootAttribute'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_120_UPDATE_APPLICATION_ROOT_ATTRIBUTES}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[Guid]
                ) VALUES (
                    1
                    ,1
                    ,'Rock Update Helper v12.0 - Public Application Root Update'
                    ,'This job will update attribute values to fix the PublicApplicationRoot paths.'
                    ,'Rock.Jobs.PostV120DataMigrationsUpdatePublicApplicationRootAttribute'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_120_UPDATE_APPLICATION_ROOT_ATTRIBUTES}'
                );
            END" );
        }

        private void RemoveJobToUpdateUpdatePublicApplicationRootAttribute()
        {
            Sql( $@"
                DELETE [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV120DataMigrationsUpdatePublicApplicationRootAttribute'
                                AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_120_UPDATE_APPLICATION_ROOT_ATTRIBUTES}'
                " );
        }
    }
}
