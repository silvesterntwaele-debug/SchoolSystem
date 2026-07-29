using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Models;
using SchoolSystem.ViewModels;
using SchoolSystem.Data;


namespace SchoolSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ============================
        // LOGIN PAGE
        // ============================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ============================
        // LOGIN
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Redirect users according to their role
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }

            if (await _userManager.IsInRoleAsync(user, "Lecturer"))
            {
                return RedirectToAction("LecturerDashboard", "Lecturer");
            }

            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                return RedirectToAction("StudentDashboard", "Student");
            }

            return RedirectToAction("Index", "Home");
        }

        // ============================
        // LOGOUT
        // ============================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        // ============================
        // REGISTER PAGE
        // ============================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        // ============================
        // REGISTER
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign selected role
                await _userManager.AddToRoleAsync(user, model.Role);

                TempData["Success"] = $"{model.Role} account created successfully.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }











    }

}