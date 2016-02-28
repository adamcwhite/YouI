using System;
using YouITest.Managers;

namespace YouITest
{
    public static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Set file variables
                var filePath = @"C:\Temp\";
                var sourceFileName = @"data.csv";
                var nameFrequencyFileName = @"name_frequencies.csv";
                var orderedAddressFileName = @"ordered_addresses.csv";

                // Instantiate person manager
                var personManager = new PersonManager();

                // Load people from file into people collection
                personManager.LoadPeopleFromFile(filePath + sourceFileName);

                // output name frequency list
                personManager.OutputNameFrequencyToFile(filePath + nameFrequencyFileName);

                // Get sorted addresses
                personManager.OutputOrderedAddressesToFile(filePath + orderedAddressFileName);

                // Prevent console from closing
                Console.WriteLine(Environment.NewLine + "Press Enter to close console app.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error has occurred: {0}", ex.Message);
                Console.ReadLine();
            }
        }
    }
}
