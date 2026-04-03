using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecondHandSharing.Migrations
{
    /// <inheritdoc />
    public partial class AddViewHistoryTable_FinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.CreateTable(
    name: "ViewHistories",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        UserId = table.Column<int>(nullable: false),
        ItemId = table.Column<int>(nullable: false),
        ViewedAt = table.Column<DateTime>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_ViewHistories", x => x.Id);
        table.ForeignKey(
            name: "FK_ViewHistories_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "UserId",
            onDelete: ReferentialAction.NoAction);
        table.ForeignKey(
            name: "FK_ViewHistories_Items_ItemId",
            column: x => x.ItemId,
            principalTable: "Items",
            principalColumn: "ItemId",
            onDelete: ReferentialAction.NoAction);
    });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
