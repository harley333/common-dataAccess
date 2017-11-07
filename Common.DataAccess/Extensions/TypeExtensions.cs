using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.DataAccess {

	/// <summary>
	/// This class contains various extension methods.
	/// </summary>
	public static partial class Extensions {

		/// <summary>
		/// Determines if the current <see cref="Type"/> is nullable.
		/// </summary>
		/// <param name="type">The current <see cref="Type"/></param>
		/// <returns>
		/// <see langword="true"/>, if the current <see cref="Type"/> is nullable.
		/// <see langword="false"/>, otherwise.
		/// </returns>
		public static bool IsNullable(this Type type) {
			return type == typeof(string) || type.IsClass || (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
		}

		private class SimpleTypeComparer : IEqualityComparer<Type> {
			public bool Equals(Type x, Type y) {
				return x.Assembly == y.Assembly &&
					x.Namespace == y.Namespace &&
					x.Name == y.Name;
			}
			public int GetHashCode(Type obj) {
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Finds a method (based on its name and typed-parameters), and returns it.
		/// </summary>
		/// <param name="type">The current <see cref="Type"/></param>
		/// <param name="name">The name of the method</param>
		/// <param name="parameterTypes">A collection of <see cref="Type"/></param>
		/// <returns>The generic method, or <see langword="null"/> (if not found)</returns>
		public static MethodInfo GetGenericMethod(this Type type, string name, IEnumerable<Type> parameterTypes) {
			var methods = type.GetMethods();
			foreach (var method in methods.Where(m => m.Name == name)) {
				var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
				if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer())) {
					return method;
				}
			}
			return null;
		}

	}

}
