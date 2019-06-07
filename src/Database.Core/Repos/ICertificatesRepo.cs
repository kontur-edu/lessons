using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface ICertificatesRepo
	{
		List<CertificateTemplate> GetTemplates(string courseId);
		CertificateTemplate FindTemplateById(Guid id);
		Certificate FindCertificateById(Guid id);
		List<Certificate> GetTemplateCertificates(Guid templateId);
		Task<CertificateTemplate> AddTemplate(string courseId, string name, string archiveName);
		Task<Certificate> AddCertificate(Guid templateId, string userId, string instructorId, Dictionary<string, string> parameters, bool isPreview = false);
		Task ChangeTemplateArchiveName(Guid templateId, string newArchiveName);
		Task ChangeTemplateName(Guid templateId, string name);
		Dictionary<Guid, List<Certificate>> GetCertificates(string courseId, bool includePreviews = false);
		Task RemoveTemplate(CertificateTemplate template);
		List<Certificate> GetUserCertificates(string userId, bool includePreviews = false);
		Task RemoveCertificate(Certificate certificate);
	}
}