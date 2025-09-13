using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MedWebApp.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Visit date is required.")]
        public DateTime VisitDate { get; set; }

        [Required(ErrorMessage = "Visit time is required.")]
        public TimeSpan VisitTime { get; set; }

        public Status Status { get; set; }
        
        public int DoctorId { get; set; }
        [ValidateNever]
        public User Doctor { get; set; }
        [ValidateNever]
        public User Patient { get; set; }

    }
}
