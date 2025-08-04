using Cogtive.DevAssignment.Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Cogtive.Test
{
    public class ModelValidationTests
    {
        [Fact]
        public void Machine_WithValidData_IsValid()
        {
            var machine = new Machine
            {
                Name = "Test Machine",
                SerialNumber = "SN-123",
                Type = "CNC",
                IsActive = true
            };

            var context = new ValidationContext(machine, null, null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(machine, context, results, true);

            Assert.True(isValid);
        }

        [Fact]
        public void Machine_WithMissingName_IsInvalid()
        {
            var machine = new Machine
            {
                Name = "",
                SerialNumber = "SN-123",
                Type = "CNC",
                IsActive = true
            };

            var context = new ValidationContext(machine, null, null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(machine, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(Machine.Name)));
        }

        [Fact]
        public void ProductionData_WithValidData_IsValid()
        {
            var data = new ProductionData
            {
                MachineId = 1,
                Timestamp = DateTime.UtcNow,
                Efficiency = 85.5m,
                UnitsProduced = 100,
                Downtime = 10
            };

            var context = new ValidationContext(data, null, null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(data, context, results, true);

            Assert.True(isValid);
        }

        [Fact]
        public void ProductionData_WithEfficiencyOutOfRange_IsInvalid()
        {
            var data = new ProductionData
            {
                MachineId = 1,
                Timestamp = DateTime.UtcNow,
                Efficiency = 150m, // Fora do intervalo permitido
                UnitsProduced = 100,
                Downtime = 10
            };

            var context = new ValidationContext(data, null, null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(data, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProductionData.Efficiency)));
        }

        [Fact]
        public void ProductionData_WithNegativeUnitsProduced_IsInvalid()
        {
            var data = new ProductionData
            {
                MachineId = 1,
                Timestamp = DateTime.UtcNow,
                Efficiency = 80m,
                UnitsProduced = -5, // Valor inválido
                Downtime = 0
            };

            var context = new ValidationContext(data, null, null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(data, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ProductionData.UnitsProduced)));
        }
    }
}