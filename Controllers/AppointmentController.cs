using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedWebApp.Data;
using MedWebApp.Models;
using Microsoft.AspNetCore.Http;

namespace MedWebApp.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private void ReloadDoctorDropdown()
        {
            ViewBag.Doctors = _context.Users
                .Where(u => u.Role == Role.Doctor)
                .Select(d => new SelectListItem
                {
                    Value = d.UserId.ToString(),
                    Text = $"{d.FirstName} {d.LastName}"
                }).ToList();
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            return View(await _context.Appointment.ToListAsync());
        }

        // GET: Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointment/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Get list of doctors for the dropdown
            var doctors = _context.Users
                .Where(u => u.Role == Role.Doctor)
                .Select(d => new SelectListItem
                {
                    Value = d.UserId.ToString(),
                    Text = $"{d.FirstName} {d.LastName}"
                }).ToList();

            ViewBag.Doctors = doctors;
            return View();
        }


        // POST: Appointment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Appointment appointment)
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            appointment.UserId = int.Parse(userIdStr);
            appointment.Status = Status.Pending;

            _context.Appointment.Add(appointment);
            _context.SaveChanges();

            TempData["Message"] = "Appointment booked successfully!";
            return RedirectToAction("MyAppointments");
        }




        public IActionResult MyAppointments()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var appointments = _context.Appointment
                .Where(a => a.UserId == userId)
                .Include(a => a.Doctor)
                .ToList();

            return View(appointments);
        }


        // GET: Appointment/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointment.FindAsync(id);
            if (appointment == null)
                return NotFound();

            return View(appointment);
        }


        // POST: Appointment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,VisitDate,VisitTime")] Appointment model)
        {
            

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appointment = await _context.Appointment.FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.VisitDate = model.VisitDate;
            appointment.VisitTime = model.VisitTime;

            await _context.SaveChangesAsync();
            

            TempData["Message"] = "Appointment updated successfully!";
            return RedirectToAction("MyAppointments");
        }





        // GET: Appointment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointment
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointment.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment == null) return NotFound();

            _context.Appointment.Remove(appointment);
            _context.SaveChanges();

            return RedirectToAction("MyAppointments");
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointment.Any(e => e.AppointmentId == id);
        }
        [HttpGet]
        public IActionResult DoctorAppointments()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int doctorId = int.Parse(userIdStr);

            var appointments = _context.Appointment
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient) 
                .ToList();

            return View(appointments);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, Status status)
        {
            var appointment = _context.Appointment.FirstOrDefault(a => a.AppointmentId == id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            _context.SaveChanges();

            TempData["Message"] = "Appointment status updated.";
            return RedirectToAction("DoctorAppointments");
        }


    }
}
