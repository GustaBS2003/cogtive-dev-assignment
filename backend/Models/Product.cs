using System;
using System.ComponentModel.DataAnnotations;

namespace Cogtive.DevAssignment.Api.Models
{
    public class Machine
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class ProductionData
    {
        public int Id { get; set; }

        [Required]
        public int MachineId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Range(0, 100)]
        public decimal Efficiency { get; set; }

        [Range(0, int.MaxValue)]
        public int UnitsProduced { get; set; }

        [Range(0, int.MaxValue)]
        public int Downtime { get; set; } // In minutes
    }
}