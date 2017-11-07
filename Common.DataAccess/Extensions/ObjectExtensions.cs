using System;
using System.Reflection;

namespace Common.DataAccess {

	// documentation for this class is stored in "TypeExtensions.cs"
	public static partial class Extensions {

		/// <summary>
		/// Allows the developer to map property values from any object to the current object.
		/// </summary>
		/// <remarks>
		/// To perform the mapping, reflection is used to:
		///	<list type="number">
		///		<item><description>
		///			locate the generic "GetMap" method on the <see cref="Common.DataAccess.Mapper"/>
		///			class
		///		</description></item>
		///		<item><description>
		///			create a delegate of the "GetMap" method using the specified types (the types
		///			of <paramref name="to"/> and <paramref name="from"/>)
		///		</description></item>
		///		<item><description>
		///			execute the delegate of the "GetMap" method (which returns a mapping delegate)
		///		</description></item>
		///		<item><description>
		///			execute the mapping delegate
		///		</description></item>
		///	</list>
		/// If you can supply type-parameters at compile-time, your code will perform faster by
		/// using the following code:
		/// <code language="cs">
		/// Mapper.GetMap&lt;TIncoming, TOutgoing&gt;()(incoming, outgoing);
		/// </code>
		/// </remarks>
		/// <param name="to">the object that is receiving the values</param>
		/// <param name="from">the object that is supplying the values</param>
		/// <param name="checkMissingColumns">
		/// If <see langword="true"/>, a property which is found in the <paramref name="from"/> object,
		/// but is missing from the <paramref name="to"/> object, will throw an exception. Otherwise,
		/// missing columns are ignored.
		/// </param>
		public static void SyncFrom(this object to, object from, bool checkMissingColumns = true) {
			Type tIncoming = from.GetType();
			Type tOutgoing = to.GetType();
			MethodInfo method = typeof(Mapper).GetMethod("GetMap");
			MethodInfo generic = method.MakeGenericMethod(tIncoming, tOutgoing);
			var lambda = generic.Invoke(null, new object[] { checkMissingColumns });
			method = lambda.GetType().GetMethod("Invoke");
			method.Invoke(lambda, new[] { from, to });
		}

	}

}
