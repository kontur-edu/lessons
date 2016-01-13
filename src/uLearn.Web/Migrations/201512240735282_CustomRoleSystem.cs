using System;

namespace uLearn.Web.Migrations
{
	using System.Data.Entity.Migrations;

	public partial class CustomRoleSystem : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.UserRoles",
				c => new
				{
					Id = c.Int(nullable: false, identity: true),
					UserId = c.String(maxLength: 128),
					CourseId = c.String(nullable: false),
					Role = c.Int(nullable: false),
				})
				.PrimaryKey(t => t.Id)
				.ForeignKey("dbo.AspNetUsers", t => t.UserId)
				.Index(t => t.UserId);

			Sql(@"
					declare @adminId nvarchar(max);
					set @adminId = (select id from AspNetRoles where Name = 'admin');
					declare @instructorId nvarchar(max);
					set @instructorId = (select id from AspNetRoles where Name = 'instructor');
					insert 
						into AspNetUserRoles(UserId, RoleId)
						(
							select distinct UserId, @adminId 
								from AspNetUserRoles as t 
								where not exists (
									select 1 
									from AspNetUserRoles as d 
									where t.UserId = d.UserId and d.RoleId = @adminId
								) and t.RoleId = @instructorId
						);"
				);
			Sql(@"delete from AspNetRoles where Name <> N'admin'");
			Sql("update AspNetRoles set Name = N'SysAdmin' where Name = N'admin'");
		}

		public override void Down()
		{
			DropForeignKey("dbo.UserRoles", "UserId", "dbo.AspNetUsers");
			DropIndex("dbo.UserRoles", new[] { "UserId" });
			DropTable("dbo.UserRoles");
			Sql("update AspNetRoles set Name = N'admin' where Name = N'SysAdmin'");
			Sql(string.Format(@"insert into AspNetRoles values (N'{0}', N'instructor')", Guid.NewGuid()));
			Sql(string.Format(@"insert into AspNetRoles values (N'{0}', N'tester')", Guid.NewGuid()));
		}
	}
}
