using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JobMailer.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserOwnedTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Templates used to be global, so every existing row belongs to nobody. Add the owner
            // column as nullable first so we can give every current user a copy of the old templates.
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Templates",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO [Templates] ([UserId], [Key], [Subject], [Body])
                SELECT u.[UserId], t.[Key], t.[Subject], t.[Body]
                FROM [Users] AS u
                CROSS JOIN [Templates] AS t
                WHERE t.[UserId] IS NULL;
                """);

            migrationBuilder.Sql("DELETE FROM [Templates] WHERE [UserId] IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Templates",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_UserId",
                table: "Templates",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Users_UserId",
                table: "Templates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverting restores the old global seed, so remove user-owned templates first.
            migrationBuilder.Sql("DELETE FROM [Templates];");

            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Users_UserId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_UserId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Templates");

            migrationBuilder.InsertData(
                table: "Templates",
                columns: new[] { "Id", "Body", "Key", "Subject" },
                values: new object[,]
                {
                    { 1, "Dear Hiring Team,\n\nI am excited to apply for the {{JobTitle}} position at {{CompanyName}}. I specialize in full-stack web applications with React and .NET and would welcome the chance to contribute to your team.\n\nMy CV is attached. You can reach me at {{Phone}} or {{Email}}.\n\nBest regards,\n{{FullName}}", "Full Stack", "Application for {{JobTitle}} - {{FullName}}" },
                    { 2, "Dear Hiring Team,\n\nI am excited to apply for the {{JobTitle}} position at {{CompanyName}}. I specialize in GIS solutions, spatial data and web mapping and would welcome the chance to contribute to your team.\n\nMy CV is attached. You can reach me at {{Phone}} or {{Email}}.\n\nBest regards,\n{{FullName}}", "GIS", "Application for {{JobTitle}} - {{FullName}}" }
                });
        }
    }
}
