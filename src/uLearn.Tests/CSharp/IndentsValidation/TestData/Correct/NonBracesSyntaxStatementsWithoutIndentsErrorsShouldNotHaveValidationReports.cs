namespace uLearn.CSharp.IndentsValidation.TestData.Correct
{
	public class NonBracesSyntaxStatementsWithoutIndentsErrorsShouldNotHaveValidationReports
    {
        public static void Main(string[] args)
        {
            if (true ||
                true) return;
            else return;

            if (true ||
                true)
                return;
            else return;

            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    for (var k = 0; j < 10; j++)
                        for (var l = 0; j < 10; j++)
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

            if (true)
                SomeMethod();
            else
                SomeMethod();

            if (true)
                SomeMethod();
            else SomeMethod();

            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
            for (var k = 0; j < 10; j++)
            for (var l = 0; j < 10; j++)
            {
            }


            do
                SomeMethod();
            while (true);

            if (true)
                           SomeMethod();

            while (false)
                SomeMethod();

            for (;;)
                SomeMethod();

            using (new DisposableMock())
            using (new DisposableMock())
            using (new DisposableMock())
            using (new DisposableMock())
            {
            }

            using (new DisposableMock())
                using (new DisposableMock())
                    using (new DisposableMock())
                        using (new DisposableMock())
                        {
                        }

            using (new DisposableMock())
                using (new DisposableMock())
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