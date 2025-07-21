using System.IO;
using System.Linq;
using System.Web.Mvc;
using LoginRegistrationMVC.Data;
using LoginRegistrationMVC.Models;

namespace LoginRegistrationMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Message = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))) });
            }
            var user = _userRepository.GetUserByEmail(model.Email);
            if (user != null)
            {
                System.Web.Security.FormsAuthentication.SetAuthCookie(model.Email, false);
                return Json(new { Success = true, Message = "Login successful" });
            }
            return Json(new { Success = false, Message = "Invalid email or password." });
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Message = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))) });
            }

            var existingUser = _userRepository.GetUserByEmail(model.Email);
            if (existingUser != null)
            {
                return Json(new { Success = false, Message = "Email is already registered." });
            }

            string imagePath = null;
            if (model.Image != null && model.Image.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Path.GetFileName(model.Image.FileName);
                string path = Path.Combine(uploadsFolder, fileName);
                model.Image.SaveAs(path);
                imagePath = "/Uploads/" + fileName;
            }

            var user = new User
            {
                Email = model.Email,
                HashedPassword = model.Password,
                Name = model.Name,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                ImagePath = imagePath,
                Role = "User"
            };
            _userRepository.AddUser(user);
            System.Web.Security.FormsAuthentication.SetAuthCookie(model.Email, false);
            return Json(new { Success = true, Message = "Registration successful" });
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var user = _userRepository.GetUserByEmail(User.Identity.Name);
            ViewBag.User = user; 
            return View(user);
        }

        public ActionResult Logout()
        {
            System.Web.Security.FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // [AllowAnonymous]
        // public ActionResult Search()
        // {
        //     var users = _userRepository.GetAllUsers() ?? new List<User>();
        //     var displayUsers = users.Select(u => new User
        //     {
        //         Id = u.Id,
        //         Email = u.Email,
        //         Name = u.Name,
        //         HashedPassword = null,
        //         Gender = null,
        //         DateOfBirth = default(DateTime),
        //         ImagePath = null,
        //         Role = null
        //     }).ToList();
        //     return View(displayUsers);
        // }

        // [HttpGet]
        // [AllowAnonymous]
        // public ActionResult SearchData(string searchTerm = null, string filter = null)
        // {
        //     var users = _userRepository.GetAllUsers(searchTerm, filter) ?? new List<User>();
        //     var displayUsers = users.Select(u => new User
        //     {
        //         Id = u.Id,
        //         Email = u.Email,
        //         Name = u.Name,
        //         HashedPassword = null,
        //         Gender = null,
        //         DateOfBirth = default(DateTime),
        //         ImagePath = null,
        //         Role = null
        //     }).ToList();
        //     return Json(displayUsers, JsonRequestBehavior.AllowGet);
        // }
    }
}

