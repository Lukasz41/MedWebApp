using System;
using System.ComponentModel.DataAnnotations;

namespace MedWebApp.Validators
{
    public class MinAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public MinAgeAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dob)
            {
                if (dob > DateTime.Today)
                {
                    return new ValidationResult("Date of Birth cannot be in the future.");
                }

                var age = DateTime.Today.Year - dob.Year;
                if (dob > DateTime.Today.AddYears(-age)) age--;

                if (age < _minimumAge)
                {
                    return new ValidationResult($"You must be at least {_minimumAge} years old.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid Date of Birth.");
        }
    }
}
