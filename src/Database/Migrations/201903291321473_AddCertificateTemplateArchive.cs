namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCertificateTemplateArchive : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CertificateTemplateArchives",
                c => new
                    {
                        ArchiveName = c.String(nullable: false, maxLength: 128),
                        CertificateTemplateId = c.Guid(nullable: false),
                        Content = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.ArchiveName)
                .ForeignKey("dbo.CertificateTemplates", t => t.CertificateTemplateId, cascadeDelete: true)
                .Index(t => t.CertificateTemplateId, name: "IDX_CertificateTemplateArchives_CertificateTemplateId");
            
            AlterColumn("dbo.CertificateTemplates", "ArchiveName", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CertificateTemplateArchives", "CertificateTemplateId", "dbo.CertificateTemplates");
            DropIndex("dbo.CertificateTemplateArchives", "IDX_CertificateTemplateArchives_CertificateTemplateId");
            AlterColumn("dbo.CertificateTemplates", "ArchiveName", c => c.String(nullable: false));
            DropTable("dbo.CertificateTemplateArchives");
        }
    }
}
