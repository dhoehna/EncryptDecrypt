using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid identification = Cosmos.Communication.Upload("Hello, I have been encrypted");
            byte[] information = Cosmos.Communication.Download(identification);
            Console.WriteLine(Encoding.ASCII.GetString(information));
        }
    }
}
