using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.DataAccess.Tests {

	public partial class MapperTests : Base.Tests.TestBaseClass {

		private const int MILLION = 1000000;

		#region Bogus classes
		private class FlatObject {
			public string Name__FirstName { get; set; }
			public string Name__MiddleName { get; set; }
			public string Name__LastName { get; set; }
			public string Name__Stuff__Value { get; set; }
			public long Age { get; set; }
		}

		private enum Ages {
			Young = 1,
			Old = 2
		}

		private class DeepObject {
			public Name Name { get; set; }
			public Ages? Age { get; set; }
		}

		private class Name {
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string MiddleName { get; set; }
			public Stuff Stuff { get; set; }
		}

		private class Stuff {
			public string Value { get; set; }
		}

		private class Integers {
			public byte aByte { get; set; }
			public short aShort { get; set; }
			public int aInt { get; set; }
			public long aLong { get; set; }
			public DayOfWeek aEnum { get; set; }
			public decimal aDecimal { get; set; }
		}
		#endregion

		[TestClass]
		public partial class SyncFrom : MapperTests {

			[TestMethod]
			public void aTonOfReads() {
				var flatO = new FlatObject {
					Name__FirstName = "Walter",
					Name__MiddleName = "Hartwell",
					Name__LastName = "White",
					Name__Stuff__Value = "whatever",
					Age = 2
				};
				var deepO = new DeepObject();

				RepeatableTest(delegate (int execution) {
					deepO.SyncFrom(flatO);
				}, 10 * MILLION);
			}

			[TestMethod]
			public void IntegerConvert() {
				var destination = new Integers();
				destination.SyncFrom(new {
					aByte = (long)3,
					aDecimal = (long)4,
					aEnum = (long)5,
					aInt = (long)6,
					aLong = (byte)7,
					aShort = (long)8
				});

				Assert.AreEqual(3, destination.aByte);
				Assert.AreEqual(4, destination.aDecimal);
				Assert.AreEqual(DayOfWeek.Friday, destination.aEnum);
				Assert.AreEqual(6, destination.aInt);
				Assert.AreEqual(7, destination.aLong);
				Assert.AreEqual(8, destination.aShort);
			}

			public class NullableIntegers {
				public byte? aByte { get; set; }
				public short? aShort { get; set; }
				public int? aInt { get; set; }
				public long? aLong { get; set; }
				public DayOfWeek? aEnum { get; set; }
				public decimal? aDecimal { get; set; }
			}

			[TestMethod]
			public void NullablesConvert() {
				var destination = new NullableIntegers();
				destination.SyncFrom(new {
					aByte = (long)3,
					aDecimal = (long)4,
					aEnum = (long)5,
					aInt = (long)6,
					aLong = (byte)7,
					aShort = (long)8
				});

				Assert.AreEqual((byte)3, destination.aByte);
				Assert.AreEqual((decimal)4, destination.aDecimal);
				Assert.AreEqual(DayOfWeek.Friday, destination.aEnum);
				Assert.AreEqual((int)6, destination.aInt);
				Assert.AreEqual((long)7, destination.aLong);
				Assert.AreEqual((short)8, destination.aShort);
			}

			[TestMethod]
			public void Nullables() {
				var destination = new NullableIntegers();
				destination.SyncFrom(new {
					aByte = (long?)null,
					aDecimal = (long?)null,
					aEnum = (long?)null,
					aInt = (long?)null,
					aLong = (byte?)null,
					aShort = (long?)null
				});

				Assert.IsNull(destination.aByte);
				Assert.IsNull(destination.aDecimal);
				Assert.IsNull(destination.aEnum);
				Assert.IsNull(destination.aInt);
				Assert.IsNull(destination.aLong);
				Assert.IsNull(destination.aShort);
			}

		}

		[TestClass]
		public partial class Remodel : MapperTests {

			[TestMethod]
			public void automaticDefiningInputAndOutput() {
				var incoming = new List<FlatObject>();
				for (var x = 0; x < 10 * MILLION; x++) {
					incoming.Add(new FlatObject {
						Name__FirstName = "Walter",
						Name__MiddleName = "Hartwell",
						Name__LastName = "White",
						Name__Stuff__Value = "whatever",
						Age = 2
					});
				}
				TimedTest(delegate (int execution) {
					var deepO = Mapper.Remodel<FlatObject, DeepObject>(incoming);
				});
			}

			[TestMethod]
			public void automatic() {
				var incoming = new List<FlatObject>();
				for (var x = 0; x < 10 * MILLION; x++) {
					incoming.Add(new FlatObject {
						Name__FirstName = "Walter",
						Name__MiddleName = "Hartwell",
						Name__LastName = "White",
						Name__Stuff__Value = "whatever",
						Age = 2
					});
				}
				TimedTest(delegate (int execution) {
					var deepO = Mapper.Remodel<DeepObject>(incoming);
				});
			}

			[TestMethod]
			public void manual() {
				var incoming = new List<FlatObject>();
				for (var x = 0; x < 10 * MILLION; x++) {
					incoming.Add(new FlatObject {
						Name__FirstName = "Walter",
						Name__MiddleName = "Hartwell",
						Name__LastName = "White",
						Name__Stuff__Value = "whatever",
						Age = 2
					});
				}
				Func<FlatObject, DeepObject> myDelegate = delegate (FlatObject flatO) {
					return new DeepObject {
						Name = new Name {
							FirstName = flatO.Name__FirstName,
							MiddleName = flatO.Name__MiddleName,
							LastName = flatO.Name__LastName,
							Stuff = new Stuff {
								Value = flatO.Name__Stuff__Value
							}
						},
						Age = (Ages)flatO.Age
					};
				};
				TimedTest(delegate (int execution) {
					var deepO = Mapper.Remodel(incoming, myDelegate);
				});
			}

		}

		[TestClass]
		public partial class SpecificProcTests : MapperTests {

			private class Source {
				public NestedSource Nested { get; set; }
			}

			private class NestedSource {
				public string Value { get; set; }
			}

			private class Destination {
				public NestedDestination Nested { get; set; }
			}

			private class NestedDestination {
				public string Value { get; set; }
			}

			[TestMethod]
			public void DataAccess_to_Contract() {
				var source = new Source {
					Nested = new NestedSource {
						Value = "stuff"
					}
				};
				var destination = Mapper.Remodel<Destination>(new[] { source }).First();

				Assert.IsNotNull(destination);
			}

			[TestMethod]
			[ExpectedException(typeof(Common.DataAccess.Exceptions.Mapper_CannotConvertException))]
			public void bool_to_string() {
				var bt = new boolThing();
				stringThing st;

				TimedTest(delegate (int execution) {
					st = Mapper.Remodel<boolThing, stringThing>(bt);
				});
			}

			public class boolThing {
				public bool thing { get; set; }
			}

			public class stringThing {
				public string thing { get; set; }
			}

		}

	}

}
