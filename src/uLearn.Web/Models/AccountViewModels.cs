using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Database.Models;
using Ulearn.Common;
using Ulearn.Core.Courses;

namespace uLearn.Web.Models
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	public class MustBeTrueAttribute : ValidationAttribute
	{
		public override bool IsValid(object value)
		{
			return value is bool && (bool)value;
		}
	}

	public class ExternalLoginConfirmationViewModel
	{
		[Required(ErrorMessage = "{0} есть у каждого пользователя")]
		[RegularExpression(@"^[^@]+$", ErrorMessage = "{0} не может содержать собачку «@»")]
		[Display(Name = "Логин")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Укажите адрес электронной почты")]
		[EmailAddress(ErrorMessage = "Не похоже на электронную почту")]
		[Display(Name = "Эл. почта")]
		public string Email { get; set; }

		[Display(Name = "Пол")]
		public Gender? Gender { get; set; }

		[Required(ErrorMessage = "{0} обязателен")]
		[StringLength(100, ErrorMessage = "{0} не может быть короче {2} символов", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Ещё раз")]
		// Workaround. Details: http://stackoverflow.com/questions/19978239/custom-errormessage-for-compare-attribute-does-not-work
#pragma warning disable 0618
		[System.Web.Mvc.Compare("Password", ErrorMessage = "Пароли отличаются")]
#pragma warning restore 0618
		public string ConfirmPassword { get; set; }

		[Display(Name = "Согласен с&nbsp;<a href=\"/Home/Terms\">правилами использования</a> сайта")]
		[MustBeTrue(ErrorMessage = "Нужно согласиться с правилами")]
		public bool AgreeWithTerms { get; set; }
	}

	public class ManageUserViewModel
	{
		[Required(ErrorMessage = "{0} обязателен")]
		[DataType(DataType.Password)]
		[Display(Name = "Текущий пароль")]
		public string OldPassword { get; set; }

		[Required(ErrorMessage = "{0} нужно обязательно ввести")]
		[StringLength(100, ErrorMessage = "{0} не может быть короче {2} символов", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Новый пароль")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Ещё раз")]
		[System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Пароли отличаются")]
		public string ConfirmPassword { get; set; }
	}

	public class LoginViewModel
	{
		[Required(ErrorMessage = "Ты забыл логин?")]
		[Display(Name = "Логин или емэйл")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Введи пароль")]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[Display(Name = "Запомнить меня")]
		public bool RememberMe { get; set; }
	}

	public class RegistrationViewModel
	{
		[Required(ErrorMessage = "{0} должен быть у каждого пользователя")]
		[RegularExpression(@"^[^@]+$", ErrorMessage = "{0} не может содержать собачку «@»")]
		[Display(Name = "Логин")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "{0} обязателен")]
		[StringLength(100, ErrorMessage = "{0} не может быть короче {2} символов", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Ещё раз")]
		// Workaround. Details: http://stackoverflow.com/questions/19978239/custom-errormessage-for-compare-attribute-does-not-work
#pragma warning disable 0618
		[System.Web.Mvc.Compare("Password", ErrorMessage = "Пароли отличаются")]
#pragma warning restore 0618
		public string ConfirmPassword { get; set; }

		[Display(Name = "Пол")]
		public Gender? Gender { get; set; }

		[Display(Name = "Эл. почта")]
		[Required(ErrorMessage = "Укажите адрес электронной почты")]
		[EmailAddress(ErrorMessage = "Не похоже на электронную почту")]
		public string Email { get; set; }

		[Display(Name = "Согласен с&nbsp;<a href=\"/Home/Terms\">правилами использования</a> сайта")]
		[MustBeTrue(ErrorMessage = "Нужно согласиться с правилами")]
		public bool AgreeWithTerms { get; set; }

		public string ReturnUrl { get; set; }

		public bool RegistrationFinished { get; set; }
	}

	public class UserViewModel
	{
		[HiddenInput]
		public ApplicationUser User { get; set; }

		public bool HasPassword { get; set; }

		[Required(ErrorMessage = "{0} должен быть у каждого пользователя")]
		[RegularExpression(@"^[^@]+$", ErrorMessage = "{0} не может содержать собачку «@»")]
		[Display(Name = "Логин")]
		public string Name { get; set; }

		[Required(ErrorMessage = "{0} — это обязательное поле")]
		[StringLength(100, ErrorMessage = "{0} не может быть короче {2} символов", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Ещё раз")]
		// Workaround. Details: http://stackoverflow.com/questions/19978239/custom-errormessage-for-compare-attribute-does-not-work
#pragma warning disable 0618
		[System.Web.Mvc.Compare("Password", ErrorMessage = "Пароли отличаются")]
#pragma warning restore 0618
		public string ConfirmPassword { get; set; }

		[Display(Name = "Имя")]
		public string FirstName { get; set; }

		[Display(Name = "Фамилия")]
		public string LastName { get; set; }

		[Display(Name = "Пол")]
		public Gender? Gender { get; set; }

		[Display(Name = "Эл. почта")]
		[Required(ErrorMessage = "Укажите адрес электронной почты")]
		[EmailAddress(ErrorMessage = "Не похоже на электронную почту")]
		public string Email { get; set; }

		/* Field for prevent error handling in AccountController.ChangeDetailsPassword() */
		public bool Render { get; set; } = false;
	}

	public class LtiUserViewModel
	{
		[Display(Name = "Имя")]
		public string FirstName { get; set; }

		[Display(Name = "Фамилия")]
		public string LastName { get; set; }

		[Display(Name = "Email")]
		public string Email { get; set; }
	}

	public class UserInfoModel
	{
		public ApplicationUser User { get; set; }
		public string GroupsNames { get; set; }
		public Dictionary<string, Course> Courses { get; set; }
		public List<Course> UserCourses { get; set; }
		public List<Certificate> Certificates { get; set; }
	}

	public class UserMenuPartialViewModel
	{
		public bool IsAuthenticated { get; set; }
		public ApplicationUser User { get; set; }
	}
}