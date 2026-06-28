using BookVerse.DataAccess.Repository.IRepository;
using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookVerse.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        // ── Register ──────────────────────────────────────────────────────────

        // GET: /User/Register
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            var vm = new RegisterVM
            {
                RoleList = _roleManager.Roles
                    .Where(r => r.Name != SD.Role_Admin)   // hide Admin from public form
                    .Select(r => new SelectListItem { Text = r.Name, Value = r.Name }),

                CompanyList = _unitOfWork.Company.GetAll()
                    .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
            };

            return View(vm);
        }

        // POST: /User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                RepopulateLists(vm);
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                Name = vm.Name,
                StreetAddress = vm.StreetAddress,
                City = vm.City,
                State = vm.State,
                PostalCode = vm.PostalCode,
                CompanyId = vm.Role == SD.Role_Company ? vm.CompanyId : null
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // Assign role — default to Customer if nothing valid was chosen
                var role = !string.IsNullOrEmpty(vm.Role) && await _roleManager.RoleExistsAsync(vm.Role)
                    ? vm.Role
                    : SD.Role_Customer;

                await _userManager.AddToRoleAsync(user, role);
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["success"] = "Welcome to BookVerse!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            RepopulateLists(vm);
            return View(vm);
        }

        // ── Login ─────────────────────────────────────────────────────────────

        // GET: /User/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginVM());
        }

        // POST: /User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: true);      // locks account after 5 bad attempts

            if (result.Succeeded)
            {
                TempData["success"] = "Login successful!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Account locked due to too many failed attempts. Try again in 5 minutes.");
                return View(vm);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        // ── Logout ────────────────────────────────────────────────────────────

        // POST: /User/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();            // wipe cart count badge
            TempData["success"] = "You have been signed out.";
            return RedirectToAction("Index", "Home");
        }

        // ── Access Denied ─────────────────────────────────────────────────────

        // GET: /User/AccessDenied
        public IActionResult AccessDenied() => View();

        // ── Profile (authenticated users only) ────────────────────────────────

        // GET: /User/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new UserProfileVM
            {
                Name = user.Name,
                Email = user.Email!,
                StreetAddress = user.StreetAddress,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                Role = roles.FirstOrDefault() ?? SD.Role_Customer
            };

            return View(vm);
        }

        // POST: /User/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Name = vm.Name;
            user.StreetAddress = vm.StreetAddress;
            user.City = vm.City;
            user.State = vm.State;
            user.PostalCode = vm.PostalCode;

            await _userManager.UpdateAsync(user);
            TempData["success"] = "Profile updated.";
            return RedirectToAction(nameof(Profile));
        }

        // ── Admin: manage users ───────────────────────────────────────────────

        // GET: /User/ManageUsers
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users
                .Select(u => new ManageUserVM
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    City = u.City,
                    LockoutEnd = u.LockoutEnd
                }).ToList();

            // Resolve roles per user (N+1 is acceptable for small admin lists)
            foreach (var u in users)
            {
                var appUser = await _userManager.FindByIdAsync(u.Id);
                u.Role = string.Join(", ", await _userManager.GetRolesAsync(appUser!));
            }

            return View(users);
        }

        // POST: /User/LockUnlock — toggles account lockout for a user
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Currently locked → unlock
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                TempData["success"] = $"{user.Name} has been unlocked.";
            }
            else
            {
                // Currently active → lock for 100 years
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["success"] = $"{user.Name} has been locked.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void RepopulateLists(RegisterVM vm)
        {
            vm.RoleList = _roleManager.Roles
                .Where(r => r.Name != SD.Role_Admin)
                .Select(r => new SelectListItem { Text = r.Name, Value = r.Name });

            vm.CompanyList = _unitOfWork.Company.GetAll()
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
        }
    }
}
