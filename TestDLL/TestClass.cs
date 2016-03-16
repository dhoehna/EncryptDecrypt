using Microsoft.SqlServer.Server;

namespace TestDLL
{
    public static class TestClass
    {
        [SqlFunction(DataAccess = DataAccessKind.None,
            IsDeterministic = false)]
        public static string SayHello()
        {
            return "Say Hello World";
        }
    }
}
