using System.Threading.Tasks;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;

namespace AntiPlagiarism.Api
{
	public interface IAntiPlagiarismClient
	{
		Task<AddSubmissionResponse> AddSubmissionAsync(AddSubmissionParameters parameters);
		Task<GetSubmissionPlagiarismsResponse> GetSubmissionPlagiarismsAsync(GetSubmissionPlagiarismsParameters parameters);
		Task<GetAuthorPlagiarismsResponse> GetAuthorPlagiarismsAsync(GetAuthorPlagiarismsParameters parameters);
	}
}