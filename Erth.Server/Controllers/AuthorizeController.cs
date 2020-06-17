using Erth.Server.Data;
using Erth.Server.Models;
using Erth.Shared;
using Erth.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erth.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public AuthorizeController(
                        UserManager<ApplicationUser> userManager,
                        SignInManager<ApplicationUser> signInManager,
                        ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginParameters parameters)
        {
            var user = await _userManager.FindByNameAsync(parameters.UserName);
            if (user == null) return BadRequest("کاربر یافت نشد");
            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);
            if (!singInResult.Succeeded) return BadRequest("رمز عبور معتبر نیست");

            await _signInManager.SignInAsync(user, parameters.RememberMe);

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterParameters parameters)
        {
            var user = new ApplicationUser();
            user.UserName = parameters.UserName;
            var result = await _userManager.CreateAsync(user, parameters.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);

            return await Login(new LoginParameters
            {
                UserName = parameters.UserName,
                Password = parameters.Password
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangPassword(ChangePasswordVM changePassword)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var result = await _userManager.ChangePasswordAsync(user, changePassword.OldPassword, changePassword.NewPassword);

                return Ok(new TbActionResult<string>
                {
                    Success = result.Succeeded,
                    Desc = result.Succeeded ? "رمز عبور با موفقیت تغییر یافت" : "خطا در تغییر رمز عبور",
                    Object = result.Succeeded ? "رمز عبور با موفقیت تغییر یافت" : "خطا در تغییر رمز عبور"
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<string>
                {
                    Success = false,
                    Desc = $"خطا در هنگام تغییر رمز عبور {err.Message}",
                    Object = $"خطا در هنگام تغییر رمز عبور {err.Message}"
                });
            }


        }


        [HttpGet]
        public UserInfo UserInfo()
        {
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            return BuildUserInfo();
        }

        [Authorize(Roles="admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            List<UserInfo> usersInfos = new List<UserInfo>();

            try
            {
                var users = _dbContext.Users.Select(s => s);
                
                // آی دی نقش مدیر
                var adminRoleId = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "admin");
                
                // آی دی همه‌ی مدیران را استخراج کن
                var allAdmins = (await _dbContext.UserRoles.ToListAsync()).Where(w=>w.RoleId.ToString()==adminRoleId.Id.ToString()).ToList();
                       
                foreach (var u in users)
                {
                    usersInfos.Add(
                        new UserInfo
                        {
                            UserName = u.UserName,                            
                            IsAdmin = allAdmins.SingleOrDefault(admin => admin.UserId == u.Id) != null
                        }
                    );
                }

                return Ok(new TbActionResult<List<UserInfo>>() {
                    Desc = "مشخصات کاربران با موفقیت استخراج شد",
                    Success = true,
                    Object = usersInfos
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<List<UserInfo>> {
                    Desc = "خطا در بازیابی مشخصات کاربران" + ":" + err.ToString(),
                    Success = false,
                    Object = null
                });
            }




        }

        private UserInfo BuildUserInfo()
        {
            return new UserInfo
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                IsAdmin = User.IsInRole("admin"),
                ExposedClaims = User.Claims
                    //Optionally: filter the claims you want to expose to the client
                    //.Where(c => c.Type == "test-claim")
                    .ToDictionary(c => c.Type, c => c.Value)
            };
        }
    }
}
