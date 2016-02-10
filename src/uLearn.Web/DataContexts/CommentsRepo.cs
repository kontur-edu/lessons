using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
    public class CommentsRepo
    {
        private readonly ULearnDb db;

        public CommentsRepo() : this(new ULearnDb())
		{

        }

        public CommentsRepo(ULearnDb db)
        {
            this.db = db;
        }

        public async Task AddComment(ApplicationUser author, string courseId, string slideId, int parentCommentId)
        {
            db.Comments.Add(new Comment
            {
                Author = author,
                CourseId = courseId,
                SlideId = slideId,
                ParentCommentId = parentCommentId,
                PublishTime = DateTime.Now,
            });
            await db.SaveChangesAsync();
        }

        public IEnumerable<Comment> GetSlideComments(string courseId, string slideId)
        {
	        return TestCommentsData(courseId, slideId);
            return db.Comments.Where(x => x.SlideId == slideId && ! x.IsDeleted);
        }

	    private IEnumerable<Comment> TestCommentsData(string courseId, string slideId)
	    {
		    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
		    return new List<Comment>
			{
				new Comment
				{
					Id = 1,
					CourseId = courseId,
					SlideId = slideId,
					ParentCommentId = -1,
					Text = "The first comment",
					PublishTime = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
					Author = userManager.FindByName("user"),
				},
				new Comment
				{
					Id = 2,
					CourseId = courseId,
					SlideId = slideId,
					ParentCommentId = 1,
					Text = "Child comment 1",
					PublishTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(30)),
					Author = userManager.FindByName("user"),
				},
				new Comment
				{
					Id = 3,
					CourseId = courseId,
					SlideId = slideId,
					ParentCommentId = -1,
					Text = "The second comment",
					PublishTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(40)),
					Author = userManager.FindByName("admin"),
				},
				new Comment
				{
					Id = 4,
					CourseId = courseId,
					SlideId = slideId,
					ParentCommentId = 1,
					Text = "Child comment 2",
					PublishTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(20)),
					Author = userManager.FindByName("admin"),
				}
			};
	    }
    }
}