using System;

namespace YouITest.Models
{
    public class Address
    {
        public Address(string addressString)
        {
            try
            {
                // Split address on space, assumes address are all in the same format as per provided file.
                // Ideally an address validator service that returns suggested real addresses would be called.
                var addressArr = addressString.Split(' ');

                HouseNumber = addressArr[0];
                StreetName = addressArr[1];
                StreetType = addressArr[2];
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured creating an Address object for address: " + addressString, ex);
            }
        }

        public string HouseNumber { get; set; }
        public string StreetName { get; set; }
        public string StreetType { get; set; }

        public override string ToString()
        {
            return HouseNumber + " " + StreetName + " " + StreetType;
        }
    }
}
