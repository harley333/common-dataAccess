using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;

namespace EF.Utility.CS {

	public class TypeMapper {
		private const string ExternalTypeNameAttributeName = @"http://schemas.microsoft.com/ado/2006/04/codegeneration:ExternalTypeName";

		private readonly System.Collections.IList _errors;
		private readonly CodeGenerationTools _code;
		private readonly MetadataTools _ef;

		public static string FixNamespaces(string typeName) {
			return typeName.Replace("System.Data.Spatial.", "System.Data.Entity.Spatial.");
		}

		public TypeMapper(CodeGenerationTools code, MetadataTools ef, System.Collections.IList errors) {
			Util.ArgumentNotNull(code, "code");
			Util.ArgumentNotNull(ef, "ef");
			Util.ArgumentNotNull(errors, "errors");

			_code = code;
			_ef = ef;
			_errors = errors;
		}

		public string GetTypeName(TypeUsage typeUsage) {
			return typeUsage == null ? null : GetTypeName(typeUsage.EdmType, _ef.IsNullable(typeUsage), modelNamespace: null);
		}

		public string GetTypeName(EdmType edmType) {
			return GetTypeName(edmType, isNullable: null, modelNamespace: null);
		}

		public string GetTypeName(TypeUsage typeUsage, string modelNamespace) {
			return typeUsage == null ? null : GetTypeName(typeUsage.EdmType, _ef.IsNullable(typeUsage), modelNamespace);
		}

		public string GetTypeName(EdmType edmType, string modelNamespace) {
			return GetTypeName(edmType, isNullable: null, modelNamespace: modelNamespace);
		}

		public string GetTypeName(EdmType edmType, bool? isNullable, string modelNamespace) {
			if (edmType == null) {
				return null;
			}

			var collectionType = edmType as CollectionType;
			if (collectionType != null) {
				return String.Format(CultureInfo.InvariantCulture, "ICollection<{0}>", GetTypeName(collectionType.TypeUsage, modelNamespace));
			}

			var typeName = _code.Escape(edmType.MetadataProperties
									.Where(p => p.Name == ExternalTypeNameAttributeName)
									.Select(p => (string)p.Value)
									.FirstOrDefault())
				?? (modelNamespace != null && edmType.NamespaceName != modelNamespace ?
					_code.CreateFullName(_code.EscapeNamespace(edmType.NamespaceName), _code.Escape(edmType)) :
					_code.Escape(edmType));

			if (edmType is StructuralType) {
				return typeName;
			}

			if (edmType is SimpleType) {
				var clrType = UnderlyingClrType(edmType);
				if (!IsEnumType(edmType)) {
					typeName = _code.Escape(clrType);
				}

				typeName = FixNamespaces(typeName);

				return clrType.IsValueType && isNullable == true ?
					String.Format(CultureInfo.InvariantCulture, "Nullable<{0}>", typeName) :
					typeName;
			}

			throw new ArgumentException("edmType");
		}

		public Type UnderlyingClrType(EdmType edmType) {
			Util.ArgumentNotNull(edmType, "edmType");

			var primitiveType = edmType as PrimitiveType;
			if (primitiveType != null) {
				return primitiveType.ClrEquivalentType;
			}

			if (IsEnumType(edmType)) {
				return GetEnumUnderlyingType(edmType).ClrEquivalentType;
			}

			return typeof(object);
		}

		public object GetEnumMemberValue(MetadataItem enumMember) {
			Util.ArgumentNotNull(enumMember, "enumMember");

			var valueProperty = enumMember.GetType().GetProperty("Value");
			return valueProperty == null ? null : valueProperty.GetValue(enumMember, null);
		}

		public string GetEnumMemberName(MetadataItem enumMember) {
			Util.ArgumentNotNull(enumMember, "enumMember");

			var nameProperty = enumMember.GetType().GetProperty("Name");
			return nameProperty == null ? null : (string)nameProperty.GetValue(enumMember, null);
		}

		public System.Collections.IEnumerable GetEnumMembers(EdmType enumType) {
			Util.ArgumentNotNull(enumType, "enumType");

			var membersProperty = enumType.GetType().GetProperty("Members");
			return membersProperty != null
				? (System.Collections.IEnumerable)membersProperty.GetValue(enumType, null)
				: Enumerable.Empty<MetadataItem>();
		}

		public bool EnumIsFlags(EdmType enumType) {
			Util.ArgumentNotNull(enumType, "enumType");

			var isFlagsProperty = enumType.GetType().GetProperty("IsFlags");
			return isFlagsProperty != null && (bool)isFlagsProperty.GetValue(enumType, null);
		}

		public bool IsEnumType(GlobalItem edmType) {
			Util.ArgumentNotNull(edmType, "edmType");

			return edmType.GetType().Name == "EnumType";
		}

		public PrimitiveType GetEnumUnderlyingType(EdmType enumType) {
			Util.ArgumentNotNull(enumType, "enumType");

			return (PrimitiveType)enumType.GetType().GetProperty("UnderlyingType").GetValue(enumType, null);
		}

		public string CreateLiteral(object value) {
			if (value == null || value.GetType() != typeof(TimeSpan)) {
				return _code.CreateLiteral(value);
			}

			return string.Format(CultureInfo.InvariantCulture, "new TimeSpan({0})", ((TimeSpan)value).Ticks);
		}

		public bool VerifyCaseInsensitiveTypeUniqueness(IEnumerable<string> types, string sourceFile) {
			Util.ArgumentNotNull(types, "types");
			Util.ArgumentNotNull(sourceFile, "sourceFile");

			var hash = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			if (types.Any(item => !hash.Add(item))) {
				_errors.Add(
					new CompilerError(sourceFile, -1, -1, "6023",
						String.Format(CultureInfo.CurrentCulture, CodeGenerationTools.GetResourceString("Template_CaseInsensitiveTypeConflict"))));
				return false;
			}
			return true;
		}

		public IEnumerable<SimpleType> GetEnumItemsToGenerate(IEnumerable<GlobalItem> itemCollection) {
			return GetItemsToGenerate<SimpleType>(itemCollection)
				.Where(e => IsEnumType(e));
		}

		public IEnumerable<T> GetItemsToGenerate<T>(IEnumerable<GlobalItem> itemCollection) where T : EdmType {
			return itemCollection
				.OfType<T>()
				.Where(i => !i.MetadataProperties.Any(p => p.Name == ExternalTypeNameAttributeName))
				.OrderBy(i => i.Name);
		}

		public IEnumerable<string> GetAllGlobalItems(IEnumerable<GlobalItem> itemCollection) {
			return itemCollection
				.Where(i => i is EntityType || i is ComplexType || i is EntityContainer || IsEnumType(i))
				.Select(g => GetGlobalItemName(g));
		}

		public string GetGlobalItemName(GlobalItem item) {
			if (item is EdmType) {
				return ((EdmType)item).Name;
			} else {
				return ((EntityContainer)item).Name;
			}
		}

		public IEnumerable<EdmProperty> GetSimpleProperties(EntityType type) {
			return type.Properties.Where(p => p.TypeUsage.EdmType is SimpleType && p.DeclaringType == type);
		}

		public IEnumerable<EdmProperty> GetSimpleProperties(ComplexType type) {
			return type.Properties.Where(p => p.TypeUsage.EdmType is SimpleType && p.DeclaringType == type);
		}

		public IEnumerable<EdmProperty> GetComplexProperties(EntityType type) {
			return type.Properties.Where(p => p.TypeUsage.EdmType is ComplexType && p.DeclaringType == type);
		}

		public IEnumerable<EdmProperty> GetComplexProperties(ComplexType type) {
			return type.Properties.Where(p => p.TypeUsage.EdmType is ComplexType && p.DeclaringType == type);
		}

		public IEnumerable<EdmProperty> GetPropertiesWithDefaultValues(EntityType type) {
			return type.Properties.Where(p => p.TypeUsage.EdmType is SimpleType && p.DeclaringType == type && p.DefaultValue != null);
		}

		public IEnumerable<EdmProperty> GetPropertiesWithDefaultValues(ComplexType type) {
			return type.Properties.Where(p => p.TypeUsage.EdmType is SimpleType && p.DeclaringType == type && p.DefaultValue != null);
		}

		public IEnumerable<NavigationProperty> GetNavigationProperties(EntityType type) {
			return type.NavigationProperties.Where(np => np.DeclaringType == type);
		}

		public IEnumerable<NavigationProperty> GetCollectionNavigationProperties(EntityType type) {
			return type.NavigationProperties.Where(np => np.DeclaringType == type && np.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many);
		}

		public FunctionParameter GetReturnParameter(EdmFunction edmFunction) {
			Util.ArgumentNotNull(edmFunction, "edmFunction");

			var returnParamsProperty = edmFunction.GetType().GetProperty("ReturnParameters");
			return returnParamsProperty == null
				? edmFunction.ReturnParameter
				: ((IEnumerable<FunctionParameter>)returnParamsProperty.GetValue(edmFunction, null)).FirstOrDefault();
		}

		public bool IsComposable(EdmFunction edmFunction) {
			Util.ArgumentNotNull(edmFunction, "edmFunction");

			var isComposableProperty = edmFunction.GetType().GetProperty("IsComposableAttribute");
			return isComposableProperty != null && (bool)isComposableProperty.GetValue(edmFunction, null);
		}

		public IEnumerable<FunctionImportParameter> GetParameters(EdmFunction edmFunction) {
			return FunctionImportParameter.Create(edmFunction.Parameters, _code, _ef);
		}

		public TypeUsage GetReturnType(EdmFunction edmFunction) {
			var returnParam = GetReturnParameter(edmFunction);
			return returnParam == null ? null : _ef.GetElementType(returnParam.TypeUsage);
		}

		public bool GenerateMergeOptionFunction(EdmFunction edmFunction, bool includeMergeOption) {
			var returnType = GetReturnType(edmFunction);
			return !includeMergeOption && returnType != null && returnType.EdmType.BuiltInTypeKind == BuiltInTypeKind.EntityType;
		}
	}

}
