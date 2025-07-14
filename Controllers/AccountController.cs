using System.Web.Mvc;
using LoginRegistrationMVC.Models;
using LoginRegistrationMVC.Data;
using LoginRegistrationMVC.Helpers;

namespace LoginRegistrationMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public AccountController()
        {
            _userRepository = new UserRepository();
        }

        // GET: /Account/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepository.GetUserByEmail(model.Email);
                if (user != null && PasswordHelper.VerifyPassword(model.Password, user.HashedPassword))
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View(model);
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _userRepository.GetUserByEmail(model.Email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        Email = model.Email,
                        HashedPassword = PasswordHelper.HashPassword(model.Password)
                    };
                    _userRepository.AddUser(user);
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Email is already registered.");
            }
            return View(model);
        }
    }
}