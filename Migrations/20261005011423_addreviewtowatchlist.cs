using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IS7012Final.Migrations
{
    /// <inheritdoc />
    public partial class addreviewtowatchlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewId",
                table: "Watchlist",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Watchlist_ReviewId",
                table: "Watchlist",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Watchlist_Review_ReviewId",
                table: "Watchlist",
                column: "ReviewId",
                principalTable: "Review",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Watchlist_Review_ReviewId",
                table: "Watchlist");

            migrationBuilder.DropIndex(
                name: "IX_Watchlist_ReviewId",
                table: "Watchlist");

            migrationBuilder.DropColumn(
                name: "ReviewId",
                table: "Watchlist");
        }
    }
}
