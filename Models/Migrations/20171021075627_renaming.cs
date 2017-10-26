using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Models.Migrations
{
    public partial class renaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdEvent",
                table: "UserEvents");

            migrationBuilder.DropColumn(
                name: "IdEvent",
                table: "UserEventNotifications");

            migrationBuilder.AddColumn<string>(
                name: "IdPlace",
                table: "UserEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdPlace",
                table: "UserEventNotifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdPlace",
                table: "UserEvents");

            migrationBuilder.DropColumn(
                name: "IdPlace",
                table: "UserEventNotifications");

            migrationBuilder.AddColumn<int>(
                name: "IdEvent",
                table: "UserEvents",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdEvent",
                table: "UserEventNotifications",
                nullable: false,
                defaultValue: 0);
        }
    }
}
