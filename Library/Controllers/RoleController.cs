using Library.Models;
using Library.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Library.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<LibraryUser> _userManager;
        private readonly IStringLocalizer<RoleController> _localizer;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<LibraryUser> userManager, IStringLocalizer<RoleController> localizer)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _localizer = localizer;

        }
        public async Task<IActionResult> Index()
        {
            List<IdentityRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel) {
            if (ModelState.IsValid)
            {
                bool roleExist = await _roleManager.RoleExistsAsync(roleModel.RoleName);
                if (roleExist) {
                    ModelState.AddModelError("", _localizer["RoleAlreadyExists"]);
                }
                else
                {                  
                    IdentityResult res = await _roleManager.CreateAsync(new IdentityRole(roleModel.RoleName));
                    if (res.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    foreach (var error in res.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(roleModel);
        }

        public async Task<IActionResult> EditRole(string roleId)
        {
            IdentityRole? role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return View("Error");
            }
            var roleModel = new EditRoleViewModel
            {
                Id = roleId,
                RoleName = role.Name,
                Users = []
            };
            foreach (var user in await _userManager.Users.ToListAsync())
            {
                if(await _userManager.IsInRoleAsync(user, role.Name))
                {
                    roleModel.Users.Add(user.UserName);
                }
            }
            return View(roleModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if(ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if(role == null)
                {
                    ViewBag.ErrorMessage = $"{model.Id} " + _localizer["IdCannotFound"];
                    return View("NotFound");
                }
                else
                {
                    role.Name = model.RoleName;
                    
                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError ("", error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    ViewBag.ErrorMessage = $"{roleId} " + _localizer["IdCannotFound"];
                    return View("NotFound");
                }
                else
                {
                    var res = await _roleManager.DeleteAsync(role);
                    if (res.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    foreach (var error in res.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }                   
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditUsersRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"{roleId} " + _localizer["IdCannotFound"];
                return View("NotFound");
            }
            ViewData["RoleName"] = role.Name;
            var model = new List<UserRoleViewModel>();
            foreach (var user in await _userManager.Users.ToListAsync())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = (await _userManager.IsInRoleAsync(user, role.Name)),
                };
                model.Add(userRoleViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"{roleId} " + _localizer["IdCannotFound"];
                return View("NotFound");
            }
            for(int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                IdentityResult? result;

                if(model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name) ))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && (await _userManager.IsInRoleAsync(user, role.Name) )){
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else { continue; }

                if(!result.Succeeded)
                {
                    return View("Error");
                }
            }
            return RedirectToAction("EditRole", new { roleId });
        }

        
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}
