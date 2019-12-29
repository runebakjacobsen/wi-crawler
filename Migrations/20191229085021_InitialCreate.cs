using Microsoft.EntityFrameworkCore.Migrations;

namespace wi_crawler.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Webpages",
                columns: table => new
                {
                    WebpageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HtmlContent = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    BaseDomain = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webpages", x => x.WebpageId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Webpages");
        }
    }
}
