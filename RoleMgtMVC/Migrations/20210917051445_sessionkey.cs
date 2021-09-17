using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleMgtMVC.Migrations
{
    public partial class sessionkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionKey",
                table: "User",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "PasswordHash", "Salt" },
                values: new object[] { new DateTime(2021, 9, 17, 10, 44, 45, 113, DateTimeKind.Local).AddTicks(5372), "sBMA1VyuQgk6C6fPe2qpH27Qap/0AB+OYztQ6U3igcc=", "�pC7�'�)6������" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionKey",
                table: "User");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "PasswordHash", "Salt" },
                values: new object[] { new DateTime(2021, 9, 17, 3, 7, 23, 316, DateTimeKind.Local).AddTicks(7990), "1x473YQv9BslZaetlke7ASwZgkoyZy3Jv4MlijzJOS0=", "�TRX5����B~��" });
        }
    }
}
