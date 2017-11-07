using System;
using System.Threading;
using System.Runtime.Serialization;

namespace Common.DataAccess {

    /// <summary>
	/// This class was generated from the "Exceptions.resx" file using the
	/// VS.CustomTools Utility (ResxT4.tt).
	/// </summary>
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("VS.CustomTools.ResxT4", "1.0.0.0")]
	public class Exceptions {

		private static global::System.Resources.ResourceManager resourceMan;

		[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
		private static global::System.Resources.ResourceManager ResourceManager {
			get {
				if (object.ReferenceEquals(resourceMan, null)) {
					global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Common.DataAccess.Exceptions", typeof(Exceptions).Assembly);
					resourceMan = temp;
				}
				return resourceMan;
			}
		}

		[global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
		private static string GetResourceString(string key, params string[] tokens) {
			var culture = Thread.CurrentThread.CurrentCulture;
			var str = ResourceManager.GetString(key, culture);

			for (int i = 0; i < tokens.Length; i += 2) {
				str = str.Replace(tokens[i], tokens[i + 1]);
			}

			return str;
		}

		///	<summary>
		///	Thrown by <see cref="Mapper{TFrom, TTo}.GetMap()"/> when a conversion method between a value type and a non-value type is attempted.
		///	</summary>
		[Serializable]
		public class Mapper_CannotConvertException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_CannotConvertException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "Cannot convert [{fromType}].{fromField} (of type {fromPropType}) to [{toType}].{toFieldName} (of type {toPropType})" This message takes into
			/// account the current system culture.
			/// </summary>
			public Mapper_CannotConvertException(string fromType, string fromField, string fromPropType, string toType, string toFieldName, string toPropType)
				: base(GetResourceString("Mapper_CannotConvertException", "{fromType}", fromType, "{fromField}", fromField, "{fromPropType}", fromPropType, "{toType}", toType, "{toFieldName}", toFieldName, "{toPropType}", toPropType)) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_CannotConvertException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public Mapper_CannotConvertException(Exception innerException, string fromType, string fromField, string fromPropType, string toType, string toFieldName, string toPropType)
				: base(GetResourceString("Mapper_CannotConvertException", "{fromType}", fromType, "{fromField}", fromField, "{fromPropType}", fromPropType, "{toType}", toType, "{toFieldName}", toFieldName, "{toPropType}", toPropType), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the Mapper_CannotConvertException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public Mapper_CannotConvertException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by <see cref="Mapper{TFrom, TTo}.GetMap()"/> when a container property being mapped <i>from</i> cannot be found on the destination object.
		///	</summary>
		[Serializable]
		public class Mapper_CannotFindPropertyContainerException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_CannotFindPropertyContainerException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "Cannot find the container property [{containerName}] on the destination object of type [{destinationTypeName}]." This message takes into
			/// account the current system culture.
			/// </summary>
			public Mapper_CannotFindPropertyContainerException(string containerName, string destinationTypeName)
				: base(GetResourceString("Mapper_CannotFindPropertyContainerException", "{containerName}", containerName, "{destinationTypeName}", destinationTypeName)) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_CannotFindPropertyContainerException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public Mapper_CannotFindPropertyContainerException(Exception innerException, string containerName, string destinationTypeName)
				: base(GetResourceString("Mapper_CannotFindPropertyContainerException", "{containerName}", containerName, "{destinationTypeName}", destinationTypeName), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the Mapper_CannotFindPropertyContainerException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public Mapper_CannotFindPropertyContainerException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by <see cref="Mapper{TFrom, TTo}.GetMap()"/> when a property being mapped <i>from</i> cannot be found on the destination object.
		///	</summary>
		[Serializable]
		public class Mapper_CannotFindPropertyException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_CannotFindPropertyException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "Cannot find the property [{propName}] on the destination object of type [{destinationTypeName}]." This message takes into
			/// account the current system culture.
			/// </summary>
			public Mapper_CannotFindPropertyException(string propName, string destinationTypeName)
				: base(GetResourceString("Mapper_CannotFindPropertyException", "{propName}", propName, "{destinationTypeName}", destinationTypeName)) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_CannotFindPropertyException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public Mapper_CannotFindPropertyException(Exception innerException, string propName, string destinationTypeName)
				: base(GetResourceString("Mapper_CannotFindPropertyException", "{propName}", propName, "{destinationTypeName}", destinationTypeName), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the Mapper_CannotFindPropertyException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public Mapper_CannotFindPropertyException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by a rendered mapping delegate when the mapping of an individual field fails.
		///	</summary>
		[Serializable]
		public class Mapper_PropertyException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_PropertyException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "Error encountered while mapping [{fromType}].{fromField} to [{toType}].{toFieldName}" This message takes into
			/// account the current system culture.
			/// </summary>
			public Mapper_PropertyException(string fromType, string fromField, string toType, string toFieldName)
				: base(GetResourceString("Mapper_PropertyException", "{fromType}", fromType, "{fromField}", fromField, "{toType}", toType, "{toFieldName}", toFieldName)) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Mapper_PropertyException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public Mapper_PropertyException(Exception innerException, string fromType, string fromField, string toType, string toFieldName)
				: base(GetResourceString("Mapper_PropertyException", "{fromType}", fromType, "{fromField}", fromField, "{toType}", toType, "{toFieldName}", toFieldName), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the Mapper_PropertyException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public Mapper_PropertyException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by <see cref="PagedData(long, int, long, System.Collections.IEnumerable)"/> when the pageSize parameter has an invalid value.
		///	</summary>
		[Serializable]
		public class PagedData_InvalidPageSizeException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_InvalidPageSizeException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "PageSize cannot be less than 1" This message takes into
			/// account the current system culture.
			/// </summary>
			public PagedData_InvalidPageSizeException()
				: base(GetResourceString("PagedData_InvalidPageSizeException")) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_InvalidPageSizeException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public PagedData_InvalidPageSizeException(Exception innerException)
				: base(GetResourceString("PagedData_InvalidPageSizeException"), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the PagedData_InvalidPageSizeException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public PagedData_InvalidPageSizeException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by <see cref="PagedData(long, int, long, System.Collections.IEnumerable)"/> when the startIndex parameter has an invalid value.
		///	</summary>
		[Serializable]
		public class PagedData_InvalidStartIndexException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_InvalidStartIndexException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "StartIndex cannot be less than 0" This message takes into
			/// account the current system culture.
			/// </summary>
			public PagedData_InvalidStartIndexException()
				: base(GetResourceString("PagedData_InvalidStartIndexException")) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_InvalidStartIndexException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public PagedData_InvalidStartIndexException(Exception innerException)
				: base(GetResourceString("PagedData_InvalidStartIndexException"), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the PagedData_InvalidStartIndexException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public PagedData_InvalidStartIndexException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by <see cref="PagedData(long, int, long, System.Collections.IEnumerable)"/> when the totalRecords parameter has an invalid value.
		///	</summary>
		[Serializable]
		public class PagedData_InvalidTotalRecordsException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_InvalidTotalRecordsException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "TotalRecords cannot be less than 0" This message takes into
			/// account the current system culture.
			/// </summary>
			public PagedData_InvalidTotalRecordsException()
				: base(GetResourceString("PagedData_InvalidTotalRecordsException")) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_InvalidTotalRecordsException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public PagedData_InvalidTotalRecordsException(Exception innerException)
				: base(GetResourceString("PagedData_InvalidTotalRecordsException"), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the PagedData_InvalidTotalRecordsException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public PagedData_InvalidTotalRecordsException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

		///	<summary>
		///	Thrown by <see cref="PagedData(long, int, long, System.Collections.IEnumerable)"/> when the startIndex parameter is greater than the totalRecords parameter.
		///	</summary>
		[Serializable]
		public class PagedData_StartIndexIsGreaterThanTotalRecordsException : global::Common.DataAccess.LocalizedException {

#pragma warning disable 1573
			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_StartIndexIsGreaterThanTotalRecordsException"/> class, setting
			/// the <see cref="System.Exception.Message"/> property of the new instance to a system-supplied message
			/// that describes the error, such as "StartIndex cannot be greater than or equal to TotalRecords" This message takes into
			/// account the current system culture.
			/// </summary>
			public PagedData_StartIndexIsGreaterThanTotalRecordsException()
				: base(GetResourceString("PagedData_StartIndexIsGreaterThanTotalRecordsException")) {
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="PagedData_StartIndexIsGreaterThanTotalRecordsException"/> class with a
			/// reference to the inner exception that is the cause of this exception.
			/// </summary>
			/// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> if no inner exception is specified.</param>
			public PagedData_StartIndexIsGreaterThanTotalRecordsException(Exception innerException)
				: base(GetResourceString("PagedData_StartIndexIsGreaterThanTotalRecordsException"), innerException) {
			}

			/// <summary>
			/// Initializes a new instance of the PagedData_StartIndexIsGreaterThanTotalRecordsException class with serialized data.
			/// </summary>
			/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
			/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
			public PagedData_StartIndexIsGreaterThanTotalRecordsException(SerializationInfo info, StreamingContext context)
				: base(info, context) {
			}
#pragma warning restore 1573

		}

	}

}
