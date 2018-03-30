namespace uLearn.CSharp.IndentsValidation.TestData.Incorrect
{
    public class NonBracesSyntaxStatementsWithIndentsErrorsShouldHaveValidationReports
    {
        public static void Main(string[] args)
        {
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                  for (var k = 0; j < 10; j++)
                        {
                        }

            if (true)
                if (true) return;
                else if (true) return;
                else return;
            else
            if (true) return;
            else if (true) return;
            else return;

            if (true) SomeMethod();
            else foreach (var e in new int[0])
                SomeMethod();
            
	  	    if (true)


 	  	   		SomeMethod();
            
            do
         SomeMethod();

            while (true);


            do
       SomeMethod();
            while (true);

            do
            SomeMethod();
          while (true);

            if (true)
        SomeMethod();
            else
    SomeMethod();

            if (true)
             SomeMethod();
         else
           SomeMethod();

            if (true)
      SomeMethod();

            if (true)
             SomeMethod();

            if (true)
		SomeMethod();

            while (false)

        SomeMethod();

            for (;;)
SomeMethod();

            using (new DisposableMock())
                    using (new DisposableMock())
            {
            }

        using (new DisposableMock())
            using (new DisposableMock())
            {
            }
        }

        public static void SomeMethod()
        {
        }
    }
}