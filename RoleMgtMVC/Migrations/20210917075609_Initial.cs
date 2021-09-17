using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoleMgtMVC.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Salt = table.Column<byte[]>(nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ExpireDate = table.Column<DateTime>(nullable: false),
                    UserRoleId = table.Column<int>(nullable: false),
                    SessionKey = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "Id", "Level", "Name" },
                values: new object[,]
                {
                    { 1, 1, "External" },
                    { 2, 2, "Internal" },
                    { 3, 3, "Lead" },
                    { 4, 4, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "CreatedDate", "Email", "ExpireDate", "FirstName", "IsActive", "LastName", "PasswordHash", "Salt", "SessionKey", "UserRoleId" },
                values: new object[] { 1, new DateTime(2021, 9, 17, 13, 26, 8, 951, DateTimeKind.Local).AddTicks(9352), "sachithsujeewa@gmail.com", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sachith", true, "Sujeewa", "T0znWEIGTQudqJKcFkOfOGxi9fNB3QG4A2B06GuzHeU=", new byte[] { 119, 46, 5, 112, 249, 169, 243, 117, 168, 167, 241, 125, 199, 138, 48, 65 }, new Guid("00000000-0000-0000-0000-000000000000"), 4 });

            migrationBuilder.CreateIndex(
                name: "IX_User_UserRoleId",
                table: "User",
                column: "UserRoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserRole");
        }
    }
}
