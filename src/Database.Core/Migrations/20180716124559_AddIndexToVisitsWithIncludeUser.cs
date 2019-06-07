using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Migrations
{
    public partial class AddIndexToVisitsWithIncludeUser : Migration
    {
		private string indexName = "IDX_Visits_BySlideAndTime_IncludeUser";
		
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql($"CREATE NONCLUSTERED INDEX [{indexName}] ON [dbo].[Visits] ([SlideId], [Timestamp]) INCLUDE ([UserId])");
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DROP INDEX [{indexName}]");
        }
    }
}
