using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyContinuity_API.Migrations
{
    /// <inheritdoc />
    public partial class CompleteUserSpace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JoinedOn",
                table: "UserSpaces",
                newName: "AddedOn");

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSpaces"" 
                ALTER COLUMN ""Role"" TYPE integer 
                USING CASE ""Role"" 
                    WHEN 'Owner' THEN 0 
                    WHEN 'Admin' THEN 1 
                    WHEN 'Editor' THEN 2 
                    WHEN 'Viewer' THEN 3 
                    ELSE 0 
                END;");

            migrationBuilder.AddColumn<int>(
                name: "AddedBy",
                table: "UserSpaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserSpaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitationExpiresOn",
                table: "UserSpaces",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationStatus",
                table: "UserSpaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InvitationToken",
                table: "UserSpaces",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAccessedOn",
                table: "UserSpaces",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastUpdatedBy",
                table: "UserSpaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedOn",
                table: "UserSpaces",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "InvitationExpiresOn",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "InvitationStatus",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "InvitationToken",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "LastAccessedOn",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "UserSpaces");

            migrationBuilder.DropColumn(
                name: "LastUpdatedOn",
                table: "UserSpaces");

            migrationBuilder.RenameColumn(
                name: "AddedOn",
                table: "UserSpaces",
                newName: "JoinedOn");

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSpaces"" 
                ALTER COLUMN ""Role"" TYPE text 
                USING CASE ""Role""::integer
                    WHEN 0 THEN 'Owner'
                    WHEN 1 THEN 'Admin'
                    WHEN 2 THEN 'Editor'
                    WHEN 3 THEN 'Viewer'
                    ELSE 'Viewer'
                END;");
        }
    }
}
