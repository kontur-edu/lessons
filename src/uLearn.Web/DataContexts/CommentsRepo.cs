using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.ServiceModel.Syndication;
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
					Text = "<h1>The first comment</h1>",
					PublishTime = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
					Author = userManager.FindByName("user"),
				},
				new Comment
				{
					Id = 2,
					CourseId = courseId,
					SlideId = slideId,
					ParentCommentId = 1,
					Text = "Child comment 1\r\nLorem ipsum dolor sit amet, ei eam hinc menandri reprehendunt. In audiam repudiare mel, per id summo accusam. Purto admodum partiendo pro ei, vel ne possit graecis menandri, no omnium mentitum eam. Mel amet atqui et, at vix qualisque necessitatibus. Ignota salutatus no vel. Nemore semper bonorum at eam, vel ex laudem adipisci constituto, eu eos dicunt deleniti.",
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

		public async Task LikeComment(Comment comment, ApplicationUser user)
		{
			db.CommentLikes.Add(new CommentLike
			{
				User = user,
				Comment = comment,
				Timestamp = DateTime.Now,
			});
			await db.SaveChangesAsync();
		}

		public IEnumerable<CommentLike> GetCommentLikes(Comment comment)
		{
			return db.CommentLikes.Where(x => x.CommentId == comment.Id);
		}

		public IEnumerable<ApplicationUser> GetCommentLikers(Comment comment)
		{
			return GetCommentLikes(comment).Select(x => x.User);
		}

		public int GetCommentLikesCount(Comment comment)
		{
			return db.CommentLikes.Count(x => x.CommentId == comment.Id);
		}

		/// <returns>{commentId => likesCount}</returns>
		public Dictionary<int, int> GetCommentsLikesCounts(IEnumerable<Comment> comments)
		{
			return comments.ToDictionary(x => x.Id, x => new Random().Next(0, 10));

			var commentsIds = comments.Select(x => x.Id).ToImmutableHashSet();
			return db.CommentLikes.Where(x => commentsIds.Contains(x.CommentId))
				.GroupBy(x => x.CommentId)
				.ToDictionary(x => x.Key, x => x.Count());
		}

	    public IEnumerable<int> GetSlideCommentsLikedByUser(string courseId, string slideId, ApplicationUser user)
	    {
		    return db.CommentLikes.Where(x => x.User == user && x.Comment.SlideId == slideId).Select(x => x.CommentId);
	    }
	}
}