namespace uLearn.Web.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class extractlargetextfields : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.TextBlobs",
				c => new
					{
						Hash = c.String(nullable: false, maxLength: 40),
						Text = c.String(nullable: false),
					})
				.PrimaryKey(t => t.Hash);

			AddColumn("dbo.UserSolutions", "SolutionCodeHash", c => c.String(nullable: false, maxLength: 40));
			AddColumn("dbo.UserSolutions", "CompilationErrorHash", c => c.String(maxLength: 40));
			AddColumn("dbo.UserSolutions", "OutputHash", c => c.String(maxLength: 40));

			const string updateFormat = "update dbo.UserSolutions set {0}Hash = convert(varchar(40), hashbytes('SHA1', Substring({0}, 0, 4000)), 2) where {0} is not null";

			const string mergeFormat = "merge dbo.TextBlobs as texts "
									   + "using (select distinct {0}Hash, Substring({0}, 0, 4000) as {0} from dbo.UserSolutions where {0} is not null) as solutions "
									   + "on (texts.Hash = solutions.{0}Hash) "
									   + "when not matched then insert values (solutions.{0}Hash, solutions.{0});";

			Sql("update dbo.UserSolutions set SolutionCodeHash = convert(varchar(40), hashbytes('SHA1', Substring(Code, 0, 4000)), 2)");
			Sql(String.Format(updateFormat, "CompilationError"));
			Sql(String.Format(updateFormat, "Output"));

			Sql("insert into dbo.TextBlobs select distinct SolutionCodeHash, Substring(Code, 0, 4000) from dbo.UserSolutions");
			Sql(String.Format(mergeFormat, "CompilationError"));
			Sql(String.Format(mergeFormat, "Output"));

			CreateIndex("dbo.UserSolutions", "SolutionCodeHash");
			CreateIndex("dbo.UserSolutions", "CompilationErrorHash");
			CreateIndex("dbo.UserSolutions", "OutputHash");
			AddForeignKey("dbo.UserSolutions", "CompilationErrorHash", "dbo.TextBlobs", "Hash");
			AddForeignKey("dbo.UserSolutions", "OutputHash", "dbo.TextBlobs", "Hash");
			AddForeignKey("dbo.UserSolutions", "SolutionCodeHash", "dbo.TextBlobs", "Hash", cascadeDelete: true);
			DropColumn("dbo.UserSolutions", "Code");
			DropColumn("dbo.UserSolutions", "CompilationError");
			DropColumn("dbo.UserSolutions", "Output");
		}

		public override void Down()
		{
			AddColumn("dbo.UserSolutions", "Output", c => c.String());
			AddColumn("dbo.UserSolutions", "CompilationError", c => c.String());
			AddColumn("dbo.UserSolutions", "Code", c => c.String(nullable: false));
			DropForeignKey("dbo.UserSolutions", "SolutionCodeHash", "dbo.TextBlobs");
			DropForeignKey("dbo.UserSolutions", "OutputHash", "dbo.TextBlobs");
			DropForeignKey("dbo.UserSolutions", "CompilationErrorHash", "dbo.TextBlobs");
			DropIndex("dbo.UserSolutions", new[] { "OutputHash" });
			DropIndex("dbo.UserSolutions", new[] { "CompilationErrorHash" });
			DropIndex("dbo.UserSolutions", new[] { "SolutionCodeHash" });

			Sql("update dbo.UserSolutions set Code = text.Text from dbo.UserSolutions inner join dbo.TextBlobs as text on (SolutionCodeHash = text.Hash)");
			Sql("update dbo.UserSolutions set CompilationError = text.Text from dbo.UserSolutions inner join dbo.TextBlobs as text on (CompilationErrorHash = text.Hash)");
			Sql("update dbo.UserSolutions set Output = text.Text from dbo.UserSolutions inner join dbo.TextBlobs as text on (OutputHash = text.Hash)");

			DropColumn("dbo.UserSolutions", "OutputHash");
			DropColumn("dbo.UserSolutions", "CompilationErrorHash");
			DropColumn("dbo.UserSolutions", "SolutionCodeHash");
			DropTable("dbo.TextBlobs");
		}
	}
}
