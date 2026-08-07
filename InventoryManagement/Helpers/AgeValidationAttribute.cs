using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Helpers
{
    public class AgeValidationAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public AgeValidationAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Date of Birth is required.");

            DateTime dob = (DateTime)value;

            if (dob >= DateTime.Today)
                return new ValidationResult("Date of Birth cannot be today or a future date.");

            int age = DateTime.Today.Year - dob.Year;

            if (dob > DateTime.Today.AddYears(-age))
                age--;

            if (age < _minimumAge)
                return new ValidationResult("Age must be at least 18 years.");

            return ValidationResult.Success;
        }
    }
}