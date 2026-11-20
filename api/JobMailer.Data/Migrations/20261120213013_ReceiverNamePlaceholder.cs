using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMailer.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReceiverNamePlaceholder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Default templates used to greet the whole team. Use the optional receiver-name
            // placeholder instead so the greeting can address the hiring manager when known.
            migrationBuilder.Sql(
                """
                UPDATE [Templates]
                SET [Body] = REPLACE([Body], 'Dear Hiring Team,', 'Dear {{ReceiverName}},')
                WHERE [Body] LIKE '%Dear Hiring Team,%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Templates]
                SET [Body] = REPLACE([Body], 'Dear {{ReceiverName}},', 'Dear Hiring Team,')
                WHERE [Body] LIKE '%Dear {{ReceiverName}},%';
                """);
        }
    }
}
