using Microsoft.AspNetCore.Mvc;
using MedWebApp.Models;
using MedWebApp.Data;
using Microsoft.AspNetCore.Identity;

namespace MedWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    DOB = model.DOB,
                    Phone = model.Phone,
                    Street = model.Street,
                    City = model.City,
                    Postcode = model.Postcode,
                    Role = Role.Patient
                };

                user.Password = _passwordHasher.HashPassword(user, model.Password);

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("RegisterSuccess");
            }

            return View(model);
        }

        public IActionResult RegisterSuccess()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email not found.");
                    return View(model);
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                if (result != PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return View(model);
                }

                HttpContext.Session.SetString("FirstName", user.FirstName);
                HttpContext.Session.SetString("Role", user.Role.ToString());
                HttpContext.Session.SetString("UserId", user.UserId.ToString());

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            TempData["LogoutMessage"] = "You have been logged out.";
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AddDoctor()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpGet]
        public IActionResult DoctorList()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var doctors = _context.Users.Where(u => u.Role == Role.Doctor).ToList();
            return View(doctors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDoctor(AddDoctorViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            if (ModelState.IsValid)
            {
                var doctor = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    DOB = model.DOB,
                    Street = model.Street,
                    City = model.City,
                    Postcode = model.Postcode,
                    Role = Role.Doctor
                };

                doctor.Password = _passwordHasher.HashPassword(doctor, model.Password);

                _context.Users.Add(doctor);
                _context.SaveChanges();

                TempData["Message"] = "Doctor created successfully.";
                return RedirectToAction("AddDoctor");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EditDoctor(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var doctor = _context.Users.FirstOrDefault(u => u.UserId == id && u.Role == Role.Doctor);
            if (doctor == null) return NotFound();

            var model = new AddDoctorViewModel
            {
                UserId = doctor.UserId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email,
                Phone = doctor.Phone,
                DOB = doctor.DOB,
                Street = doctor.Street,
                City = doctor.City,
                Postcode = doctor.Postcode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDoctor(AddDoctorViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var doctor = _context.Users.FirstOrDefault(u => u.UserId == model.UserId && u.Role == Role.Doctor);
            if (doctor == null) return NotFound();

            doctor.FirstName = model.FirstName;
            doctor.LastName = model.LastName;
            doctor.Email = model.Email;
            doctor.Phone = model.Phone;
            doctor.DOB = model.DOB;
            doctor.Street = model.Street;
            doctor.City = model.City;
            doctor.Postcode = model.Postcode;

            _context.SaveChanges();
            TempData["Message"] = "Doctor updated successfully.";

            return RedirectToAction("DoctorList");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDoctor(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var doctor = _context.Users.FirstOrDefault(u => u.UserId == id && u.Role == Role.Doctor);
            if (doctor == null) return NotFound();

            _context.Users.Remove(doctor);
            _context.SaveChanges();

            TempData["Message"] = "Doctor deleted successfully.";
            return RedirectToAction("DoctorList");
        }

        [HttpGet]
        public IActionResult DoctorsDirectory(string search)
        {
            var doctorsQuery = _context.Users.Where(u => u.Role == Role.Doctor);

            if (!string.IsNullOrWhiteSpace(search))
            {
                doctorsQuery = doctorsQuery.Where(d =>
                    d.FirstName.Contains(search) || d.LastName.Contains(search));
            }

            var doctors = doctorsQuery
                .Select(d => new
                {
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.Phone
                }).ToList();

            return View(doctors);
        }

        [HttpGet]
        public IActionResult SearchDoctors(string search)
        {
            var doctorsQuery = _context.Users.Where(u => u.Role == Role.Doctor);

            if (!string.IsNullOrWhiteSpace(search))
            {
                doctorsQuery = doctorsQuery.Where(d =>
                    d.FirstName.Contains(search) || d.LastName.Contains(search));
            }

            var doctors = doctorsQuery
                .Select(d => new
                {
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.Phone
                }).ToList();

            return PartialView("_DoctorsTable", doctors);
        }

    }
}
