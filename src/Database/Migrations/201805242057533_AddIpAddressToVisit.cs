namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIpAddressToVisit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Visits", "IpAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Visits", "IpAddress");
        }
    }
}
