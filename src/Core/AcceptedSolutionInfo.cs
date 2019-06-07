using System.Collections.Generic;
using System.Linq;

namespace Ulearn.Core
{
	public class AcceptedSolutionInfo
	{
		public string Code { get; private set; }
		public int Id { get; private set; }
		public List<string> UsersWhoLike { get; private set; }
		public bool LikedAlready { get; set; }
		public string RemoveSolutionUrl { get; set; }

		public AcceptedSolutionInfo(string code, int id, IEnumerable<string> usersWhoLike)
		{
			Code = code;
			Id = id;
			UsersWhoLike = usersWhoLike.ToList();
		}
	}
}