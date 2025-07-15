using System.Linq;
using System.Web.Mvc;
using LoginRegistrationMVC.Data;
using LoginRegistrationMVC.Helpers;
using LoginRegistrationMVC.Models;

namespace LoginRegistrationMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public AccountController()
        {
            _userRepository = new UserRepository();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { Success = false, Message = string.Join("; ", errors) });
            }

            var user = _userRepository.GetUserByEmail(model.Email);
            if (user != null && PasswordHelper.VerifyPassword(model.Password, user.HashedPassword))
            {
                return Json(new { Success = true, Message = "Login successful" });
            }

            return Json(new { Success = false, Message = "Invalid email or password." });
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { Success = false, Message = string.Join("; ", errors) });
            }

            var existingUser = _userRepository.GetUserByEmail(model.Email);
            if (existingUser != null)
            {
                return Json(new { Success = false, Message = "Email is already registered." });
            }

            var user = new User
            {
                Email = model.Email,
                HashedPassword = PasswordHelper.HashPassword(model.Password)
            };
            _userRepository.AddUser(user);

            return Json(new { Success = true, Message = "Registration successful" });
        }
        public ActionResult Index()
        {
            return View();
        }
    }
}