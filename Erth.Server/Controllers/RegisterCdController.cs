using System;
using System.Linq;
using System.Threading.Tasks;
using Erth.Server.Data;
using Erth.Server.Models;
using Erth.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Erth.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    [AllowAnonymous]
    public class RegisterCdController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;

        public RegisterCdController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(RegisterCdVM registerVM)
        {
            try
            {
                // اول چک کن که سی دی با این برچسب در بانک اطلاعاتی هست یا نه
                var cdlable = await dbContext.CdLabels.SingleOrDefaultAsync(cd => cd.Label.Trim() == registerVM.CdLabel.ToUpper().Trim());
                if (cdlable == null)
                {
                    return BadRequest(new TbActionResult<RegisterCdVM>
                    {
                        Success = false,
                        Object = registerVM,
                        Desc = $"خطا! سی دی با برچسب {registerVM.CdLabel} فروخته نشده است"
                    });
                }

                // دوم چک کن که نوع سی دی در بانک اطلاعاتی همان است که کاربر گفته است:
                if (cdlable.TypeErth != int.Parse(registerVM.SoftwareType))
                {
                    return BadRequest(new TbActionResult<RegisterCdVM>
                    {
                        Success = false,
                        Object = registerVM,
                        Desc = $"نسخه نرم افزار را درست انتخاب نکرده اید"
                    });
                }

                // سوم چک کن که تعداد درخواستهای ثبت شده با این برچسب بیشتر از حد مجاز نباشد           
                int limitedCount = configuration.GetValue<int>("TbSettings:MaxLimitToRegisterCdLabel");

                var n = await dbContext.RegisteredLabels.Include(r => r.CdLabel).Where(cd => cd.CdLabel.Label == cdlable.Label).CountAsync();
                if (n > limitedCount)
                {
                    return BadRequest(new TbActionResult<RegisterCdVM>
                    {
                        Success = false,
                        Object = registerVM,
                        Desc = $"شما حداکثر {limitedCount} کد فعالسازی می توانید دریافت کنید در حالیکه تاکنون {n} کد دریافت کرده اید."
                    });
                }

                // حالا باید عملیات ایجاد کد و ثبت آن در بانک اطلاعاتی را انجام دهی

                //Todo: ایجاد کد
                var softwareType = (CdTypeErth) int.Parse(registerVM.SoftwareType);
                var activatedNumber = Utility.CreateActiveCode(registerVM.Sn, softwareType);

                // ثبت در بانک اطلاعاتی

                var valNew = await dbContext.RegisteredLabels.AddAsync( new RegistredLabel {
                    CdLabel = cdlable,
                    City = registerVM.City,
                    United = registerVM.United,
                    DateOf = DateTime.Now,
                    FullName = registerVM.FullName,
                    Shobeh = (CdTypeErth)(int.Parse(registerVM.SoftwareType)) == CdTypeErth.Pro ?  registerVM.Shobeh : "-",
                    UserPcSN = registerVM.Sn                    
                });

                var nAdded = await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<RegisterCdVM>
                {
                    Success = true,
                    Object = registerVM,
                    Desc = $"کد فعالسازی: ===> {activatedNumber} <===" + "\n\r" + 
                           $"با این برچسب تاکنون {n+1} کد فعالسازی گرفته شده است." + 
                           $" امکان دریافت {limitedCount - n - 1} کد دیگر وجود دارد."
                });



            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<RegisterCdVM>
                {
                    Success = false,
                    Object = registerVM,
                    Desc = $"خطا در انجام عملیات. لطفا با شماره ۰۹۱۳۲۷۰۸۳۴۱ تماس بگیرید. {err.Message}"
                });
            }
        }
    }

}