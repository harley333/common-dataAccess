using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace EF.Utility.CS {

	public class EdmMetadataLoader {
		private readonly IDynamicHost _host;
		private readonly System.Collections.IList _errors;

		public EdmMetadataLoader(IDynamicHost host, System.Collections.IList errors) {
			Util.ArgumentNotNull(host, "host");
			Util.ArgumentNotNull(errors, "errors");

			_host = host;
			_errors = errors;
		}

		public IEnumerable<GlobalItem> CreateEdmItemCollection(string sourcePath) {
			Util.ArgumentNotNull(sourcePath, "sourcePath");

			if (!ValidateInputPath(sourcePath)) {
				return new EdmItemCollection();
			}

			var schemaElement = LoadRootElement(_host.ResolvePath(sourcePath));
			if (schemaElement != null) {
				using (var reader = schemaElement.CreateReader()) {
					IList<EdmSchemaError> errors;
					var itemCollection = EdmItemCollection.Create(new[] { reader }, null, out errors);

					ProcessErrors(errors, sourcePath);

					return itemCollection ?? new EdmItemCollection();
				}
			}
			return new EdmItemCollection();
		}

		public string GetModelNamespace(string sourcePath) {
			Util.ArgumentNotNull(sourcePath, "sourcePath");

			if (!ValidateInputPath(sourcePath)) {
				return string.Empty;
			}

			var model = LoadRootElement(_host.ResolvePath(sourcePath));
			if (model == null) {
				return string.Empty;
			}

			var attribute = model.Attribute("Namespace");
			return attribute != null ? attribute.Value : "";
		}

		private bool ValidateInputPath(string sourcePath) {
			if (sourcePath == "$" + "edmxInputFile" + "$") {
				_errors.Add(
					new CompilerError(_host.TemplateFile ?? sourcePath, 0, 0, string.Empty,
						CodeGenerationTools.GetResourceString("Template_ReplaceVsItemTemplateToken")));
				return false;
			}

			return true;
		}

		public XElement LoadRootElement(string sourcePath) {
			Util.ArgumentNotNull(sourcePath, "sourcePath");

			var root = XElement.Load(sourcePath, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
			return root.Elements()
				.Where(e => e.Name.LocalName == "Runtime")
				.Elements()
				.Where(e => e.Name.LocalName == "ConceptualModels")
				.Elements()
				.Where(e => e.Name.LocalName == "Schema")
				.FirstOrDefault()
					?? root;
		}

		private void ProcessErrors(IEnumerable<EdmSchemaError> errors, string sourceFilePath) {
			foreach (var error in errors) {
				_errors.Add(
					new CompilerError(
						error.SchemaLocation ?? sourceFilePath,
						error.Line,
						error.Column,
						error.ErrorCode.ToString(CultureInfo.InvariantCulture),
						error.Message) {
							IsWarning = error.Severity == EdmSchemaErrorSeverity.Warning
						});
			}
		}

		public bool IsLazyLoadingEnabled(EntityContainer container) {
			string lazyLoadingAttributeValue;
			var lazyLoadingAttributeName = MetadataConstants.EDM_ANNOTATION_09_02 + ":LazyLoadingEnabled";
			bool isLazyLoading;
			return !MetadataTools.TryGetStringMetadataPropertySetting(container, lazyLoadingAttributeName, out lazyLoadingAttributeValue)
				|| !bool.TryParse(lazyLoadingAttributeValue, out isLazyLoading)
				|| isLazyLoading;
		}
	}

}
