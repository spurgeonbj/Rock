using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V12.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v12.0 - Public Application Root Update" )]
    [Description( "This job will update attribute values to fix the PublicApplicationRoot paths." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Attribute Values, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV120DataMigrationsUpdatePublicApplicationRootAttribute : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            UpdateAttributeValues( commandTimeout );

            ServiceJobService.DeleteJob( context.GetJobId() );
        }

        private void UpdateAttributeValues(int commandTimeout)
        {
            var currentValue = @"{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}/assessments?{{ Person.ImpersonationParameter }}";

            var newValue = @"{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}assessments?{{ Person.ImpersonationParameter }}";

            UpdateTableColumn( "AttributeValue", "Value", currentValue, newValue, commandTimeout );
        }

        private void UpdateTableColumn( string tableName, string columnName, string currentValue, string newValue, int commandTimeout )
        {
            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );

            var normalizedColumn = migrationHelper.NormalizeColumnCRLF( columnName );

            jobMigration.Sql( $@"UPDATE [{tableName}]
                    SET [{columnName}] = REPLACE({normalizedColumn}, '{currentValue}', '{newValue}')
                    WHERE {normalizedColumn} LIKE '%{currentValue}%'" );
        }
    }
}
