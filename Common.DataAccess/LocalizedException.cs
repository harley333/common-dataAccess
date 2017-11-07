using System;
using System.Runtime.Serialization;

namespace Common.DataAccess {

	/// <summary>
	/// Provides a common base-class for all Localize-able exceptions.
	/// </summary>
	/// <remarks>
	/// Localize-able exceptions can be generated using the VS.CustomTools plug-in on RESX files.
	/// </remarks>
	[Serializable]
	public class LocalizedException : Exception {

		/// <param name="message">The message that describes the error</param>
		public LocalizedException(string message)
			: base(message) {
		}

		/// <param name="message">The message that describes the error</param>
		/// <param name="innerException">
		/// The exception that is the cause of the current exception, or a <see langword="null"/>
		/// reference (<see langword="Nothing"/> in Visual Basic) if no inner exception is
		/// specified.
		/// </param>
		public LocalizedException(string message, Exception innerException)
			: base(message, innerException) {
		}

		/// <param name="info">
		/// The <see cref="SerializationInfo"/> that holds the serialized object data about the
		/// exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="StreamingContext"/> that contains contextual information about the
		/// source or destination.
		/// </param>
		public LocalizedException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}

	}

}
