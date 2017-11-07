using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Base.Tests {

	/// <summary>
	/// This base-class provides a simple mechanism to consolidate typical testing functionality.
	/// </summary>
	/// <remarks>
	/// Each class which is derived from this class will have all the benefits of this class.
	/// These benefits include helper methods which allow repetitive and timed testing.
	/// </remarks>
	public class TestBaseClass {

		/// <summary>
		/// Provides access to the <see cref="TestContext"/> (which is useful for writing
		/// information to the Test Explorer panel).
		/// </summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// A Delegate which represents the code that is executed during a repeatable test.
		/// </summary>
		/// <param name="execution">The current execution.</param>
		public delegate void WhatToExecuteDelegate(int execution);

		/// <summary>
		/// A Delegate which represents the code which determines when a repeatable test should
		/// write to the Test Explorer panel.
		/// </summary>
		/// <param name="execution">The current execution.</param>
		/// <returns>The execution to log.</returns>
		public delegate int WhenToLogDelegate(int execution);

		/// <summary>
		/// This method is a simple mechanism to execute a test multiple times.
		/// </summary>
		/// <param name="del">The test.</param>
		/// <param name="totalExecutions">The number of executions.</param>
		/// <param name="nextMilestone">
		/// A delegate to execute to determine the next execution at which to write a log to the
		/// Test Explorer panel. For example,
		/// <code>
		/// delegate(int execution) {
		///     switch (execution) {
		///			case 1:
		///             // After the first execution, break after the twenty-fifth.
		///             return 25;
		///         case 25:
		///             // After the twenty-fifth execution, break after the fiftieth.
		///             return 50;
		///         default:
		///             // Otherwise, don't break anymore.
		///             return 0;
		///     }
		/// }
		/// </code>
		/// </param>
		/// <remarks>
		/// The first milestone is always after the first execution.
		/// </remarks>
		public void RepeatableTest(WhatToExecuteDelegate del, int totalExecutions, WhenToLogDelegate nextMilestone) {
			var milestone = 1;

			var sw = new Stopwatch();
			sw.Start();

			for (var execution = 0; execution < totalExecutions; execution++) {
				del(execution);

				if (execution == milestone) {
					sw.Stop();
					TestContext.WriteLine(string.Concat(execution, " executions takes ", sw.Elapsed));
					milestone = nextMilestone(execution);
					sw.Restart();
				}
			}

			sw.Stop();
			TestContext.WriteLine(string.Concat(totalExecutions, " executions takes ", sw.Elapsed));
		}

		/// <summary>
		/// This method is a simple mechanism to execute a test multiple times.
		/// </summary>
		/// <param name="del">The test.</param>
		/// <param name="totalExecutions">The number of executions.</param>
		/// <remarks>
		/// Timing information is written to the Test Explorer panel on every execution which is an
		/// order of 10. Which means, information is written on executions 1, 10, 100, etc.
		/// </remarks>
		public void RepeatableTest(WhatToExecuteDelegate del, int totalExecutions) {
			RepeatableTest(del, totalExecutions, delegate (int execution) {
				return execution * 10;
			});
		}

		/// <summary>
		/// This method writes timing information to the Test Explorer panel.
		/// </summary>
		/// <param name="del">The test.</param>
		public void TimedTest(WhatToExecuteDelegate del) {
			RepeatableTest(del, 1);
		}

	}

}
