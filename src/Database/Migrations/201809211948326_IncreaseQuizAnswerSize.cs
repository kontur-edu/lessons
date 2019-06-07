namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreaseQuizAnswerSize : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserQuizs", "Text", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserQuizs", "Text", c => c.String(maxLength: 1024));
        }
    }
}
