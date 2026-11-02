using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rafiq.Migrations
{
    /// <inheritdoc />
    public partial class deleteMultiLanguageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BrandEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DosageAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DosageEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ManufacturerAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "AnswerAr",
                table: "ImportantQuestions");

            migrationBuilder.DropColumn(
                name: "AnswerEn",
                table: "ImportantQuestions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ActiveIngredients");

            migrationBuilder.RenameColumn(
                name: "WarningEn",
                table: "Products",
                newName: "Warning");

            migrationBuilder.RenameColumn(
                name: "WarningAr",
                table: "Products",
                newName: "ScientificName");

            migrationBuilder.RenameColumn(
                name: "ScientificNameEn",
                table: "Products",
                newName: "Manufacturer");

            migrationBuilder.RenameColumn(
                name: "ScientificNameAr",
                table: "Products",
                newName: "Dosage");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Products",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameAr",
                table: "Products",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ManufacturerEn",
                table: "Products",
                newName: "Brand");

            migrationBuilder.RenameColumn(
                name: "QuestionEn",
                table: "ImportantQuestions",
                newName: "Question");

            migrationBuilder.RenameColumn(
                name: "QuestionAr",
                table: "ImportantQuestions",
                newName: "Answer");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "ActiveIngredients",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Warning",
                table: "Products",
                newName: "WarningEn");

            migrationBuilder.RenameColumn(
                name: "ScientificName",
                table: "Products",
                newName: "WarningAr");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Manufacturer",
                table: "Products",
                newName: "ScientificNameEn");

            migrationBuilder.RenameColumn(
                name: "Dosage",
                table: "Products",
                newName: "ScientificNameAr");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "NameAr");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "Products",
                newName: "ManufacturerEn");

            migrationBuilder.RenameColumn(
                name: "Question",
                table: "ImportantQuestions",
                newName: "QuestionEn");

            migrationBuilder.RenameColumn(
                name: "Answer",
                table: "ImportantQuestions",
                newName: "QuestionAr");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ActiveIngredients",
                newName: "NameEn");

            migrationBuilder.AddColumn<string>(
                name: "BrandAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DosageAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerAr",
                table: "ImportantQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AnswerEn",
                table: "ImportantQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ActiveIngredients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
