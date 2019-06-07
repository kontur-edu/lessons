using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using uLearn.NUnitTestRunning.TestsToRun;
using Ulearn.Core.NUnitTestRunning;

namespace uLearn.NUnitTestRunning
{
	public class NUnitTestRunner_Should
	{
		private NUnitTestRunner.TestListener listener;

		[SetUp]
		public void SetUp()
		{
			listener = new NUnitTestRunner.TestListener();
		}

		[Test]
		public void Run_SpecifiedTests()
		{
			var testsTuRun = typeof(ThreePassingTests).FullName;
			const int expectedCount = 3;

			NUnitTestRunner.RunAllTests(listener, Assembly.GetExecutingAssembly(), testsTuRun);
			var finishedTestsCount = listener.GetNumberOfFinishedTests();

			Assert.AreEqual(expectedCount, finishedTestsCount);
		}

		[Test]
		public void Stop_On_FirstTestFailure()
		{
			var testsTuRun = typeof(ThreeTestsWithSecondFailing).FullName;
			const int expectedCount = 2;

			NUnitTestRunner.RunAllTests(listener, Assembly.GetExecutingAssembly(), testsTuRun);
			var finishedTestsCount = listener.GetNumberOfFinishedTests();

			Assert.AreEqual(expectedCount, finishedTestsCount);
		}

		[Test]
		public void Run_Tests_InSpecifiedOrder()
		{
			string[] testsTuRun = { typeof(ThreeTestsWithSecondFailing).FullName, typeof(ThreePassingTests).FullName };
			const int expectedCount = 2;

			NUnitTestRunner.RunAllTests(listener, Assembly.GetExecutingAssembly(), testsTuRun);
			var finishedTestsCount = listener.GetNumberOfFinishedTests();

			Assert.AreEqual(expectedCount, finishedTestsCount);
		}

		[Test]
		[Repeat(2)]
		public void Support_SetUp_Feature()
		{
			ThreeTestsWithSetUp.Counter = 0;
			var expectedCounterValue = 3;

			NUnitTestRunner.RunAllTests(new NUnitTestRunner.TestListener(), Assembly.GetExecutingAssembly(), typeof(ThreeTestsWithSetUp).FullName);

			Assert.AreEqual(expectedCounterValue, ThreeTestsWithSetUp.Counter);
		}

		[Test]
		[Repeat(2)]
		public void Support_OneTimeSetUp_Feature()
		{
			TwoTestWithOneTimeSetup.Counter = 0;
			var expectedCounterValue = 1;

			NUnitTestRunner.RunAllTests(new NUnitTestRunner.TestListener(), Assembly.GetExecutingAssembly(), typeof(TwoTestWithOneTimeSetup).FullName);

			Assert.AreEqual(expectedCounterValue, TwoTestWithOneTimeSetup.Counter);
		}

		[Test]
		[Repeat(2)]
		public void Support_TearDown_Feature()
		{
			ThreeTestsWithTearDown.Counter = 0;
			var expectedCounterValue = 3;

			NUnitTestRunner.RunAllTests(new NUnitTestRunner.TestListener(), Assembly.GetExecutingAssembly(), typeof(ThreeTestsWithTearDown).FullName);

			Assert.AreEqual(expectedCounterValue, ThreeTestsWithTearDown.Counter);
		}

		[Test]
		[Repeat(2)]
		public void Support_TestCase_Feature()
		{
			var testsTuRun = typeof(FiveTestCases).FullName;
			const int testCount = 5;

			NUnitTestRunner.RunAllTests(listener, Assembly.GetExecutingAssembly(), testsTuRun);
			var finishedTestsCount = listener.GetNumberOfFinishedTests();

			Assert.AreEqual(testCount, finishedTestsCount);
		}

		[Test]
		[Repeat(2)]
		public void Support_Repeat_Feature()
		{
			TenRepeatTest.Counter = 0;
			var testsTuRun = typeof(TenRepeatTest).FullName;
			const int expectedCounterValue = 10;

			NUnitTestRunner.RunAllTests(listener, Assembly.GetExecutingAssembly(), testsTuRun);

			Assert.AreEqual(expectedCounterValue, TenRepeatTest.Counter);
		}

		[Test]
		public void Support_Order_Feature()
		{
			var testsToRun = typeof(ThreeOrderedTests).FullName;

			NUnitTestRunner.RunAllTests(listener, Assembly.GetExecutingAssembly(), testsToRun);

			Assert.AreEqual("012", ThreeOrderedTests.Order);
		}

		[Test]
		public void ReportOn_NonexistentTestClasses()
		{
			var expected = "Error in checking system: test class Nonexistent does not exist.";

			var message = Assert.Throws<ArgumentException>(
				() => NUnitTestRunner.ReportOnNonexistentTestClasses(Assembly.GetExecutingAssembly(), "Nonexistent"));
			Assert.AreEqual(expected, message.Message);
		}
	}

	public static class TestListenerExtentions
	{
		public static int GetNumberOfFinishedTests(this NUnitTestRunner.TestListener listener)
		{
			return listener.Results.Count(x => !x.HasChildren);
		}
	}
}