using Microsoft.EntityFrameworkCore.Migrations;

namespace wi_crawler.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermIndexes",
                columns: table => new
                {
                    Term = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermIndexes", x => x.Term);
                });

            migrationBuilder.CreateTable(
                name: "Webpages",
                columns: table => new
                {
                    WebpageId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HtmlContent = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    BaseDomain = table.Column<string>(nullable: true),
                    TermIndexTerm = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webpages", x => x.WebpageId);
                    table.ForeignKey(
                        name: "FK_Webpages_TermIndexes_TermIndexTerm",
                        column: x => x.TermIndexTerm,
                        principalTable: "TermIndexes",
                        principalColumn: "Term",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Webpages_TermIndexTerm",
                table: "Webpages",
                column: "TermIndexTerm");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Webpages");

            migrationBuilder.DropTable(
                name: "TermIndexes");
        }
    }
}
