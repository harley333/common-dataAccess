using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.DataAccess {
	
	/// <summary>
	/// This class contains helper methods for mapping properties from one type to another.
	/// </summary>
	/// <remarks>
	/// Remodeling data can be a very costly operation (performance-wise). A best-case will always
	/// be represented by a mapping function which is hand-written by a developer. However, the
	/// upfront development cost and on-going maintenance of mapping functions can become daunting
	/// (investment-wise) as well.
	/// <para>
	/// Spending resource investments on explicitly-defined mapping functions seems silly and a
	/// waste. After all, the vast majority of mapping functions are a simple matter of naming
	/// conventions. For example, "I want to take the value from the 'Name' property from this
	/// object and put it in the 'Name' property of this other object."
	/// </para>
	/// <para>
	/// When an explicitly-defined mapping function is a waste of time for a developer, the
	/// <see cref="Mapper"/> class will instantiate an instance of the
	/// <see cref="Mapper{TFrom, TTo}"/> class.
	/// </para>
	/// <para>
	/// Any delegate created via these methods is cached for future use.
	/// </para>
	/// </remarks>
	public class Mapper {

		#region Using Explicit Mapper (fastest - the developer supplies a custom-made mapper)

		/// <summary>
		/// Maps the properties of one type to another using an explicitly-defined mapper.
		/// </summary>
		/// <typeparam name="TIncoming">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <typeparamref name="TIncoming"/></param>
		/// <param name="mapper">An explicitly-defined mapper.</param>
		/// <returns>A <typeparamref name="TOutgoing"/></returns>
		public static TOutgoing Remodel<TIncoming, TOutgoing>(TIncoming source, Func<TIncoming, TOutgoing> mapper)
			where TIncoming : class
			where TOutgoing : class {

			return Remodel(new[] { source }, mapper).First();
		}

		/// <summary>
		/// Maps the properties of one type to another using an explicitly-defined mapper.
		/// </summary>
		/// <typeparam name="TIncoming">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <see cref="IEnumerable"/>&lt;<typeparamref name="TIncoming"/>&gt;</param>
		/// <param name="mapper">An explicitly-defined mapper.</param>
		/// <returns>An <see cref="IEnumerable"/>&lt;<typeparamref name="TOutgoing"/>&gt;</returns>
		public static IEnumerable<TOutgoing> Remodel<TIncoming, TOutgoing>(IEnumerable<TIncoming> source, Func<TIncoming, TOutgoing> mapper)
			where TIncoming : class
			where TOutgoing : class {

			var list = new List<TOutgoing>();
			if (source != null) {
				foreach (var item in source) {
					list.Add(mapper(item));
				}
			}
			return list;
		}

		/// <summary>
		/// Maps the properties of one type to another using an explicitly-defined mapper.
		/// </summary>
		/// <typeparam name="TIncoming">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <see cref="PagedData"/>&lt;<typeparamref name="TIncoming"/>&gt;</param>
		/// <param name="mapper">An explicitly-defined mapper.</param>
		/// <returns>A <see cref="PagedData"/>&lt;<typeparamref name="TOutgoing"/>&gt;</returns>
		public static PagedData<TOutgoing> Remodel<TIncoming, TOutgoing>(PagedData<TIncoming> source, Func<TIncoming, TOutgoing> mapper)
			where TIncoming : class
			where TOutgoing : class {

			var list = Remodel(source.Data, mapper);
			return new PagedData<TOutgoing>(source.TotalRecords, source.PageSize, source.StartIndex, list);
		}

		#endregion

		#region Using Default Mapper (a little slower - a Mapper is compiled by reflective inspection)

		/// <summary>
		/// Maps the properties of a known type to another by automatically building a mapper.
		/// </summary>
		/// <remarks>
		/// The mapper will be an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// </remarks>
		/// <typeparam name="TIncoming">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <typeparamref name="TIncoming"/></param>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>A <typeparamref name="TOutgoing"/></returns>
		public static TOutgoing Remodel<TIncoming, TOutgoing>(TIncoming source, bool checkMissingColumns = true)
			where TIncoming : class
			where TOutgoing : class {

			return Remodel<TIncoming, TOutgoing>(new[] { source }, checkMissingColumns).First();
		}

		/// <summary>
		/// Maps the properties of a known type to another by automatically building a mapper.
		/// </summary>
		/// <remarks>
		/// The mapper will be an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// </remarks>
		/// <typeparam name="TIncoming">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <see cref="IEnumerable"/>&lt;<typeparamref name="TIncoming"/>&gt;</param>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>A <see cref="IEnumerable"/>&lt;<typeparamref name="TOutgoing"/>&gt;</returns>
		public static IEnumerable<TOutgoing> Remodel<TIncoming, TOutgoing>(IEnumerable<TIncoming> source, bool checkMissingColumns = true)
			where TIncoming : class
			where TOutgoing : class {

			return Remodel(source, GetMapDelegate<TIncoming, TOutgoing>(checkMissingColumns));
		}

		/// <summary>
		/// Maps the properties of a known type to another by automatically building a mapper.
		/// </summary>
		/// <remarks>
		/// The mapper will be an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// </remarks>
		/// <typeparam name="TIncoming">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <see cref="PagedData"/>&lt;<typeparamref name="TIncoming"/>&gt;</param>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>A <see cref="PagedData"/>&lt;<typeparamref name="TOutgoing"/>&gt;</returns>
		public static PagedData<TOutgoing> Remodel<TIncoming, TOutgoing>(PagedData<TIncoming> source, bool checkMissingColumns = true)
			where TIncoming : class
			where TOutgoing : class {

			var list = Remodel<TIncoming, TOutgoing>(source.Data, checkMissingColumns);
			return new PagedData<TOutgoing>(source.TotalRecords, source.PageSize, source.StartIndex, list);
		}

		#endregion

		#region Using Default Mapper - Generic Destination with Unknown Source (slowest - reflection is used to find the "GetMapDelegate" method and execute it, which then compiles a Mapper by reflective inspection)

		/// <summary>
		/// Maps the properties of an unknown type to another by automatically building a mapper.
		/// </summary>
		/// <remarks>
		/// The mapper will be an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// <para>
		/// The <paramref name="source"/> parameter is assumed to be an instance of
		/// <see cref="IEnumerable{T}"/>. The generic argument "T" is discovered from the source
		/// collection and used to create an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// </para>
		/// </remarks>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <see cref="IEnumerable"/></param>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>A <see cref="IEnumerable"/>&lt;<typeparamref name="TOutgoing"/>&gt;</returns>
		public static IEnumerable<TOutgoing> Remodel<TOutgoing>(IEnumerable source, bool checkMissingColumns = true)
			where TOutgoing : class {

			Type tIncoming = null;
			foreach (Type i in source.GetType().GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>))) {
					tIncoming = i.GetGenericArguments()[0];
					break;
				}
			}
			Type tOutgoing = typeof(TOutgoing);

			Type me = typeof(Mapper);

			MethodInfo method = me.GetMethod("GetMapDelegate");
			MethodInfo generic = method.MakeGenericMethod(tIncoming, tOutgoing);
			var map = generic.Invoke(null, new object[] { checkMissingColumns });

			var sourceType = typeof(IEnumerable<>).MakeGenericType(tIncoming);
			var delegateType = typeof(Func<,>).MakeGenericType(tIncoming, tOutgoing);

			method = me.GetGenericMethod("Remodel", new[] { sourceType, delegateType });
			generic = method.MakeGenericMethod(tIncoming, tOutgoing);
			return (IEnumerable<TOutgoing>)generic.Invoke(null, new[] { source, map });
		}

		/// <summary>
		/// Maps the properties of an unknown type to another by automatically building a mapper.
		/// </summary>
		/// <remarks>
		/// The mapper will be an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// <para>
		/// The <paramref name="source"/> parameter is assumed to be an instance of
		/// <see cref="PagedData{TEntity}"/>. The generic argument "TEntity" is discovered from the
		/// source collection and used to create an instance of <see cref="Mapper{TFrom, TTo}"/>.
		/// </para>
		/// </remarks>
		/// <typeparam name="TOutgoing">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="source">An instance of <see cref="PagedData"/></param>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>A <see cref="PagedData"/>&lt;<typeparamref name="TOutgoing"/>&gt;</returns>
		public static PagedData<TOutgoing> Remodel<TOutgoing>(PagedData source, bool checkMissingColumns = true)
			where TOutgoing : class {

			var list = Remodel<TOutgoing>(source.Data, checkMissingColumns);
			return new PagedData<TOutgoing>(source.TotalRecords, source.PageSize, source.StartIndex, list);
		}

		#endregion

		#region Map-cache Helper Methods

		// here's a collection of delegate functions
		//	each delegate is responsible for performing a specific map operation (FROM one type of object, TO another type of object)
		//  the key of the collection is generated by the GetMapName method
		private static SortedList<string, object> _Mappers = new SortedList<string, object>();

		// here's a collection of delegate functions which are currently being built
		//  keeping this collection allows us to build recursive mappers
		private static List<string> _MappersBeingBuilt = new List<string>();

		/// <summary>
		/// Returns a delegate which maps the properties from one object to another and returns the
		/// destination object.
		/// </summary>
		/// <typeparam name="TFrom">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TTo">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>A <see cref="Func{TFrom, TTo}"/></returns>
		public static Func<TFrom, TTo> GetMapDelegate<TFrom, TTo>(bool checkMissingColumns = true)
			where TFrom : class
			where TTo : class {

			var map = GetMap<TFrom, TTo>(checkMissingColumns);
			return delegate(TFrom source) {
				var destination = Activator.CreateInstance<TTo>();
				map(source, destination);
				return destination;
			};
		}

		/// <summary>
		/// Returns a delegate which maps the properties from one supplied object to another
		/// supplied object.
		/// </summary>
		/// <typeparam name="TFrom">
		/// The data-type that is being mapped from.
		/// </typeparam>
		/// <typeparam name="TTo">
		/// The data-type that is being mapped to.
		/// </typeparam>
		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		/// <returns>An <see cref="Action{TFrom, TTo}"/></returns>
		public static Action<TFrom, TTo> GetMap<TFrom, TTo>(bool checkMissingColumns = true)
			where TFrom : class
			where TTo : class {

			var tFrom = typeof(TFrom);
			var tTo = typeof(TTo);

			object temp = null;
			Action<TFrom, TTo> mapper = null;
			var mapName = GetMapName(tFrom, tTo, checkMissingColumns);
			if (!_Mappers.TryGetValue(mapName, out temp)) {
				if (_MappersBeingBuilt.Contains(mapName)) {
					// If the requested map is currently being built, we must be building a
					//  recursive map. We cannot return the map, because it's still being built.
					//  Instead, we'll return a delegate which can request the map after it has
					//  been built.
					return (x, y) => GetMap<TFrom, TTo>(checkMissingColumns);
				} else {
					_MappersBeingBuilt.Add(mapName);
					mapper = new Mapper<TFrom, TTo>(checkMissingColumns).GetMap();
					_Mappers.Add(mapName, mapper);
					_MappersBeingBuilt.Remove(mapName);
				}
			} else {
				mapper = (Action<TFrom, TTo>)temp;
			}
			return mapper;
		}

		private static string GetMapName(Type tFrom, Type tTo, bool checkMissingColumns) {
			return string.Concat(checkMissingColumns ? "STRICT_" : "", tFrom.FullName, "_|_", tTo.FullName);
		}

		#endregion

	}

}
