using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using YouITest.Managers;
using YouITest.Models;

namespace UnitTests
{
    [TestFixture]
    public class PersonTests
    {
        private const string TestFilesDirectory = @"C:\Temp\";
        private const string SourceFileName = @"data.csv";
        private const string NameFrequencyFileName = @"name_frequencies.csv";
        private const string OrderedAddressFileName = @"ordered_addresses.csv";

        private readonly string _testDataDirectory =
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "\\TestData\\";

        [SetUp]
        public void Init()
        {
            // Copy test file to C:\Temp
            if (!Directory.Exists(TestFilesDirectory))
            {
                // Create directory
                var directoryInfo = new DirectoryInfo(TestFilesDirectory);
                directoryInfo.Create();
            }

            // Copy valid file
            var validFile = _testDataDirectory + "data.csv";
            File.Copy(validFile, TestFilesDirectory + SourceFileName, true);
        }

        [TearDown]
        public void Dispose()
        {
            // Delete valid file
            File.Delete(TestFilesDirectory + SourceFileName);
        }
        
        [TestCase]
        public void LoadPeopleFromFile_Successfully()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;

            // Act
            personManager.LoadPeopleFromFile(sourceFile);

            // Assert
            Assert.AreEqual(8, personManager.GetPersonList().Count, "Unexpected number of people.");
        }

        [TestCase]
        public void LoadPeopleFromFile_FileMissing_Exception()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;
            var expectedException =
                "Source file " + sourceFile + " doesn't exist, please ensure the file exists in the location and try again.";

            // Act
            File.Delete(sourceFile);
            var ex = Assert.Catch<ApplicationException>(() => personManager.LoadPeopleFromFile(sourceFile));

            // Assert
            Assert.IsTrue(ex.Message.Contains(expectedException));
        }

        [TestCase]
        public void LoadPeopleFromFile_MissingFieldHeaders_Exception()
        {
            // Arrange
            var invalidDataFileName = "data_NoHeaders.csv";
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + invalidDataFileName;
            var expectedException = "First line does not contain field headers";

            File.Copy(_testDataDirectory + invalidDataFileName, TestFilesDirectory + invalidDataFileName, true);

            // Act
            var ex = Assert.Catch<ApplicationException>(() => personManager.LoadPeopleFromFile(sourceFile));

            // Assert
            Assert.IsTrue(ex.Message.Contains(expectedException));

            // Cleanup
            File.Delete(TestFilesDirectory + invalidDataFileName);
        }
        
        [TestCase]
        public void LoadPeopleFromFile_InvalidFieldHeaders_Exception()
        {
            // Arrange
            var invalidDataFileName = "data_InvalidHeaders.csv";
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + invalidDataFileName;
            var expectedException = "Unexpected field headers";

            File.Copy(_testDataDirectory + invalidDataFileName, TestFilesDirectory + invalidDataFileName, true);

            // Act
            var ex = Assert.Catch<ApplicationException>(() => personManager.LoadPeopleFromFile(sourceFile));

            // Assert
            Assert.IsTrue(ex.Message.Contains(expectedException));

            // Cleanup
            File.Delete(TestFilesDirectory + invalidDataFileName);
        }

        [TestCase]
        public void ValidateFileHeader_InvalidFieldHeaders_True()
        {
            // Arrange
            var personManager = new PersonManager();
            var headerArr = new string[] { "FirstName", "LastName", "Address", "PhoneNumber" };

            // Act
            var isHeaderValid = personManager.ValidateFileHeader(headerArr);

            // Assert
            Assert.IsTrue(isHeaderValid, "Headers should be valid.");
        }

        [TestCase]
        public void ValidateFileHeader_InvalidFieldHeaders_False()
        {
            // Arrange
            var personManager = new PersonManager();
            var headerArr = new string[] {"FirstName", "Address", "PhoneNumber"};

            // Act
            var isHeaderValid = personManager.ValidateFileHeader(headerArr);

            // Assert
            Assert.IsFalse(isHeaderValid, "Headers should not be valid.");
        }

        [TestCase]
        public void CreatePersonFromLine_Successful()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;
            var dataLine = "Jimmy,Smith,102 Long Lane,29384857";
            var expectedPerson = new Person
            {
                FirstName = "Jimmy",
                LastName = "Smith",
                Address = new Address("102 Long Lane"),
                PhoneNumber = "29384857"
            };

            // Act
            personManager.LoadPeopleFromFile(sourceFile);
            var person = personManager.CreatePersonFromLine(dataLine);

            // Assert
            Assert.IsTrue(person.FirstName.Equals(expectedPerson.FirstName), "Unexpected FirstName.");
            Assert.IsTrue(person.LastName.Equals(expectedPerson.LastName), "Unexpected LastName.");
            Assert.IsTrue(person.Address.ToString().Equals(expectedPerson.Address.ToString()), "Unexpected Address.");
            Assert.IsTrue(person.PhoneNumber.Equals(expectedPerson.PhoneNumber), "Unexpected PhoneNumber.");
        }

        [TestCase]
        public void CreatePersonFromLine_MissingFields_Exception()
        {
            // Arrange
            var personManager = new PersonManager();
            var dataLine = "Jimmy,Smith,29384857";
            var expectedException = "the file has incorrect number of fields";

            // Act
            var ex = Assert.Catch<ApplicationException>(() => personManager.CreatePersonFromLine(dataLine));

            // Assert
            Assert.IsTrue(ex.Message.Contains(expectedException));

        }

        [TestCase]
        public void GetNameFrequencyList_Successful()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;
            var expectedFirstFrequencyName = "Brown";
            var expectedFirstFrequencyCount = 2;
            var expectedLastFrequencyName = "John";
            var expectedLastFrequencyCount=  1;
            var expectedFrequencyList = new List<NameFrequency>
            {
                new NameFrequency("Brown", 2),
                new NameFrequency("Clive", 2),
                new NameFrequency("Graham", 2),
                new NameFrequency("Howe", 2),
                new NameFrequency("James", 2),
                new NameFrequency("Owen", 2),
                new NameFrequency("Smith", 2),
                new NameFrequency("Jimmy", 1),
                new NameFrequency("John", 1),
            };

            // Act
            personManager.LoadPeopleFromFile(sourceFile);
            var frequencyList = personManager.GetNameFrequencyList();

            // Assert
            Assert.AreEqual(expectedFrequencyList.Count, frequencyList.Count, "Unexpected count in Frequency List");
            Assert.AreEqual(expectedFirstFrequencyName, frequencyList.FirstOrDefault().Name, "First entry Name in Frequency List unexpected");
            Assert.AreEqual(expectedFirstFrequencyCount, frequencyList.FirstOrDefault().Frequency, "First entry Count in Frequency List unexpected");
            Assert.AreEqual(expectedLastFrequencyName, frequencyList.LastOrDefault().Name, "Last entry Name in Frequency List unexpected");
            Assert.AreEqual(expectedLastFrequencyCount, frequencyList.LastOrDefault().Frequency, "Last entry Count in Frequency List unexpected");
        }

        [TestCase]
        public void GetOrderedAddressList_Successful()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;
            var expectedFirstAddress = "65 Ambling Way";
            var expectedLastAddress = "49 Sutherland St";
            var expectedAddressList = new List<Address>
            {
                new Address("65 Ambling Way"),
                new Address("8 Crimson Rd"),
                new Address("12 Howard St"),
                new Address("102 Long Lane"),
                new Address("94 Roland St"),
                new Address("78 Short Lane"),
                new Address("82 Stewart St"),
                new Address("49 Sutherland St")
            };

            // Act
            personManager.LoadPeopleFromFile(sourceFile);
            var addressList = personManager.GetOrderedAddressList();

            // Assert
            Assert.AreEqual(expectedAddressList.Count, addressList.Count, "Unexpected count in Address List");
            Assert.AreEqual(expectedFirstAddress, addressList.FirstOrDefault().ToString(), "First entry Address List unexpected");
            Assert.AreEqual(expectedLastAddress, addressList.LastOrDefault().ToString(), "Last entry Address List unexpected");
        }

        [TestCase]
        public void OutputNameFrequencyToFile_Successful()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;
            var destinationFile = TestFilesDirectory + "name_frequencies.csv";
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }
            
            // Act
            personManager.LoadPeopleFromFile(sourceFile);
            personManager.OutputNameFrequencyToFile(destinationFile);
            ;

            // Assert
            Assert.IsTrue(File.Exists(destinationFile), "Name frequency file not created.");

            // Cleanup
            File.Delete(destinationFile);
        }

        [TestCase]
        public void OutputOrderedAddressesToFile_Successful()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;
            var destinationFile = TestFilesDirectory + "ordered_addresses.csv";
            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            // Act
            personManager.LoadPeopleFromFile(sourceFile);
            personManager.OutputOrderedAddressesToFile(destinationFile);
            ;

            // Assert
            Assert.IsTrue(File.Exists(destinationFile), "Ordered addresses file not created.");

            // Cleanup
            File.Delete(destinationFile);
        }

        [TestCase]
        public void GetPersonList_Successfully()
        {
            // Arrange
            var personManager = new PersonManager();
            var sourceFile = TestFilesDirectory + SourceFileName;

            // Act
            personManager.LoadPeopleFromFile(sourceFile);
            var personList = personManager.GetPersonList();

            // Assert
            Assert.AreEqual(8, personList.Count, "Unexpected number of people.");
        }

    }
}