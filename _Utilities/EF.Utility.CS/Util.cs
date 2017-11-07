using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Design.PluralizationServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EF.Utility.CS {

	public static class Util {

		public static Dictionary<string, string> TemplateMetadata = new Dictionary<string, string>();

		public static void ArgumentNotNull<T>(T arg, string name) where T : class {
			if (arg == null) {
				throw new ArgumentNullException(name);
			}
		}

		public static string GetSummary(MetadataItem item, bool indent) {
			string summary = null;
			if (item.Documentation != null) {
				summary = item.Documentation.Summary;
				if (!String.IsNullOrWhiteSpace(summary)) {
					summary = summary.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
					summary = String.Format("/// <summary>{0}/// {1}{0}/// </summary>{0}",
						Environment.NewLine + (indent ? "\t" : ""),
						summary.Replace("\n", Environment.NewLine + (indent ? "\t" : "") + "/// ")
					);
				} else {
					summary = "";
				}
			}
			return summary;
		}

		private static PluralizationService GetPluralizationService() {
			var service = PluralizationService.CreateService(Thread.CurrentThread.CurrentCulture);
			var mapping = (ICustomPluralizationMapping)service;
			mapping.AddWord("SurveyInstanceStatus", "SurveyInstanceStatuses");
			mapping.AddWord("SurveyCampaignInstanceStatus", "SurveyCampaignInstanceStatuses");
			mapping.AddWord("SurveyStatus", "SurveyStatuses");
			mapping.AddWord("DatalineInstanceStatus", "DatalineInstanceStatuses");
			mapping.AddWord("ReportInstanceStatus", "ReportInstanceStatuses");
			mapping.AddWord("ReportJobInstanceStatus", "ReportJobInstanceStatuses");
			mapping.AddWord("ReportJobInstanceOutputTypeStatus", "ReportJobInstanceOutputTypeStatuses");
			
			return service;
		}

		public static string Pluralize(string value) {
           

			var service = GetPluralizationService();
			var pluralized = service.Pluralize(value);
			if (pluralized == value) {
               
				throw new Exception("Cannot distinguish between singular and pluralized: " + value);
			}
           
			return pluralized;
		}

		public static string Singularize(string value) {         
			var service = GetPluralizationService();
			var pluralized = service.Singularize(value);
			if (pluralized == value) {
				throw new Exception("Cannot distinguish between plural and singularized: " + value);
			}
			return pluralized;
		}

	}

}
