using Cosmos;
using System;
using System.Text;

namespace EncryptDecryptApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid identifier = Communication.Upload("Hello, it is me");
            byte[] stuff = Communication.Download(identifier);

            Console.WriteLine(Encoding.ASCII.GetString(stuff));

        }
    }
}
