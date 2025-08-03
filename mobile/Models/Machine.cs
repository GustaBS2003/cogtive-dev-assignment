using System;

namespace CogtiveDevAssignment.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductionData
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public DateTime Timestamp { get; set; }
        // Intentional error: Efficiency should be a decimal/double, not string
        public decimal Efficiency { get; set; }
        public int UnitsProduced { get; set; }
        public int Downtime { get; set; } // In minutes
    }
}
