namespace uLearn.CSharp.ExcessLinesValidation.TestData.Incorrect

{

	public class BaseClass { }
	
	public class SomeClass
		: BaseClass
	
	{
	}
	
	public class SomeClass2
	
	{
	}

	public class SomeClass3
	{
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
		
		public void SomeMethod4<TData>()
			where TData : class
		
		{
		}

		public int SomeMethod5()
		{

			SomeMethod2();
			return 0;

		}

		public void SomeMethod11()
		{

			{

				SomeMethod2();
			}

		}
		
		public void SomeMethod12() { SomeMethod11(); }
		public void SomeMethod13() { SomeMethod11(); }
		
	}

	public class SomeClass4
	{
	
		public void SomeMethod()
		{
		}
	
		public void SomeMethod2()
		{
			for (; ; )
	
			{
	
				SomeMethod();
	
			}
	
		}
	
		public void SomeMethod3()
		{
		}
		public void SomeMethod4()
		{
		}
		public class SomeClass5
		{
		}

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

	public enum Enum3
	{
	}
	public interface IInterface3
	{
	}
	public class SomeClass8
	{
	}

}
namespace MyNamespace1
{
}
namespace MyNamespace1
{
	public class Foo
	{
		public Foo()
		{
		}
		private int Pr { get; set; } = 0;
	}
}

namespace MyNamespace1
{
	public class IfDefClass
	{
		
#if true
		public void SomeMethod()
		{
		}
#endif

		private int field;
	}	
}