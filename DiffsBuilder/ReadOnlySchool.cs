using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffsBuilder
{
    class ReadOnlySchool
    {
        public string Name { get; set; }

        public IReadOnlyList<Pupil> pupil { get; set; }
    }
}
