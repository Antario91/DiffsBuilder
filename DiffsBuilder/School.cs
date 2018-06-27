using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    class School
    {
        public string Name { get; set; }

        public IEnumerable<Pupil> Pupil { get; set; }

        public Room Room { get; set; }
    }
}
