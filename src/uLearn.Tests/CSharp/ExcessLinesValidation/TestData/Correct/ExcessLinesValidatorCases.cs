using System;
using System.Collections.Generic;
using uLearn.CSharp.ExcessLinesValidation.TestData;

namespace uLearn.CSharp.ExcessLinesValidation.TestData.Correct
{
	using List = System.Collections.Generic;
	#region MyRegion
	public class BaseClass { }

	public class SomeClass
		: BaseClass
	{
	}
	#endregion

	public interface InterfaceWithWhere<out T>
		where T: IInterface1
	{
		T GetCommand();
	}
	
	public class SomeClass2<T>
		: BaseClass
		where T : IComparable<T>
	{
	}
	
	public class SomeClass2
	{
	}

	public class SomeClass3
	{
		private int field;

		public SomeClass3(int x)
		{
			
		}
		
		public SomeClass3()
			: this(0)
		{
			
		}

		public SomeClass3(Func<string, string> makeCaption, Func<string> beginList,
			Func<IEnumerable<double>, string> makeStatistics, int itemMaker, Func<string> endList)
		{
			var a = 1;
			field = a;
		}

		private int Property => 0;

		public int this[int index]
		{
			get => 0;
			set => field = value;
		}

		public void SomeMethod() { }

		public void SomeMethod1(int arg1,
								int arg2,
								int arg3)
		{
		}
		
		public void SomeMethod1<TData>(int arg1,
									   int arg2,
									   TData arg3)
			where TData : class
		{
		}

		public void SomeMethod2()
		{
		}

		public void SomeMethod3<TData>()
			where TData : class
		{
		}

		public int SomeMethod5()
		{
			SomeMethod();
			return 0;
		}

		public void SomeMethod6<TData1, TData2>()
			where TData1 : class
			where TData2 : class
		{
		}

		public void SomeMethod7()
		{
			for (;;)
			{
			}
			for (;;)
			{
			}
		}

		public void SomeMethod8() {
			SomeMethod7();
		}

		public void SomeMethod10_2()
		{
			#region MyRegion
			SomeMethod7();
			#endregion
		}

		public void SomeMethod11()
		{
			if (true)
			{ SomeMethod7(); }
			if (true)
			{
				SomeMethod7(); }
			if (true)
			{ SomeMethod7();
			}
			if (true) {
				SomeMethod7();}
		}

		public void SomeMethod12()
		{
			{
				SomeMethod7();
			}
			{
				SomeMethod7();
			}
			{
			}
			{
			}
		}

		public void SomeMethod13()
		{
			if (true
				|| false
				)
			{
				SomeMethod12();
			}

			while (true
				|| false
				)
			{
				SomeMethod12();
			}

			foreach (var b in new bool[0])
			{
				SomeMethod12();
			}

			foreach (var b in
				new bool[0]
				)
			{
				SomeMethod12();
			}
		}
		
		public void ReportMakerParameters(Func<string, string> makeCaption, Func<string> beginList,
			Func<IEnumerable<double>, string> makeStatistics, int itemMaker, Func<string> endList)
		{
			var a = 1;
			Console.WriteLine(a);
		}
		
		public void SomeMethod14() { SomeMethod13(); }
		public void SomeMethod15() { SomeMethod13(); }
	}

	public class SomeClass6
	{
	}

	public class SomeClass7
	{
	}

	public interface IInterface1
	{
	}

	public interface IInterface2
	{
	}

	public enum Enum1
	{
	}

	public enum Enum2
	{
	}

	public class SomeClass8 {
	}
}

namespace MyNamespace1
{
	/// <summary>
	/// blah blah class
	/// </summary>
	public class CommentsClass
	{
		public void SomeMethod()
		{
		}

		public void SomeMethod1()
		{
			//comment
			SomeMethod();
			//comment
		}

		public void SomeMethod2()
		{
			/*comment
			comment*/
			SomeMethod();
			/*comment
			comment*/
		}

		/*comment
		comment*/
		public void SomeMethod3()
		{
		}

		/*comment
		comment*/

		public void SomeMethod4()
		{
		}

		//comment
		//comment

		public void SomeMethod5()
		{
		}

		//comment
		//comment
		public void SomeMethod6()
		{
		}
	}

	public class CommentsClass2
	{
		/*comment
		comment*/
		public void SomeMethod()
		{
		}
	}

	public class CommentsClass3
	{
		/*comment
		comment*/

		public void SomeMethod()
		{
		}
	}

	public class CommentsClass4
	{
		//comment
		//comment

		public void SomeMethod()
		{
		}
	}

	public class CommentsClass5
	{
		//comment
		//comment
		public void SomeMethod()
		{
		}
	}

	public class CommentsClass6
	{
		/// <summary>
		/// blah blah method
		/// </summary>

		public void SomeMethod()
		{
		}
	}

	public class CommentsClass7
	{
		/// <summary>
		/// blah blah method
		/// </summary>
		public void SomeMethod()
		{
		}
	}

	public class CommentsClass8
	{
#if MYDEBUG
		public void SomeMethod()
		{
		}
#endif
	}
	
	public class CommentsClass8b
	{
#if true
		public void SomeMethod()
		{
		}
#endif
	}	

	public class CommentsClass9
	{
#if MYDEBUG
		public void SomeMethod()
		{
		}
#endif

		private int field;
	}
	
	public class CommentsClass9b
	{
#if true
		public void SomeMethod()
		{
		}
#endif

		private int field;
	}	
	
	public class CommentsClass10
	{
#if MYDEBUG
		public void SomeMethod()
		{
		}
#else
		public void AnotherMethod()
		{
		}
#endif

		private int field;
	}	
	
	public class CommentsClass11
	{
		#if MYDEBUG
		public void SomeMethod()
		{
		}
		#endif
	}	
}

namespace MyNamespace1
{
	[Mock]
	class Class1
	{
		[Mock]
		void Method()
		{
		}
	}
}