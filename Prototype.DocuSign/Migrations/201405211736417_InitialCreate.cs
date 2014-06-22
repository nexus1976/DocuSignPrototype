namespace Prototype.DocuSign.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        DocumentId = c.Int(nullable: false, identity: true),
                        DocumentName = c.String(),
                        RequestedBy = c.String(),
                        RequestedDate = c.DateTime(nullable: false),
                        DocuSignEnvelopId = c.String(),
                        Status = c.String(),
                        DocumentPDF = c.Binary(),
                    })
                .PrimaryKey(t => t.DocumentId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Documents");
        }
    }
}
