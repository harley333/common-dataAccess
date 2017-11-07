using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;

namespace EF.Utility.CS {

	public class CodeStringGenerator {
		private readonly CodeGenerationTools _code;
		private readonly TypeMapper _typeMapper;
		private readonly MetadataTools _ef;

		public CodeStringGenerator(CodeGenerationTools code, TypeMapper typeMapper, MetadataTools ef) {
			Util.ArgumentNotNull(code, "code");
			Util.ArgumentNotNull(typeMapper, "typeMapper");
			Util.ArgumentNotNull(ef, "ef");

			_code = code;
			_typeMapper = typeMapper;
			_ef = ef;
		}

		public string Property(EdmProperty edmProperty, String ownerName) {
			var summary = Util.GetSummary(edmProperty, true);
			var localization = string.Format(
				CultureInfo.InvariantCulture,
				"[Display(ResourceType = typeof({0}_Resources), Name = \"{1}_Name\", ShortName = \"{1}_ShortName\")]{2}\t",
				ownerName,
				_code.Escape(edmProperty),
				Environment.NewLine
			);
			var key = _ef.IsKey(edmProperty) ? "[Key]" + Environment.NewLine + "\t" : "";
			return summary + key + localization + Property(edmProperty);
		}

		public string Property(EdmProperty edmProperty) {
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} {1} {2} {{ {3}get; {4}set; }}",
				Accessibility.ForProperty(edmProperty),
				_typeMapper.GetTypeName(edmProperty.TypeUsage),
				_code.Escape(edmProperty),
				_code.SpaceAfter(Accessibility.ForGetter(edmProperty)),
				_code.SpaceAfter(Accessibility.ForSetter(edmProperty)));
		}

		public string NavigationProperty(NavigationProperty navProp) {
			var endType = _typeMapper.GetTypeName(navProp.ToEndMember.GetEntityType());
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} {1} {2} {{ {3}get; {4}set; }}",
				AccessibilityAndVirtual(Accessibility.ForNavigationProperty(navProp)),
				navProp.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many ? ("ICollection<" + endType + ">") : endType,
				_code.Escape(navProp),
				_code.SpaceAfter(Accessibility.ForGetter(navProp)),
				_code.SpaceAfter(Accessibility.ForSetter(navProp)));
		}

		public string AccessibilityAndVirtual(string accessibility) {
			return accessibility + (accessibility != "private" ? " virtual" : "");
		}

		public string EntityClassOpening(EntityType entity, string attributes = null) {
			if (string.IsNullOrWhiteSpace(attributes)) {
				attributes = "";
			}
			if (!string.IsNullOrWhiteSpace(attributes)) {
				attributes = attributes + Environment.NewLine;
			}
			string summary = Util.GetSummary(entity, false);
			return summary + attributes + "[Serializable]" + Environment.NewLine +
				string.Format(
					CultureInfo.InvariantCulture,
					"{0} {1}partial class {2}{3}",
					Accessibility.ForType(entity),
					_code.SpaceAfter(_code.AbstractOption(entity)),
					_code.Escape(entity),
					_code.StringBefore(" : ", _typeMapper.GetTypeName(entity.BaseType))
				);
		}

		public string EnumOpening(SimpleType enumType) {
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} enum {1} : {2}",
				Accessibility.ForType(enumType),
				_code.Escape(enumType),
				_code.Escape(_typeMapper.UnderlyingClrType(enumType)));
		}

		public void WriteFunctionParameters(EdmFunction edmFunction, Action<string, string, string, string> writeParameter) {
			var parameters = FunctionImportParameter.Create(edmFunction.Parameters, _code, _ef);
			foreach (var parameter in parameters.Where(p => p.NeedsLocalVariable)) {
				var isNotNull = parameter.IsNullableOfT ? parameter.FunctionParameterName + ".HasValue" : parameter.FunctionParameterName + " != null";
				var notNullInit = "new ObjectParameter(\"" + parameter.EsqlParameterName + "\", " + parameter.FunctionParameterName + ")";
				var nullInit = "new ObjectParameter(\"" + parameter.EsqlParameterName + "\", typeof(" + TypeMapper.FixNamespaces(parameter.RawClrTypeName) + "))";
				writeParameter(parameter.LocalVariableName, isNotNull, notNullInit, nullInit);
			}
		}

		public string ComposableFunctionMethod(EdmFunction edmFunction, string modelNamespace) {
			var parameters = _typeMapper.GetParameters(edmFunction);

			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} IQueryable<{1}> {2}({3})",
				AccessibilityAndVirtual(Accessibility.ForMethod(edmFunction)),
				_typeMapper.GetTypeName(_typeMapper.GetReturnType(edmFunction), modelNamespace),
				_code.Escape(edmFunction),
				string.Join(", ", parameters.Select(p => TypeMapper.FixNamespaces(p.FunctionParameterType) + " " + p.FunctionParameterName).ToArray()));
		}

		public string ComposableCreateQuery(EdmFunction edmFunction, string modelNamespace) {
			var parameters = _typeMapper.GetParameters(edmFunction);

			return string.Format(
				CultureInfo.InvariantCulture,
				"return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<{0}>(\"[{1}].[{2}]({3})\"{4});",
				_typeMapper.GetTypeName(_typeMapper.GetReturnType(edmFunction), modelNamespace),
				edmFunction.NamespaceName,
				edmFunction.Name,
				string.Join(", ", parameters.Select(p => "@" + p.EsqlParameterName).ToArray()),
				_code.StringBefore(", ", string.Join(", ", parameters.Select(p => p.ExecuteParameterName).ToArray())));
		}

		public string FunctionMethod(EdmFunction edmFunction, string modelNamespace, bool includeMergeOption) {
			var parameters = _typeMapper.GetParameters(edmFunction);
			var returnType = _typeMapper.GetReturnType(edmFunction);

			var paramList = String.Join(", ", parameters.Select(p => TypeMapper.FixNamespaces(p.FunctionParameterType) + " " + p.FunctionParameterName).ToArray());
			if (includeMergeOption) {
				paramList = _code.StringAfter(paramList, ", ") + "MergeOption mergeOption";
			}

			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} {1} {2}({3})",
				AccessibilityAndVirtual(Accessibility.ForMethod(edmFunction)),
				returnType == null ? "int" : "ObjectResult<" + _typeMapper.GetTypeName(returnType, modelNamespace) + ">",
				_code.Escape(edmFunction),
				paramList);
		}

		public string ExecuteFunction(EdmFunction edmFunction, string modelNamespace, bool includeMergeOption) {
			var parameters = _typeMapper.GetParameters(edmFunction);
			var returnType = _typeMapper.GetReturnType(edmFunction);

			var callParams = _code.StringBefore(", ", String.Join(", ", parameters.Select(p => p.ExecuteParameterName).ToArray()));
			if (includeMergeOption) {
				callParams = ", mergeOption" + callParams;
			}

			return string.Format(
				CultureInfo.InvariantCulture,
				"return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction{0}(\"{1}\"{2});",
				returnType == null ? "" : "<" + _typeMapper.GetTypeName(returnType, modelNamespace) + ">",
				edmFunction.Name,
				callParams);
		}

		public string DbSet(EntitySet entitySet) {
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} virtual DbSet<{1}> {2} {{ get; set; }}",
				Accessibility.ForReadOnlyProperty(entitySet),
				_typeMapper.GetTypeName(entitySet.ElementType),
				_code.Escape(entitySet));
		}

		public string DbSetInitializer(EntitySet entitySet) {
			return string.Format(
				CultureInfo.InvariantCulture,
				"{0} = Set<{1}>();",
				_code.Escape(entitySet),
				_typeMapper.GetTypeName(entitySet.ElementType));
		}

		public string UsingDirectives(bool inHeader, bool includeCollections = true) {
			return inHeader == string.IsNullOrEmpty(_code.VsNamespaceSuggestion())
				? string.Format(
					CultureInfo.InvariantCulture,
					"{0}using System;{1}" +
					"{2}using System.ComponentModel.DataAnnotations;{2}",
					inHeader ? Environment.NewLine : "",
					includeCollections ? (Environment.NewLine + "using System.Collections.Generic;") : "",
					inHeader ? "" : Environment.NewLine)
				: "";
		}
	}

}
