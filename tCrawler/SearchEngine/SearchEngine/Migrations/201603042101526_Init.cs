namespace SearchEngine.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.IndexedPages",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(),
                        Content = c.String(),
                        Site = c.String(),
                        Url = c.String(),
                        ReplyCount = c.Int(nullable: false),
                        ViewCount = c.Int(nullable: false),
                        Author = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.IndexedPages");
        }
    }
}
