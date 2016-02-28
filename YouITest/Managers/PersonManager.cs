using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YouITest.Common;
using YouITest.Models;

namespace YouITest.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonManager
    {
        private const char FieldDelimiter = ','; // Holds each line from file
        private int _firstNameFieldPosition;     // Delimited position of the FirstName Field.
        private int _lastNameFieldPosition;      // Delimited position of the LastName Field.
        private int _addressFieldPosition;       // Delimited position of the Address Field.
        private int _phoneNumberFieldPosition;   // Delimited position of the PhoneNumber Field.
        private List<Person> _personList;        // Holds people loaded from a source file in memory. 

        public PersonManager()
        {
            _personList = new List<Person>();    // Initialise PersonList.
        }

        /// <summary>
        /// Loads people into an in memory collection from the file passed in.
        /// </summary>
        /// <param name="filePath">comma dilimited csv file</param>
        public void LoadPeopleFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new ApplicationException("Source file " + filePath + " doesn't exist, please ensure the file exists in the location and try again.");
                }
                
                using (StreamReader streamReader = File.OpenText(filePath))
                {
                    var line = "";                          // Holds each line from file
                    string[] fieldPositions;                // Holds position of the fields in the header of the input file

                    // Read first line to get header positions
                    line = streamReader.ReadLine();

                    Console.WriteLine(line);    // Testing

                    if (line == null || line.Length <= 0)
                    {
                        throw new ApplicationException("First line does not contain field headers.  Please verify the first line contains the correct field headers and try again.");
                    }
                    else
                    {
                        // Header in file contains data, split and then validate.
                        fieldPositions = line.Split(FieldDelimiter);

                        // Validate Headers.
                        if (ValidateFileHeader(fieldPositions) == false)
                        {
                            throw new ApplicationException("Unexpected field headers.  Please verify the first line contains the correct field headers and try again.");
                        }

                        // Header fields are valid, loop through file to extract data.
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line.Length <= 0) continue; // skip if line contains no data

                            Console.WriteLine(line);    // Testing

                            // Add Person to in memory collection
                            _personList.Add(CreatePersonFromLine(line));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error has occurred loading people: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Checks the headers from a data file and makes sure they match the expected names and have the expected number of fields.
        /// </summary>
        /// <param name="fieldPositions">Array of field names from the source data file.</param>
        /// <returns>True if header feilds are expected.  False if there are missing, extra or misnamed field headers.</returns>
        public bool ValidateFileHeader(string[] fieldPositions)
        {
            try
            {
                var isValid = fieldPositions.Length == PersonConstants.FieldCount &&
                              fieldPositions.Contains(PersonConstants.FirstNameFieldName) &&
                              fieldPositions.Contains(PersonConstants.LastNameFieldName) &&
                              fieldPositions.Contains(PersonConstants.AddressFieldName) &&
                              fieldPositions.Contains(PersonConstants.PhoneNumberFieldName);

                // set field positions
                _firstNameFieldPosition = Array.IndexOf(fieldPositions, PersonConstants.FirstNameFieldName);
                _lastNameFieldPosition = Array.IndexOf(fieldPositions, PersonConstants.LastNameFieldName);
                _addressFieldPosition = Array.IndexOf(fieldPositions, PersonConstants.AddressFieldName);
                _phoneNumberFieldPosition = Array.IndexOf(fieldPositions, PersonConstants.PhoneNumberFieldName);

                return isValid;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while validating the file headers.", ex);
            }
        }

        /// <summary>
        /// Converts a data line from a source file into a Person object.
        /// </summary>
        /// <param name="line">single data line from source file.</param>
        /// <returns>Person object</returns>
        public Person CreatePersonFromLine(string line)
        {
            try
            {
                var lineArr = line.Split(FieldDelimiter);

                // Check line from file has required number of fields
                if (lineArr.Length != PersonConstants.FieldCount)
                {
                    throw new ApplicationException("The following line in the file has incorrect number of fields: " +
                                                   line);
                }

                var person = new Person
                {
                    FirstName = lineArr[_firstNameFieldPosition],
                    LastName = lineArr[_lastNameFieldPosition],
                    Address = new Address(lineArr[_addressFieldPosition]),
                    PhoneNumber = lineArr[_phoneNumberFieldPosition]
                };

                return person;
            }
            catch (ApplicationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating the Person Object for line: " + line, ex);
            }
        }

        /// <summary>
        /// Returns list of occurances for all FirstName and Surnames.
        /// </summary>
        /// <returns>List of NameFrequency objects.</returns>
        public List<NameFrequency> GetNameFrequencyList()
        {
            try
            {
                // Merge Given Names and Surnames into list
                var names = (from person in _personList
                            select person.FirstName)
                            .Concat(from person in _personList
                                select person.LastName).ToList();

                // Create name frequency list.  Default order to Frequency Descending then by Name Ascending
                var nameFrequencies = names.GroupBy(name => name).Select(name => new NameFrequency(name.Key, name.Count()))
                .OrderByDescending(x => x.Frequency).ThenBy(x => x.Name).ToList();

                return nameFrequencies;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while creating the name frequency list.", ex);
            }
        }

        /// <summary>
        /// Returns a list of addresses ordered by street name.
        /// </summary>
        /// <returns>List of Address objects</returns>
        public List<Address> GetOrderedAddressList()
        {
            try
            {
                return _personList.Select(person => person.Address).OrderBy(address => address.StreetName).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while ordering the address list.", ex);
            }
        }

        /// <summary>
        /// Creates comma dilimited file of addresses stored in Person Sollection, ordered by Street Name ascending.
        /// </summary>
        /// <param name="filePath">Location and name of output file.</param>
        public void OutputNameFrequencyToFile(string filePath)
        {
            try
            {
                using (var streamWriter = File.CreateText(filePath))
                {
                    foreach (var nameFrequency in GetNameFrequencyList())
                    {
                        streamWriter.WriteLine(nameFrequency.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while outputting the name frequency list.", ex);
            }
        }

        /// <summary>
        /// Creates comma dilimited file of addresses stored in Person Sollection, ordered by Street Name ascending.
        /// </summary>
        /// <param name="filePath">Location and name of output file.</param>
        public void OutputOrderedAddressesToFile(string filePath)
        {
            try
            {
                using (var streamWriter = File.CreateText(filePath))
                {
                    foreach (var nameFrequency in GetOrderedAddressList())
                    {
                        streamWriter.WriteLine(nameFrequency.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while outputting the ordered addresses list.", ex);
            }
        }

        /// <summary>
        /// Gets the collection of Person objects from memory.
        /// </summary>
        /// <returns>Person List</returns>
        public List<Person> GetPersonList()
        {
            return _personList;
        }
    }
}
