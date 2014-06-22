namespace Prototype.DocuSign.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSenderInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documents", "SentToName", c => c.String());
            AddColumn("dbo.Documents", "SentToEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documents", "SentToEmail");
            DropColumn("dbo.Documents", "SentToName");
        }
    }
}
