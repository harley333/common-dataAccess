using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.DataAccess {

	/// <summary>
	/// This class has the ability to create a mapping method which maps properties from one type
	/// to another.
	/// </summary>
	/// <typeparam name="TFrom">
	/// The data-type that is being mapped from.
	/// </typeparam>
	/// <typeparam name="TTo">
	/// The data-type that is being mapped to.
	/// </typeparam>
	public class Mapper<TFrom, TTo>
		where TFrom : class
		where TTo : class {

		static Mapper() {
			_PropertyException = typeof(Exceptions.Mapper_PropertyException).GetConstructor(new[] {
				typeof(Exception), // inner-exception
				typeof(string),    // fromType
				typeof(string),    // fromField
				typeof(string),    // toType
				typeof(string)     // toField
			});
			_PropertyCache = new Dictionary<string, IEnumerable<PropertyInfo>>();
		}
		private static ConstructorInfo _PropertyException;
		private static Dictionary<string, IEnumerable<PropertyInfo>> _PropertyCache;
		private static Expression cNull = Expression.Constant(null);
		private const string NAME_SEPARATOR = "__";
		private bool _CheckMissingColumns;

		/// <param name="checkMissingColumns">
		/// When properties are in the source object, but absent in the destination object, this
		/// value determines if an exception is thrown during the mapping operation.
		/// </param>
		public Mapper(bool checkMissingColumns) {
			_Variables = new List<ParameterExpression>();
			_CheckMissingColumns = checkMissingColumns;
		}

		// this will hold our list of variables
		private List<ParameterExpression> _Variables;

		/// <summary>
		/// Returns a method which performs a map operation of properties from one type to another.
		/// </summary>
		/// <returns>An <see cref="Action{TFrom, TTo}"/></returns>
		public Action<TFrom, TTo> GetMap() {
			var tFrom = typeof(TFrom);
			var tTo = typeof(TTo);

			// our target expression will have:
			//  - a single parameter (of type TFrom)
			//  - one or more variables (of type TTo and any necessary, non-value-type properties)
			//  - a list of statements (the last statement will be our return variable)


			// If an exception occurs during an assignment operation, we'll use the following
			// variable to hold the exception.
			var pException = Expression.Parameter(typeof(Exception), "ex");

			// The following PropertyInfo ("DateTime.TimeOfDay") is to be used for converting a
			//  DateTime to a TimeSpan.
			var piTimeOfDay = typeof(DateTime).GetProperty("TimeOfDay");

			// The following PropertyInfos ("Nullable<DateTime>.Value",
			//  "Nullable<DateTime>.HasValue", and "DateTime.MinValue") are to be used for
			//  converting a DateTime to a TimeSpan.
			var piValue = typeof(DateTime?).GetProperty("Value");
			var piHasValue = typeof(DateTime?).GetProperty("HasValue");
			var cMinValue = Expression.Property(Expression.Field(null, typeof(DateTime).GetField("MinValue")), piTimeOfDay);

			// here's our single parameter
			var pFrom = Expression.Parameter(tFrom, "from");
			// here's our first variable (this is also our return variable and will be added as the
			// last statement)
			var pTo = Expression.Parameter(tTo, "to");

			// here's our list of statements
			var statements = new List<Expression>();

			// get a list of ALL the values that we're trying to pull FROM
			var fromProps = GetNonIndexedPublicProps(tFrom);

			// now, let's loop through the values that we're trying to pull FROM
			foreach (var fromProp in fromProps) {

				// During an un-flattening map operation (for example, pulling a flattened
				// hierarchy out of a database and pushing it into a hierarchical object model),
				// the property names will not match. For example, when pulling from
				// "Name__LastName," our target object will have a property called "Name" which has
				// a sub-property called "LastName."

				// To properly perform this type of un-flattening operation, we'll call the
				// GetContainer method (which returns a reference to the variable which actually
				// contains our target property). In our example, the GetContainer method will return
				// the variable of the "Name" object.

				var containerInfo = GetContainer(pTo, fromProp.Name);
				var vParent = containerInfo.Variable;
				var adjustedPropName = containerInfo.AdjustedPropName;
				var parentProps = containerInfo.Properties;

				// let's make sure the property actually exists on the target object
				var toProp = parentProps.FirstOrDefault(x => x.Name == adjustedPropName);
				if (toProp == null) {
					// The "RecordCount" column is used by our paging conventions. When mapping,
					// this column is safe to ignore.
					if (!_CheckMissingColumns && adjustedPropName != "RecordCount") {
						throw new Exceptions.Mapper_CannotFindPropertyException(adjustedPropName, vParent.Type.FullName);
					}
					continue;
				} else {
					// if the property DOES exists on the target object, we want to assign its value

					if (!toProp.CanWrite) {
						// If the destination is not writable, we'll skip it.
						continue;
					}

					var fromPropType = fromProp.PropertyType;
					var toPropType = toProp.PropertyType;

					var fromIsList = fromPropType != typeof(string) && fromPropType.GetInterface("IEnumerable`1") != null;
					var toIsList = toPropType != typeof(string) && toPropType.GetInterface("IEnumerable`1") != null;

					var fromIsValue = fromPropType.IsValueType || fromPropType == typeof(string);
					var toIsValue = toPropType.IsValueType || toPropType == typeof(string);

					var assignStatements = new List<Expression>();

					// first, let's get a reference to the source property
					Expression fromValue = Expression.Property(pFrom, fromProp.Name);
					Expression toValue = Expression.Property(vParent, adjustedPropName);

					var assignmentIsSimple = fromPropType == toPropType && ((fromIsValue && toIsValue) || (fromIsList && toIsList));
					try {
						if ((fromIsValue && toIsValue) || (fromIsList && toIsList)) {
							// Here's a basic assignment statement. This assignment is intended for
							//  value-types. At the moment, we're also reference-assigning
							//  collection-types (because I'm too lazy to loop through the
							//  collection and perform a deep-copy on each element).

							// "Expression.Convert" will not successfully convert a System.DateTime to a System.TimeSpan.
							//  However, DateTime to TimeSpan is common enough that we will supply some custom logic for
							//  it.
							if ((fromPropType == typeof(DateTime) || fromPropType == typeof(DateTime?))
								&& (toPropType == typeof(TimeSpan) || toPropType == typeof(TimeSpan?))) {

								Expression defaultValue = cMinValue;
								if (toPropType == typeof(TimeSpan?)) {
									defaultValue = cNull;
								}

								if (fromPropType == typeof(DateTime?)) {
									assignStatements.Add(Expression.IfThenElse(
										Expression.Property(fromValue, piHasValue),
										Expression.Assign(
											toValue,
											Expression.Convert(Expression.Property(Expression.Property(fromValue, piValue), piTimeOfDay), toPropType)
										),
										Expression.Assign(
											toValue,
											Expression.Convert(defaultValue, toPropType)
										)
									));
								} else {
									assignStatements.Add(Expression.Assign(
										toValue,
										Expression.Convert(Expression.Property(fromValue, piTimeOfDay), toPropType)
									));
								}
							} else {
								// if this Expression.Convert call fails, we'll fall into the Catch block.
								assignStatements.Add(Expression.Assign(
									toValue,
									assignmentIsSimple ? fromValue : Expression.Convert(fromValue, toPropType)
								));
								if (containerInfo.EnsureStatements != null) {
									// If the current "from" property needs to be mapped to a nested property,
									// let's ensure the nested property's dependencies are created.  For
									// example, from "User__FirstName" to "User.FirstName" ("User" is an object
									// that cannot be null when assigning the "FirstName" property).
									assignStatements.InsertRange(0, containerInfo.EnsureStatements);

									assignmentIsSimple = false;
								}
							}
						} else {
							// If our values are both complex (and neither is a collection-type),
							//  we'll perform "deep" mapping. Deep mapping may require recursion.
							//  The Mapper.GetMap`2 method allows us to support recursion.
							assignStatements.Add(GetDeepAssignment(fromPropType, toPropType, fromValue, toValue));
						}
					} catch {

						// there's no way to reliably determine (as far as I can tell), if
						// Expression.Convert is supported for a given Type.  Therefore, the above
						// Try block will attempt to Convert the current value.  If the attempt
						// fails, this Catch block will check to see if the value is complex.  If
						// the value is complex, we will attempt to perform "deep" mapping.

						if (fromIsValue != toIsValue || fromIsValue) {
							// We cannot convert from/to a value type and a non-value type.
							// Also, if they're both value types and Convert did not work, we need
							// to throw the exception.
							throw new Exceptions.Mapper_CannotConvertException(
								tFrom.FullName,
								fromProp.Name,
								fromPropType.FullName,
								vParent.Type.FullName,
								adjustedPropName,
								toPropType.FullName
							);
						} else if (!fromIsValue) {
							// If our values are both complex, we'll perform "deep" mapping
							assignStatements.Add(GetDeepAssignment(fromPropType, toPropType, fromValue, toValue));
						}
					}

					Expression nullAssignment = Expression.Empty();
					if (toPropType.IsNullable()) {
						if (toPropType == fromPropType) {
							nullAssignment = Expression.Assign(toValue, fromValue);
						} else {
							nullAssignment = Expression.Assign(toValue, Expression.Convert(cNull, toPropType));
						}
						if (vParent != pTo) {
							nullAssignment = Expression.IfThen(
								Expression.NotEqual(vParent, cNull),
								nullAssignment
							);
						}
					}
					var theAssignment = (assignStatements.Count() == 1 ? assignStatements.First() : Expression.Block(assignStatements));
					if (!assignmentIsSimple && fromPropType.IsNullable()) {
						theAssignment = Expression.IfThenElse(
							Expression.NotEqual(fromValue, Expression.Convert(cNull, fromPropType)),
							theAssignment,
							nullAssignment
						);
					}

					statements.Add(
						// A Try-Catch structure has two expression blocks (a block for Try, and a
						// block for Catch). Both blocks must have the same result-type.
						//
						// In our Catch, we're merely re-throwing the exception with some useful
						// information. The result-type is "void."
						//
						// So, we'll force the Try block to have a result-type of "void" as well.
						Expression.TryCatch(
							Expression.Block(
								typeof(void),
								theAssignment
							),
							Expression.Catch(pException, Expression.Throw(
								Expression.New(_PropertyException, new Expression[] {
									pException,
									Expression.Constant(tFrom.FullName),
									Expression.Constant(fromProp.Name),
									Expression.Constant(vParent.Type.FullName),
									Expression.Constant(adjustedPropName)
								})
							))
						)
					);
				}
			}

			// finally, build a lambda expression which contains all of our hard-work
			var expression = Expression.Lambda<Action<TFrom, TTo>>(
				Expression.Block(
					_Variables,
					statements.ToArray()
				),
				new[] { pFrom, pTo }
			);

			// A compiled lambda expression is a delegate.
			//  So, compile our lambda and return it!
			return expression.Compile();

			// If you would like to see an textual representation of the complete expression, paste
			// the following into the Watch window while debugging.
			//     System.IO.File.WriteAllText(@"C:\" + Mapper.GetMapName(tFrom, tTo, _CheckMissingColumns).Replace("|", "_") + ".txt", expression.DebugView)
			// Unfortunately, the "DebugView" property is only available in Debug mode, and the
			// implementation that generates the text is marked "internal," so there's no easy way
			// to retrieve this output at runtime.
		}

		#region private implementation

		private class ContainerInfo {
			public ParameterExpression Variable;
			public IEnumerable<Expression> EnsureStatements;
			public IEnumerable<PropertyInfo> Properties;
			public string AdjustedPropName;
		}

		private BinaryExpression GetDeepAssignment(Type fromPropType, Type toPropType, Expression fromValue, Expression toValue) {
			// Use reflection to get a mapper-delegate based on the specific types that
			// we need to map.
			Type me = typeof(Mapper);
			MethodInfo method = me.GetMethod("GetMapDelegate");
			MethodInfo generic = method.MakeGenericMethod(fromPropType, toPropType);
			var map = generic.Invoke(null, new object[] { _CheckMissingColumns });
			var del = (Delegate)map;

			// Get the instance of the mapper class that the delegate belongs to.
			var mapperInstance = Expression.Constant(del.Target);

			return Expression.Assign(
				toValue,
				Expression.Call(mapperInstance, del.Method, fromValue) // execute the delegate!
			);
		}

		private IEnumerable<PropertyInfo> GetNonIndexedPublicProps(Type type) {
			IEnumerable<PropertyInfo> list = null;
			if (!_PropertyCache.TryGetValue(type.FullName, out list)) {
				list = type
					.GetMembers(BindingFlags.Instance | BindingFlags.Public)
					.Where(mi => mi.MemberType == MemberTypes.Property && ((PropertyInfo)mi).GetIndexParameters().Length == 0)
					.Cast<PropertyInfo>()
					.OrderBy(x => // this orderBy simply puts the property assignments in a predictable order
						GetNameParts(x.Name).Count().ToString().PadLeft(3, '0')
						+ x.Name
					);
				_PropertyCache.Add(type.FullName, list);
			}
			return list;
		}

		private List<PropertyInfo> GetPropertyTree(Type destination, IEnumerable<string> names) {
			var propTree = new List<PropertyInfo>();
			if (!names.Any()) {
				return propTree;
			}
			var propName = names.First();

			var props = GetNonIndexedPublicProps(destination);
			PropertyInfo prop = null;
			try {
				prop = props.Single(x => x.Name == propName);
			} catch (Exception ex) {
				if (!_CheckMissingColumns) {
					throw new Exceptions.Mapper_CannotFindPropertyException(ex, propName, destination.FullName);
				}
				return propTree;
			}

			propTree = GetPropertyTree(prop.PropertyType, names.Skip(1));
			propTree.Insert(0, prop);
			return propTree;
		}

		private IEnumerable<Expression> GetEnsureStatement(ParameterExpression vTo, Type destination, IEnumerable<string> names) {
			var propTree = GetPropertyTree(destination, names);
			var vParent = vTo;
			var statements = new List<Expression>();
			var variableName = "";
			foreach (PropertyInfo prop in propTree) {
				var tProp = prop.PropertyType;
				variableName += NAME_SEPARATOR + prop.Name;
				var vProp = _Variables.SingleOrDefault(x => x.Name == variableName);
				if (vProp == null) {
					vProp = Expression.Variable(tProp, variableName);
					_Variables.Add(vProp);
				}
				statements.AddRange(new Expression[] {
					Expression.IfThen(
						Expression.Equal(Expression.Property(vParent, prop.Name), cNull),
						Expression.Assign(Expression.Property(vParent, prop.Name), Expression.New(tProp))
					),
					Expression.Assign(vProp, Expression.Property(vParent, prop.Name))
				});
				vParent = vProp;
			}
			return statements;
		}

		private IEnumerable<string> GetNameParts(string fullName) {
			return fullName.Split(new string[] { NAME_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
		}

		private ContainerInfo GetContainer(ParameterExpression vTo, string fromPropName) {
			// the following block supports our naming convention
			var names = GetNameParts(fromPropName);
			if (names.Count() == 1) {
				return new ContainerInfo{
					Variable = vTo,
					AdjustedPropName = fromPropName,
					Properties = GetNonIndexedPublicProps(vTo.Type),
					EnsureStatements = null
				};
			}

			var containerName = fromPropName.Substring(0, fromPropName.LastIndexOf(NAME_SEPARATOR));
			var adjustedPropName = names.Last();
			var ensureStatements = GetEnsureStatement(vTo, vTo.Type, names.Take(names.Count() - 1));
			var vParent = _Variables.SingleOrDefault(x => x.Name == NAME_SEPARATOR + containerName);
			if (vParent == null) {
				throw new Exceptions.Mapper_CannotFindPropertyContainerException(containerName, vTo.Type.FullName);
			}
			var props = GetNonIndexedPublicProps(vParent.Type);
			return new ContainerInfo {
				Variable = vParent,
				AdjustedPropName = adjustedPropName,
				Properties = props,
				EnsureStatements = ensureStatements
			};
		}

		#endregion

	}

}
