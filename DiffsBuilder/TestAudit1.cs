using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DiffsBuilder
{
    public class TestAudit1
    {
        public string Name { get; set; }
        public long Number { get; set; }

        public IEnumerable<TestAudit2> TestAudits { get; set; }
    }
}