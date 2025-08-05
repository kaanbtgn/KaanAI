using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KaanAI.Persistence.Context.Main.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenUsageToAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "Answers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "Answers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "Answers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "Answers");
        }
    }
}
