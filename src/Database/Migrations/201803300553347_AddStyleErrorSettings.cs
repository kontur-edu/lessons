namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStyleErrorSettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StyleErrorSettings",
                c => new
                    {
                        ErrorType = c.Int(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ErrorType);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StyleErrorSettings");
        }
    }
}
