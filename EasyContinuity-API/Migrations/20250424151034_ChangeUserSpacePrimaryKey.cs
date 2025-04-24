using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EasyContinuity_API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserSpacePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSpaces",
                table: "UserSpaces");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserSpaces",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSpaces",
                table: "UserSpaces",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserSpaces_UserId_SpaceId",
                table: "UserSpaces",
                columns: new[] { "UserId", "SpaceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSpaces",
                table: "UserSpaces");

            migrationBuilder.DropIndex(
                name: "IX_UserSpaces_UserId_SpaceId",
                table: "UserSpaces");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserSpaces",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSpaces",
                table: "UserSpaces",
                columns: new[] { "UserId", "SpaceId" });
        }
    }
}
