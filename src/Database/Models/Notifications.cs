using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Database.DataContexts;
using uLearn;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.Models
{
	public abstract class NotificationTransport
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[StringLength(64)]
		[Index("IDX_NotificationTransport_ByUser")]
		[Index("IDX_NotificationTransport_ByUserAndDeleted", 1)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		public bool IsEnabled { get; set; }

		[Index("IDX_NotificationTransport_ByUserAndDeleted", 2)]
		public bool IsDeleted { get; set; }
	}

	public class MailNotificationTransport : NotificationTransport
	{
		public override string ToString()
		{
			return $"Email <{User?.Email ?? User?.Id}> (#{Id})";
		}
	}

	public class TelegramNotificationTransport : NotificationTransport
	{
		public override string ToString()
		{
			return $"Telegram <{User?.TelegramChatId?.ToString() ?? User?.Id}> (#{Id})";
		}
	}

	public class FeedNotificationTransport : NotificationTransport
	{
		public override string ToString()
		{
			return $"Feed <{User?.Id}> (#{Id})";
		}
	}

	public class NotificationTransportSettings
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Index("IDX_NotificationTransportSettings_ByNotificationTransport")]
		public int NotificationTransportId { get; set; }

		public virtual NotificationTransport NotificationTransport { get; set; }

		[StringLength(100)]
		[Index("IDX_NotificationTransportSettings_ByCourse")]
		[Index("IDX_NotificationTransportSettings_ByCourseAndNofiticationType", 1)]
		public string CourseId { get; set; }

		[Index("IDX_NotificationTransportSettings_ByNotificationType")]
		[Index("IDX_NotificationTransportSettings_ByCourseAndNofiticationType", 2)]
		public NotificationType NotificationType { get; set; }

		public bool IsEnabled { get; set; }
	}

	public class NotificationDelivery
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Index("IDX_NotificationDelivery_ByNotificationAndTransport", 1)]
		public int NotificationId { get; set; }

		public virtual Notification Notification { get; set; }

		[Index("IDX_NotificationDelivery_ByNotificationAndTransport", 2)]
		public int NotificationTransportId { get; set; }

		public virtual NotificationTransport NotificationTransport { get; set; }

		public NotificationDeliveryStatus Status { get; set; }

		[Index("IDX_NotificatoinDelivery_ByCreateTime")]
		public DateTime CreateTime { get; set; }

		[Index("IDX_NotificationDelivery_ByNextTryTime")]
		public DateTime? NextTryTime { get; set; }

		public int FailsCount { get; set; }
	}

	public enum NotificationDeliveryStatus : byte
	{
		NotSent = 1,
		Sent = 2,
		Read = 3,
		WontSend = 4
	}

	public class MinCourseRoleAttribute : Attribute
	{
		public readonly CourseRole MinCourseRole;

		public MinCourseRoleAttribute(CourseRole minCourseRole)
		{
			MinCourseRole = minCourseRole;
		}
	}

	public class NotificationTypeAttribute : Attribute
	{
		public NotificationType Type { get; }

		public NotificationTypeAttribute(NotificationType type)
		{
			Type = type;
		}
	}

	public class SysAdminsOnlyAttribute : Attribute
	{
	}

	public class IsEnabledByDefaultAttribute : Attribute
	{
		public bool IsEnabled { get; }

		public IsEnabledByDefaultAttribute(bool isEnabled)
		{
			IsEnabled = isEnabled;
		}
	}

	public enum NotificationType : short
	{
		// Everybody
		[Display(Name = @"Сообщение от платформы ulearn.me", GroupName = @"Сообщения от платформы ulearn.me")]
		[IsEnabledByDefault(true)]
		SystemMessage = 1,

		[Display(Name = @"Сообщение от преподавателя", GroupName = @"Сообщения от преподавателя")]
		[IsEnabledByDefault(true)]
		InstructorMessage = 2,

		[Display(Name = @"Новый комментарий", GroupName = @"Новые комментарии")]
		NewComment = 3,

		[Display(Name = @"Ответ на ваш комментарий", GroupName = @"Ответы на ваши комментарии")]
		[IsEnabledByDefault(true)]
		RepliedToYourComment = 4,

		[Display(Name = @"Отметка «нравится» вашему комментарию", GroupName = @"Отметки «нравится» вашим комментариям")]
		[IsEnabledByDefault(true)]
		LikedYourComment = 5,

		[Display(Name = @"Вы прошли код-ревью", GroupName = @"Вы прошли код-ревью")]
		[IsEnabledByDefault(true)]
		PassedManualExerciseChecking = 6,

		[Display(Name = @"Тест проверен преподавателем", GroupName = @"Тест проверен преподавателем")]
		[IsEnabledByDefault(true)]
		PassedManualQuizChecking = 7,

		[Display(Name = @"Вы получили сертификат", GroupName = @"Вы получили сертификаты")]
		[IsEnabledByDefault(true)]
		ReceivedCertificate = 8,

		[Display(Name = @"Преподаватель выставил дополнительные баллы", GroupName = @"Дополнительные баллы, выставленные преподавателем")]
		[IsEnabledByDefault(true)]
		ReceivedAdditionalScore = 9,
		
		[Display(Name = @"Ответ на комментарий в код-ревью", GroupName = "Ответы на комментарии в код-ревью")]
		[IsEnabledByDefault(true)]
		ReceivedCommentToCodeReview = 10,

		// Instructors
		[Display(Name = @"Кто-то присоединился к вашей группе", GroupName = @"Кто-то присоединился к вашей группе")]
		[MinCourseRole(CourseRole.Instructor)]
		[IsEnabledByDefault(false)]
		JoinedToYourGroup = 101,

		[Display(Name = @"Вас назначили преподавателем группы", GroupName = @"Вас назначили преподавателем групп")]
		[MinCourseRole(CourseRole.Instructor)]
		[IsEnabledByDefault(true)]
		GrantedAccessToGroup = 102,

		[Display(Name = @"Вы перестали быть преподавателем группы", GroupName = @"Вы перестали быть преподавателем группы")]
		[MinCourseRole(CourseRole.Instructor)]
		[IsEnabledByDefault(true)]
		RevokedAccessToGroup = 103,

		[Display(Name = @"Преподаватель удалил студента из вашей группы", GroupName = @"Преподаватель удалил студентов из ваших групп")]
		[MinCourseRole(CourseRole.Instructor)]
		[IsEnabledByDefault(true)]
		GroupMemberHasBeenRemoved = 104,

		// Course admins
		[Display(Name = @"Добавлен новый преподаватель", GroupName = @"Добавлены новые преподаватели")]
		[MinCourseRole(CourseRole.CourseAdmin)]
		[IsEnabledByDefault(true)]
		AddedInstructor = 201,

		[Display(Name = @"Создана новая группа", GroupName = @"Созданы новые группы")]
		[MinCourseRole(CourseRole.CourseAdmin)]
		[IsEnabledByDefault(true)]
		CreatedGroup = 202,

		[Display(Name = @"Загружена новая версия курса", GroupName = @"Загружены новые версии курса")]
		[MinCourseRole(CourseRole.CourseAdmin)]
		[IsEnabledByDefault(true)]
		UploadedPackage = 203,

		[Display(Name = @"Опубликована новая версия курса", GroupName = @"Опубликованы новые версии курса")]
		[MinCourseRole(CourseRole.CourseAdmin)]
		[IsEnabledByDefault(true)]
		PublishedPackage = 204,

		[Display(Name = @"Курс скопирован на Степик", GroupName = @"Курсы скопированы на Степик")]
		[MinCourseRole(CourseRole.CourseAdmin)]
		[IsEnabledByDefault(true)]
		CourseExportedToStepik = 205,
	}

	public static class NotificationTypeExtensions
	{
		public static CourseRole GetMinCourseRole(this NotificationType type)
		{
			var attribute = type.GetAttribute<MinCourseRoleAttribute>();
			return attribute?.MinCourseRole ?? CourseRole.Student;
		}

		public static string GetDisplayName(this NotificationType type)
		{
			return type.GetAttribute<DisplayAttribute>().GetName();
		}

		public static string GetGroupName(this NotificationType type)
		{
			return type.GetAttribute<DisplayAttribute>().GetGroupName();
		}

		public static bool IsForSysAdminsOnly(this NotificationType type)
		{
			return type.GetAttribute<SysAdminsOnlyAttribute>() != null;
		}

		public static bool IsEnabledByDefault(this NotificationType type)
		{
			var attribute = type.GetAttribute<IsEnabledByDefaultAttribute>();
			return attribute?.IsEnabled ?? false;
		}
	}

	public class NotificationButton
	{
		public NotificationButton(string text, string link)
		{
			Text = text;
			Link = link;
		}

		public string Link { get; private set; }
		public string Text { get; private set; }
	}

	public abstract class Notification
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[StringLength(100)]
		[Required(AllowEmptyStrings = true)]
		[Index("IDX_Notification_ByCourse")]
		public string CourseId { get; set; }

		[StringLength(60)]
		[Required]
		public string InitiatedById { get; set; }

		public virtual ApplicationUser InitiatedBy { get; set; }

		[Required]
		[Index("IDX_Notification_ByCreateTime")]
		public DateTime CreateTime { get; set; }

		[Required]
		[Index("IDX_Notification_ByAreDeliveriesCreated")]
		public bool AreDeliveriesCreated { get; set; }

		public virtual ICollection<NotificationDelivery> Deliveries { get; set; }

		public abstract string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl);
		public abstract string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl);
		public abstract NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl);

		public abstract List<string> GetRecipientsIds(ULearnDb db);
		public virtual bool IsNotificationForEveryone => false;

		public abstract bool IsActual();

		/* Returns list of notifications, which blocks this notification from sending to specific user. I.e. NewComment is blocked by ReplyToYourComment */
		public virtual List<Notification> GetBlockerNotifications(ULearnDb db)
		{
			return new List<Notification>();
		}

		protected string GetSlideTitle(Course course, Slide slide)
		{
			return $"{course.Title.MakeNestedQuotes()}: {slide.Title.MakeNestedQuotes()}";
		}

		protected string GetUnitTitle(Course course, Unit unit)
		{
			return $"{course.Title.MakeNestedQuotes()}: {unit.Title.MakeNestedQuotes()}";
		}

		protected string GetSlideUrl(Course course, Slide slide, string baseUrl)
		{
			return baseUrl + $"/Course/{course.Id}/{slide.Url}";
		}

		protected string GetCourseUrl(Course course, string baseUrl)
		{
			return baseUrl + $"/Course/{course.Id}/";
		}

		protected static string GetGroupsUrl(Course course, string baseUrl)
		{
			return baseUrl + $"/Admin/Groups?courseId={course.Id.EscapeHtml()}";
		}
	}

	public static class NotificationExtensions
	{
		public static NotificationType GetNotificationType(this Type notificationClass)
		{
			if (!notificationClass.IsSubclassOf(typeof(Notification)))
				throw new ArgumentException(@"Should be subclass of Notification class", nameof(notificationClass));

			var attribute = (NotificationTypeAttribute)notificationClass.GetCustomAttribute(typeof(NotificationTypeAttribute), false);
			return attribute.Type;
		}

		public static NotificationType GetNotificationType(this Notification notification)
		{
			return GetNotificationType(ObjectContext.GetObjectType(notification.GetType()));
		}
	}

	[NotificationType(NotificationType.SystemMessage)]
	public class SystemMessageNotification : Notification
	{
		[Required]
		public string Text { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>Сообщение от ulearn.me:</b><br/><br/>{Text.EscapeHtml()}";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"Сообщение от ulearn.me:\n\n{Text}";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return null;
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			/* If you want to send system message you should create NotificationDelivery yourself. By default nobody receives it */
			return new List<string>();
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	[NotificationType(NotificationType.InstructorMessage)]
	public class InstructorMessageNotification : Notification
	{
		[Required]
		public string Text { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>Сообщение от преподавателя:</b><br/><br/>{Text.EscapeHtml()}";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"Сообщение от преподавателя:\n\n{Text}";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return null;
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			/* If you want to send message you should create NotificationDelivery yourself. By default nobody receives it */
			return new List<string>();
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	public abstract class AbstractCommentNotification : Notification
	{
		[Required]
		public int CommentId { get; set; }

		public virtual Comment Comment { get; set; }

		protected string GetCommentUrl(Course course, Slide slide, string baseUrl)
		{
			return GetSlideUrl(course, slide, baseUrl) + "#comment-" + Comment.Id;
		}

		public override bool IsActual()
		{
			return !Comment.IsDeleted && Comment.IsApproved;
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return new NotificationButton("Перейти к комментарию", GetCommentUrl(course, slide, baseUrl));
		}

		protected string GetHtmlCommentText(bool isCitation=false)
		{
			return GetHtmlCommentText(Comment, isCitation);
		}

		protected string GetHtmlCommentText(Comment comment, bool isCitation=false)
		{
			var html = comment.Text.Trim()
				.EscapeHtml()
				.RenderSimpleMarkdown(isHtml: false, telegramMode: true)
				.Replace("\n", "<br/>" + (isCitation ? "&gt; " : ""));

			if (isCitation)
				html = "&gt; " + html;

			return html;
		}
	}

	[NotificationType(NotificationType.NewComment)]
	public class NewCommentNotification : AbstractCommentNotification
	{
		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return $"<b>{Comment.Author.VisibleName.EscapeHtml()} прокомментировал{Comment.Author.Gender.ChooseEnding()} «{GetSlideTitle(course, slide).EscapeHtml()}»</b><br/><br/>" +
					$"{GetHtmlCommentText()}";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return $"{Comment.Author.VisibleName} прокомментировал{Comment.Author.Gender.ChooseEnding()} «{GetSlideTitle(course, slide)}»\n\n{Comment.Text.Trim()}";
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new VisitsRepo(db).GetCourseUsers(CourseId);
		}

		public override bool IsNotificationForEveryone => true;

		public override List<Notification> GetBlockerNotifications(ULearnDb db)
		{
			return new NotificationsRepo(db).FindNotifications<RepliedToYourCommentNotification>(n => n.CommentId == CommentId).Cast<Notification>().ToList();
		}
	}

	[NotificationType(NotificationType.RepliedToYourComment)]
	public class RepliedToYourCommentNotification : AbstractCommentNotification
	{
		[Required]
		public int ParentCommentId { get; set; }

		public virtual Comment ParentComment { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return $"<b>{Comment.Author.VisibleName.EscapeHtml()} ответил{Comment.Author.Gender.ChooseEnding()} на ваш комментарий в «{GetSlideTitle(course, slide).EscapeHtml()}»</b><br/><br/>" +
					$"{GetHtmlCommentText(ParentComment, isCitation: true)}<br/><br/>" +
					$"{GetHtmlCommentText()}<br/>";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return $"{Comment.Author.VisibleName} ответил{Comment.Author.Gender.ChooseEnding()} на ваш комментарий в «{GetSlideTitle(course, slide)}»\n\n" +
					$"> {ParentComment.Text.Trim().Replace("\n", "\n> ")}\n\n" +
					$"{Comment.Text.Trim()}";
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { ParentComment.AuthorId };
		}
	}

	[NotificationType(NotificationType.LikedYourComment)]
	public class LikedYourCommentNotification : AbstractCommentNotification
	{
		[Required]
		[StringLength(64)]
		public string LikedUserId { get; set; }

		public virtual ApplicationUser LikedUser { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return $"<b>{InitiatedBy.VisibleName.EscapeHtml()} лайкнул{InitiatedBy.Gender.ChooseEnding()} ваш комментарий в «{GetSlideTitle(course, slide).EscapeHtml()}»</b>:<br/><br/>" +
					$"{GetHtmlCommentText(isCitation: true)}";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment.SlideId);
			if (slide == null)
				return null;

			return $"{InitiatedBy.VisibleName} лайкнул{InitiatedBy.Gender.ChooseEnding()} ваш комментарий в «{GetSlideTitle(course, slide)}»\n\n" +
					$"> {Comment.Text.Trim().Replace("\n", "\n >")}";
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Comment.AuthorId };
		}
	}

	public abstract class AbstractCodeReviewNotification : Notification
	{
		protected string GetReviewText(ExerciseCodeReview review, string[] solutionCodeLines, bool html, bool withAuthorsNames)
		{
			var reviewText = "";

			var reviewPosition = review.StartLine == review.FinishLine
				? $"Строка {review.StartLine + 1}"
				: $"Строки {review.StartLine + 1}–{review.FinishLine + 1}";

			if (html)
			{
				reviewText += $"<b>{reviewPosition}</b>";
				
				var codeFragment = GetSolutionCodeFragments(solutionCodeLines, review).EscapeHtml().LineEndingsToBrTags();
				var reviewCommentHtml = review.Comment.EscapeHtml().RenderSimpleMarkdown(isHtml: false, telegramMode: true).LineEndingsToBrTags();
				reviewText += $"<br/><pre>{codeFragment}</pre>";

				var comments = review.NotDeletedComments;
				if (comments.Any())
				{
					if (withAuthorsNames)
						reviewText += $"<i>{review.Author.VisibleName.EscapeHtml()}:</i><br/>";
					reviewText += reviewCommentHtml;
					
					foreach (var comment in comments)
					{
						reviewText += "<br/><br/>";
						var commentHtml = comment.Text.EscapeHtml().RenderSimpleMarkdown(isHtml: false, telegramMode: true).LineEndingsToBrTags();
						if (withAuthorsNames)
							reviewText += $"<i>{comment.Author.VisibleName.EscapeHtml()}:</i><br/>";
						reviewText += commentHtml;
					}

					reviewText += "<br/><br/>";
				}
				else
				{
					reviewText += $"Комментарий: {reviewCommentHtml}<br/><br/>";	
				}
			}
			else
			{
				reviewText += reviewPosition;
				reviewText += review.Comment + "\n\n";
				var comments = review.NotDeletedComments;
				foreach (var comment in comments)
				{
					if (withAuthorsNames)
						reviewText += comment.Author.VisibleName + ":\n";
					reviewText += comment.Text + "\n\n";
				}
			}

			return reviewText;
		}
		
		protected string GetReviewsText(ManualExerciseChecking checking, bool html)
		{
			var commentsText = "";
			if (checking.NotDeletedReviews.Count > 0)
			{
				var solutionCodeLines = checking.Submission.SolutionCode.Text.SplitToLines();
				commentsText = "";
				var reviewIndex = 0;
				foreach (var review in checking.NotDeletedReviews)
				{
					commentsText += $"{++reviewIndex}. {GetReviewText(review, solutionCodeLines, html, withAuthorsNames: false)}";
				}
			}
			return commentsText;
		}

		protected string GetSolutionCodeFragments(IReadOnlyList<string> solutionCodeLines, ExerciseCodeReview review)
		{
			if (review.StartLine == review.FinishLine)
				return solutionCodeLines[review.StartLine].Substring(review.StartPosition, review.FinishPosition - review.StartPosition);

			var startLineStub = string.Join("", Enumerable.Repeat(" ", review.StartPosition));
			var startLineEnding = solutionCodeLines[review.StartLine].Substring(review.StartPosition);
			var mediumLines = string.Join("\n", solutionCodeLines.Skip(review.StartLine + 1).Take(review.FinishLine - review.StartLine - 1));
			var finishLineBeginning = solutionCodeLines[review.FinishLine].Substring(0, review.FinishPosition);
			return startLineStub + startLineEnding + "\n" + mediumLines + "\n" + finishLineBeginning;
		}
	}

	[NotificationType(NotificationType.PassedManualExerciseChecking)]
	public class PassedManualExerciseCheckingNotification : AbstractCodeReviewNotification
	{
		[Required]
		public int CheckingId { get; set; }

		public virtual ManualExerciseChecking Checking { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Checking.SlideId);
			if (slide == null)
				return null;

			var commentsText = GetReviewsText(Checking, html: true);

			return $"{InitiatedBy.VisibleName.EscapeHtml()} проверил{InitiatedBy.Gender.ChooseEnding()} ваше решение в «{GetSlideTitle(course, slide).EscapeHtml()}»<br/><br/>" +
					$"<b>Вы получили {Checking.Score.PluralizeInRussian(RussianPluralizationOptions.Score)}</b><br/><br/>" +
					commentsText;
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Checking.SlideId);
			if (slide == null)
				return null;

			var commentsText = GetReviewsText(Checking, html: false);

			return $"{InitiatedBy.VisibleName} проверил{InitiatedBy.Gender.ChooseEnding()} ваше решение в «{GetSlideTitle(course, slide)}»\n" +
					$"Вы получили {Checking.Score.PluralizeInRussian(RussianPluralizationOptions.Score)}\n\n" +
					commentsText;
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Checking.SlideId);
			if (slide == null)
				return null;

			return new NotificationButton("Перейти к странице с заданием", GetSlideUrl(course, slide, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Checking.UserId };
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	[NotificationType(NotificationType.PassedManualQuizChecking)]
	public class PassedManualQuizCheckingNotification : Notification
	{
		[Required]
		public int CheckingId { get; set; }

		public virtual ManualQuizChecking Checking { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Checking.SlideId);
			if (slide == null)
				return null;

			return $"<b>{InitiatedBy.VisibleName.EscapeHtml()} проверил{InitiatedBy.Gender.ChooseEnding()} ваш тест «{GetSlideTitle(course, slide).EscapeHtml()}»:</b><br/>" +
					$"вы получили {Checking.Score.PluralizeInRussian(RussianPluralizationOptions.Score)}";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Checking.SlideId);
			if (slide == null)
				return null;

			return $"{InitiatedBy.VisibleName} проверил{InitiatedBy.Gender.ChooseEnding()} ваш тест «{GetSlideTitle(course, slide)}»:\n" +
					$"вы получили {Checking.Score.PluralizeInRussian(RussianPluralizationOptions.Score)}";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Checking.SlideId);
			if (slide == null)
				return null;

			return new NotificationButton("Перейти к странице с тестом", GetSlideUrl(course, slide, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Checking.UserId };
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	[NotificationType(NotificationType.ReceivedCertificate)]
	public class ReceivedCertificateNotification : Notification
	{
		[Required]
		public Guid CertificateId { get; set; }

		public virtual Certificate Certificate { get; set; }

		private static string GetCertificateUrl(Certificate certificate, string baseUrl)
		{
			return baseUrl + $"/Certificate/{certificate.Id}";
		}

		private static string GetCertificatesListUrl(ApplicationUser user, string baseUrl)
		{
			return baseUrl + "/CertificatesList";
		}

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>Поздравляем! Вы получили сертификат по курсу «{course.Title.EscapeHtml()}».</b><br/><br/>" +
					$"Посмотреть сертификат можно <a href=\"{GetCertificateUrl(Certificate, baseUrl).EscapeHtml()}\">по ссылке</a> " +
					$"или в любой момент на <a href=\"{GetCertificatesListUrl(Certificate.User, baseUrl).EscapeHtml()}\">{GetCertificatesListUrl(Certificate.User, baseUrl)}</a>.<br/><br/>" +
					"Поделитесь ссылкой на сертификат с друзьями в социальных сетях — пусть ваше достижение увидят все!";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"Поздравляем! Вы получили сертификат по курсу «{course.Title}».\n\n" +
					$"Посмотреть сертификат можно по ссылке {GetCertificateUrl(Certificate, baseUrl)} или в любой момент на {GetCertificatesListUrl(Certificate.User, baseUrl)}.\n\n" +
					"Поделитесь ссылкой на сертификат с друзьями в социальных сетях — пусть ваше достижение увидят все!";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Смотреть сертификат", GetCertificateUrl(Certificate, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Certificate.UserId };
		}

		public override bool IsActual()
		{
			return !Certificate.IsDeleted;
		}
	}

	[NotificationType(NotificationType.ReceivedAdditionalScore)]
	public class ReceivedAdditionalScoreNotification : Notification
	{
		public int? ScoreId { get; set; }

		public virtual AdditionalScore Score { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var unit = course.FindUnitById(Score.UnitId);
			var scoringGroup = unit?.Scoring.Groups.GetOrDefault(Score.ScoringGroupId, null);
			if (scoringGroup == null)
				return null;

			return $"<b>{InitiatedBy.VisibleName.EscapeHtml()}</b> поставил{InitiatedBy.Gender.ChooseEnding()} вам баллы <b>{scoringGroup.Name.EscapeHtml()}</b> в&nbsp;модуле «{GetUnitTitle(course, unit).EscapeHtml()}»:<br/>" +
					$"вы получили {Score.Score.PluralizeInRussian(RussianPluralizationOptions.Score)}<br/><br/>" +
					"Полную ведомость смотрите на&nbsp;<a href=\"https://ulearn.me\">ulearn.me</a>.";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var unit = course.FindUnitById(Score.UnitId);
			var scoringGroup = unit?.Scoring.Groups.GetOrDefault(Score.ScoringGroupId, null);
			if (scoringGroup == null)
				return null;

			return $"{InitiatedBy.VisibleName} поставил{InitiatedBy.Gender.ChooseEnding()} вам баллы {scoringGroup.Name} в модуле «{GetUnitTitle(course, unit)}»:\n" +
					$"вы получили {Score.Score.PluralizeInRussian(RussianPluralizationOptions.Score)}\n\n" +
					"Полную ведомость смотрите на ulearn.me.";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return null;
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Score.UserId };
		}

		public override bool IsActual()
		{
			return ScoreId != null && Score != null;
		}
	}

	[NotificationType(NotificationType.ReceivedCommentToCodeReview)]
	public class ReceivedCommentToCodeReviewNotification : AbstractCodeReviewNotification
	{
		public int? CommentId { get; set; }
		
		public virtual ExerciseCodeReviewComment Comment { get; set; }
		
		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var checking = Comment?.Review?.ExerciseChecking;
			if (checking == null)
				return null;
			
			var slide = course.FindSlideById(checking.SlideId);
			if (slide == null)
				return null;

			var messagePrefix = $"{InitiatedBy.VisibleName.EscapeHtml()} оставил{InitiatedBy.Gender.ChooseEnding()} комментарий в код-ревью задания «{GetSlideTitle(course, slide).EscapeHtml()}»<br/><br/>";
			if (transport is MailNotificationTransport)
			{
				var solutionCodeLines = checking.Submission.SolutionCode.Text.SplitToLines();
				var commentsText = GetReviewText(Comment.Review, solutionCodeLines, html: true, withAuthorsNames: true);

				return messagePrefix + commentsText;
			}

			if (transport is TelegramNotificationTransport)
			{
				var commentText = Comment.Text.EscapeHtml().RenderSimpleMarkdown(isHtml: false, telegramMode: true).LineEndingsToBrTags();
				return messagePrefix + commentText;
			}

			throw new Exception($"Unknown transport type: {transport.GetType()}");
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var checking = Comment?.Review?.ExerciseChecking;
			if (checking == null)
				return null;
			
			var slide = course.FindSlideById(checking.SlideId);
			if (slide == null)
				return null;

			var messagePrefix = $"{InitiatedBy.VisibleName} оставил{InitiatedBy.Gender.ChooseEnding()} комментарий в код-ревью задания «{GetSlideTitle(course, slide)}»<br/><br/>";
			if (transport is MailNotificationTransport)
			{
				var solutionCodeLines = checking.Submission.SolutionCode.Text.SplitToLines();
				var commentsText = GetReviewText(Comment.Review, solutionCodeLines, html: false, withAuthorsNames: true);

				return messagePrefix + commentsText;
			}
			
			if (transport is TelegramNotificationTransport)
			{
				return messagePrefix + Comment.Text;
			}

			throw new Exception($"Unknown transport type: {transport.GetType()}");
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var slide = course.FindSlideById(Comment?.Review?.ExerciseChecking?.SlideId ?? Guid.Empty);
			if (slide == null)
				return null;

			var currentUserId = transport.UserId;
			var isStudent = currentUserId == Comment.Review.ExerciseChecking.UserId;
			var url = GetUrl(course, baseUrl, currentUserId);

			var title = isStudent ? "Перейти к странице с заданием" : "Перейти к код-ревью";
			return new NotificationButton(title, url);
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			var review = Comment.Review;
			if (review == null)
				return new List<string>();

			var authorsIds = new HashSet<string>(review.Comments.Select(c => c.AuthorId));
			authorsIds.Add(review.AuthorId);
			authorsIds.Add(review.ExerciseChecking.UserId);

			return authorsIds.ToList();
		}

		public override bool IsActual()
		{
			return CommentId != null && Comment != null;
		}
		
		public override List<Notification> GetBlockerNotifications(ULearnDb db)
		{
			var reviewId = Comment.ReviewId;
			return new NotificationsRepo(db)
				.FindNotifications<ReceivedCommentToCodeReviewNotification>(n => n.Comment.ReviewId == reviewId, n => n.Comment)
				.Cast<Notification>()
				.Where(n => n.CreateTime < CreateTime && n.CreateTime >= CreateTime - NotificationsRepo.sendNotificationsDelayAfterCreating)
				.ToList();
		}

		public string GetUrl(Course course, string baseUrl, string currentUserId)
		{
			var slide = course.FindSlideById(Comment?.Review?.ExerciseChecking?.SlideId ?? Guid.Empty);
			if (slide == null)
				return null;
			
			var isStudent = currentUserId == Comment.Review.ExerciseChecking.UserId;
			var url = GetSlideUrl(course, slide, baseUrl);
			if (!isStudent)
				url += $"?CheckQueueItemId={Comment.Review.ExerciseCheckingId}";
			return url;
		}
	}

	[NotificationType(NotificationType.JoinedToYourGroup)]
	public class JoinedToYourGroupNotification : Notification
	{
		[Required]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[Required]
		[StringLength(64)]
		public string JoinedUserId { get; set; }

		public virtual ApplicationUser JoinedUser { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>{JoinedUser.VisibleName.EscapeHtml()}</b> присоедини{JoinedUser.Gender.ChooseEnding("лся", "лась")} к вашей группе «{Group.Name.EscapeHtml()}» по курсу «{course.Title.EscapeHtml()}».";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"{JoinedUser.VisibleName} присоедини{JoinedUser.Gender.ChooseEnding("лся", "лась")} к вашей группе «{Group.Name}» по курсу «{course.Title}».";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Перейти к группам", GetGroupsUrl(course, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Group.OwnerId };
		}

		public override bool IsActual()
		{
			return !Group.IsDeleted;
		}
	}

	[NotificationType(NotificationType.GrantedAccessToGroup)]
	public class GrantedAccessToGroupNotification : Notification
	{
		[Required]
		public int AccessId { get; set; }

		public virtual GroupAccess Access { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>{Access.GrantedBy.VisibleName.EscapeHtml()}</b> назначил вас преподавателем группы <b>«{Access.Group.Name.EscapeHtml()}»</b> в курсе «{course.Title.EscapeHtml()}».";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"{Access.GrantedBy.VisibleName} назначил вас преподавателем группы <b>«{Access.Group.Name}»</b> в курсе «{course.Title}».";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Перейти к группам", GetGroupsUrl(course, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Access.UserId };
		}

		public override bool IsActual()
		{
			return Access != null && Access.IsEnabled;
		}
	}

	[NotificationType(NotificationType.RevokedAccessToGroup)]
	public class RevokedAccessToGroupNotification : Notification
	{
		[Required]
		public int AccessId { get; set; }

		public virtual GroupAccess Access { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"Вы перестали быть преподавателем группы <b>«{Access.Group.Name.EscapeHtml()}»</b> в курсе «{course.Title.EscapeHtml()}».";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"Вы перестали быть преподавателем группы «{Access.Group.Name}» в курсе «{course.Title}».";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Перейти к группам", GetGroupsUrl(course, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Access.UserId };
		}

		public override bool IsActual()
		{
			return Access != null && ! Access.IsEnabled;
		}
	}

	[NotificationType(NotificationType.GroupMemberHasBeenRemoved)]
	public class GroupMemberHasBeenRemovedNotification : Notification
	{
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>{InitiatedBy.VisibleName.EscapeHtml()}</b> удалил{InitiatedBy.Gender.ChooseEnding()} студента <b>{User.VisibleName.EscapeHtml()}</b> из вашей группы <b>«{Group.Name.EscapeHtml()}»</b> (курс «{course.Title.EscapeHtml()}»).";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"{InitiatedBy.VisibleName} удалил{InitiatedBy.Gender.ChooseEnding()} студента {User.VisibleName} из вашей группы «{Group.Name}» (курс «{course.Title}»).";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Перейти к группам", GetGroupsUrl(course, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			var groupsRepo = new GroupsRepo(db, WebCourseManager.Instance);
			var accesses = groupsRepo.GetGroupAccesses(GroupId);
			return accesses.Select(a => a.UserId).ToList();
		}

		public override bool IsActual()
		{
			return User != null && ! Group.IsDeleted;
		}
	}

	[NotificationType(NotificationType.AddedInstructor)]
	public class AddedInstructorNotification : Notification
	{
		[Required]
		[StringLength(64)]
		public string AddedUserId { get; set; }

		public virtual ApplicationUser AddedUser { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>{AddedUser.VisibleName.EscapeHtml()}</b> стал{AddedUser.Gender.ChooseEnding()} преподавателем курса «{course.Title.EscapeHtml()}».";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"{AddedUser.VisibleName} стал{AddedUser.Gender.ChooseEnding()} преподавателем курса «{course.Title}».";
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new UserRolesRepo(db).GetListOfUsersWithCourseRole(CourseRole.CourseAdmin, CourseId);
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return null;
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	[NotificationType(NotificationType.CreatedGroup)]
	public class CreatedGroupNotification : Notification
	{
		[Required]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"<b>{Group.Owner.VisibleName.EscapeHtml()}</b> создал{Group.Owner.Gender.ChooseEnding()} новую группу <b>«{Group.Name.EscapeHtml()}»</b> в курсе «{course.Title.EscapeHtml()}».";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"{Group.Owner.VisibleName} создал{Group.Owner.Gender.ChooseEnding()} новую группу «{Group.Name}» в курсе «{course.Title}».";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Перейти к группам", GetGroupsUrl(course, baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new UserRolesRepo(db).GetListOfUsersWithCourseRole(CourseRole.CourseAdmin, CourseId);
		}

		public override bool IsActual()
		{
			return !Group.IsDeleted;
		}
	}

	public abstract class AbstractPackageNotification : Notification
	{
		protected static string GetPackagesUrl(Course course, string baseUrl)
		{
			return baseUrl + $"/Admin/Packages?courseId={course.Id.EscapeHtml()}";
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Перейти к загруженным версиям", GetPackagesUrl(course, baseUrl));
		}
	}

	[NotificationType(NotificationType.UploadedPackage)]
	public class UploadedPackageNotification : AbstractPackageNotification
	{
		[Required]
		public Guid CourseVersionId { get; set; }

		public virtual CourseVersion CourseVersion { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"Загружена новая версия курса <b>«{course.Title.EscapeHtml()}»</b>. Теперь её можно опубликовать.";
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"Загружена новая версия курса «{course.Title.EscapeHtml()}». Теперь её можно опубликовать.";
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new UserRolesRepo(db).GetListOfUsersWithCourseRole(CourseRole.CourseAdmin, CourseId);
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	[NotificationType(NotificationType.PublishedPackage)]
	public class PublishedPackageNotification : AbstractPackageNotification
	{
		[Required]
		public Guid CourseVersionId { get; set; }

		public virtual CourseVersion CourseVersion { get; set; }

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return $"Опубликована новая версия курса <b>«{course.Title.EscapeHtml()}»</b>.<br/><br/>" +
					GetCourseUrl(course, baseUrl).EscapeHtml();
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			return $"Опубликована новая версия курса «{course.Title.EscapeHtml()}».\n\n" +
					GetCourseUrl(course, baseUrl);
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new UserRolesRepo(db).GetListOfUsersWithCourseRole(CourseRole.CourseAdmin, CourseId);
		}

		public override bool IsActual()
		{
			return true;
		}
	}

	[NotificationType(NotificationType.CourseExportedToStepik)]
	public class CourseExportedToStepikNotification : Notification
	{
		[Required]
		public int ProcessId { get; set; }
		public virtual StepikExportProcess Process { get; set; }

		/* TODO (andgein): Process.UlearnCourseId should be safely urlized */
		private string GetStepikExportProcessUrl(string baseUrl)
		{
			return baseUrl + $"/Stepik/Process?courseId={Process.UlearnCourseId}&processId={ProcessId}";
		}

		public override string GetHtmlMessageForDelivery(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			var isInitial = Process.IsInitialExport;
			if (Process.IsSuccess)
			{
				return $"Курс <b>«{Process.StepikCourseTitle.EscapeHtml()}»</b> на Степике успешно {(isInitial ? "скопирован" : "обновлён")} из курса <b>«{course.Title.EscapeHtml()}»</b>.";
			}
			else
			{
				return $"<b>Произошла ошибка</b> при {(isInitial ? "копировании" : "обновлении")} курса {course.Title.EscapeHtml()} на Степик{(isInitial ? "" : "е")}:<br/><br/>Лог:<br/>" + Process.Log;
			}
		}

		public override string GetTextMessageForDelivery(NotificationTransport transport, NotificationDelivery notificationDelivery, Course course, string baseUrl)
		{
			var isInitial = Process.IsInitialExport;
			if (Process.IsSuccess)
			{
				return $"Курс «{Process.StepikCourseTitle}» на Степике успешно {(isInitial ? "скопирован" : "обновлён")} из курса «{course.Title}».";
			}
			else
			{
				return $"Произошла ошибка при {(isInitial ? "копировании" : "обновлении")} курса {course.Title} на Степик{(isInitial ? "" : "е")}:\n\nЛог:" + Process.Log;
			}
		}

		public override NotificationButton GetNotificationButton(NotificationTransport transport, NotificationDelivery delivery, Course course, string baseUrl)
		{
			return new NotificationButton("Смотреть детали переноса курса", GetStepikExportProcessUrl(baseUrl));
		}

		public override List<string> GetRecipientsIds(ULearnDb db)
		{
			return new List<string> { Process.OwnerId };
		}

		public override bool IsActual()
		{
			return Process.IsFinished;
		}
	}
}