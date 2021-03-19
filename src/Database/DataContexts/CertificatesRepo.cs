using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Newtonsoft.Json;

namespace Database.DataContexts
{
	public class CertificatesRepo
	{
		private readonly ULearnDb db;

		public CertificatesRepo(ULearnDb db)
		{
			this.db = db;
		}

		public List<CertificateTemplate> GetTemplates(string courseId)
		{
			return db.CertificateTemplates.Where(t => t.CourseId == courseId && !t.IsDeleted).ToList();
		}

		public CertificateTemplate FindTemplateById(Guid id)
		{
			return db.CertificateTemplates.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
		}

		public Certificate FindCertificateById(Guid id)
		{
			return db.Certificates.FirstOrDefault(c => c.Id == id && !c.IsDeleted);
		}

		public List<Certificate> GetTemplateCertificates(Guid templateId)
		{
			return db.Certificates.Where(c => c.TemplateId == templateId && !c.IsDeleted).ToList();
		}

		public async Task<CertificateTemplate> AddTemplate(string courseId, string name, string archiveName)
		{
			var template = new CertificateTemplate
			{
				Id = Guid.NewGuid(),
				CourseId = courseId,
				Name = name,
				Timestamp = DateTime.Now,
				ArchiveName = archiveName,
			};
			db.CertificateTemplates.Add(template);
			await db.SaveChangesAsync();
			return template;
		}

		public async Task<Certificate> AddCertificate(Guid templateId, string userId, string instructorId, Dictionary<string, string> parameters, bool isPreview = false)
		{
			var certificate = new Certificate
			{
				Id = Guid.NewGuid(),
				TemplateId = templateId,
				UserId = userId,
				InstructorId = instructorId,
				Parameters = JsonConvert.SerializeObject(parameters),
				Timestamp = DateTime.Now,
				IsPreview = isPreview,
			};
			db.Certificates.Add(certificate);
			await db.SaveChangesAsync();
			return certificate;
		}

		public async Task ChangeTemplateArchiveName(Guid templateId, string newArchiveName)
		{
			var template = FindTemplateById(templateId);
			if (template == null)
				throw new ArgumentException("Invalid templateId", nameof(templateId));

			template.ArchiveName = newArchiveName;
			await db.SaveChangesAsync();
		}

		public async Task ChangeTemplateName(Guid templateId, string name)
		{
			var template = FindTemplateById(templateId);
			if (template == null)
				throw new ArgumentException("Invalid templateId", nameof(templateId));

			template.Name = name;
			await db.SaveChangesAsync();
		}

		public Dictionary<Guid, List<Certificate>> GetCertificates(string courseId, bool includePreviews = false)
		{
			var certificates = db.Certificates
				.Where(c => c.Template.CourseId == courseId && !c.IsDeleted);
			if (!includePreviews)
				certificates = certificates.Where(c => !c.IsPreview);
			return certificates
				.Include(c => c.User)
				.Include(c => c.Instructor)
				.GroupBy(c => c.TemplateId)
				.ToDictionary(g => g.Key, g => g.OrderBy(c => c.Timestamp).ToList());
		}

		public async Task RemoveTemplate(CertificateTemplate template)
		{
			template.IsDeleted = true;
			await db.SaveChangesAsync();
		}

		public List<Certificate> GetUserCertificates(string userId, bool includePreviews = false)
		{
			var certificates = db.Certificates.Where(c => c.UserId == userId && !c.IsDeleted);
			if (!includePreviews)
				certificates = certificates.Where(c => !c.IsPreview);
			return certificates.ToList();
		}

		public async Task RemoveCertificate(Certificate certificate)
		{
			certificate.IsDeleted = true;
			await db.SaveChangesAsync();
		}

		public async Task AddCertificateTemplateArchive(string archiveName, Guid certificateTemplateId, byte[] content)
		{
			var archive = new CertificateTemplateArchive
			{
				ArchiveName = archiveName,
				CertificateTemplateId = certificateTemplateId,
				Content = content
			};
			db.CertificateTemplateArchives.Add(archive);
			await db.SaveChangesAsync();
		}
	}
}