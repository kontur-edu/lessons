using System;

namespace Ulearn.Common.Api
{
	public class StatusCodeException : Exception
	{
		public int Code { get; }

		public StatusCodeException(int code, string message) : base(message)
		{
			Code = code;
		}
	}
}