using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouITest.Models
{
    public class NameFrequency
    {
        public NameFrequency(string name, int frequency)
        {
            Name = name;
            Frequency = frequency;
        }

        public string Name { get; set; }
        public int Frequency { get; set; }

        public override string ToString()
        {
            return Name + ", " + Frequency;            
        }
    }
}
