// <auto-generated />
namespace Rock.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.3-40302")]
    public sealed partial class CollectHostingMetrics : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(AddGroupSyncScheduleIntervalMinutes));
        
        string IMigrationMetadata.Id
        {
            get { return "202004240006402_CollectHostingMetrics"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
