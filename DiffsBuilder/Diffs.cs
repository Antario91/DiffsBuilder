using System.Collections.Generic;

namespace Scc.Portal.Orchard.AuditTrails
{
    public class Diffs
    {
        public string FieldName { get; set; }
        public OperationType OperationType { get; set; }
        public string Code { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public ICollection<Diffs> ChildDiffs { get; set; }
    }

    public enum OperationType
    {
        Modified,
        Inserted,
        Deleted
    }
}