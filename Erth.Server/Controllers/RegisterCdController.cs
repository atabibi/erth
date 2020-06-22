using System;
using System.Collections.Generic;
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
    [Route("api/[controller]")]    
    public class RegisterCdController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;

        public RegisterCdController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpGet("[action]")]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> RegisteredCds(RegisteredCdType cdType, 
                                                        OrderBy orderBy,
                                                        int from, 
                                                        int count=10,
                                                        bool onlyCount=false)
        {            
            try
            {
                int cdTypeId = (int)cdType % 2;
                var cds = dbContext.RegisteredLabels.Include(r => r.CdLabel)
                    .Where(r => r.CdLabel.TypeErth == cdTypeId);

                if (onlyCount) // فقط تعداد کل رکوردها را بازگردان
                {
                    int allRecordsCount = await cds.CountAsync();
                    return Ok(new TbActionResult<int> 
                        {
                            Success = true,
                            Desc = $"تعداد رکوردها: {allRecordsCount}",
                            Object = allRecordsCount
                        });
                }
                
                switch (orderBy)
                {
                    case OrderBy.Date:
                        cds = cds.OrderBy(r => r.DateOf);
                        break;
                    case OrderBy.DateInverse:
                        cds = cds.OrderByDescending(r => r.DateOf);                        
                        break;
                    case OrderBy.United:
                        cds = cds.OrderBy(r => r.United);
                        break;
                    case OrderBy.UnitedInverse: 
                        cds = cds.OrderByDescending(r => r.United);               
                        break;
                }

                cds = count == -1 ? cds.Skip(from) : cds.Skip(from).Take(count);
                
                var results = await cds.Select( s => new RegisterCdVMwithDate {
                            CdLabel = s.CdLabel.Label,
                            City = s.City,
                            DateRegistred = s.DateOf.ToShamsi(),
                            FullName = s.FullName,
                            Shobeh = cdType == (int)CdTypeErth.Pro ? s.Shobeh : "-",
                            Sn = s.UserPcSN,
                            United = s.United
                        }).ToListAsync();

                return Ok(new TbActionResult<List<RegisterCdVMwithDate>> {
                    Success = true,
                    Desc = "لیست سی‌دی‌های ثبت شده با موفقیت بازیابی شد",
                    Object = results
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<RegisterCdVM>
                {
                    Success = false,
                    Object = null,
                    Desc = $"خطا در انجام عملیات.. {err.Message}"
                });
            }
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

        [HttpPost("[action]")]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> RegisterLabel(NewCdLabelVM newCdLabel)
        {
            try
            {
                // اولین لیبلی که فیلدهای
                // CustomerName and CustomerUnited
                // آن خالی باشد 
                // و در عین حال هیچ کد فعالسازی تاکنون دریافت نکرده باشد
                // را باز گردان
                var lbl = await  dbContext.CdLabels.Include(l=>l.RegisteredLabels)
                    .FirstOrDefaultAsync(
                        l =>
                            l.TypeErth == (int)newCdLabel.TypeErth &&    
                            l.RegisteredLabels.Count == 0 && 
                            string.IsNullOrEmpty(l.CustomerName) &&
                            string.IsNullOrEmpty(l.CustomerUnited)
                        );
                
                if (lbl == null)
                {
                    return BadRequest(new TbActionResult<string>
                    {
                        Success = false,
                        Object = "UnKnown",
                        Desc = "لیبل خالی یافت نشد!"
                    });
                }
                
                lbl.CustomerName = newCdLabel.CustomerFullName.Trim();
                lbl.CustomerUnited = newCdLabel.CustomerUnited.Trim();

                dbContext.CdLabels.Update(lbl);
                await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<NewCdLabelVM>
                {
                    Success = true,
                    Desc =  $"برچسب {lbl.Label} به نام {lbl.CustomerName} از استان {lbl.CustomerUnited} ثبت شد",
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<string>
                {
                    Success = false,
                    Object = err.ToString(),
                    Desc = $"خطا در انجام عملیات. {err.Message}"
                });
            }
        }
    
        [HttpGet("[action]")]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> GetDedicatedLabels(CdTypeErth cdTypeErth,int from=0, int count=10, bool countOnly= false)
        {
            try
            {
                var labels = dbContext.CdLabels.Include(l => l.RegisteredLabels)
                            .Where( l => 
                                    l.TypeErth == (int)cdTypeErth &&
                                    (
                                        string.IsNullOrEmpty(l.CustomerName) == false ||
                                        string.IsNullOrEmpty(l.CustomerUnited) == false ||
                                        l.RegisteredLabels.Count > 0
                                    )
                                )
                                .OrderBy(l => l.Label);

                if (countOnly)        
                {
                    var n = await labels.CountAsync();
                    return Ok( new TbActionResult<int> {
                        Success = true,
                        Object = n,
                        Desc = $"تعداد کل سی‌دی‌های اختصاص یافته: {n}"
                    });
                }

                var result = await labels.Skip(from).Take(count).Select (
                        l => new DedicatedLablsVM {
                            FullName = l.CustomerName,
                            Id = l.Id,
                            Label = l.Label,
                            RegisteredCount = l.RegisteredLabels.Count,
                            United = l.CustomerUnited
                        }
                    ).ToListAsync();
                
                return Ok(new TbActionResult<List<DedicatedLablsVM>> {
                        Success = true,
                        Object = result,
                        Desc = "با موفقیت بازیابی شد"
                    });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<DedicatedLablsVM>
                {
                    Success = false,
                    Object = null,
                    Desc = $"خطا در انجام عملیات.. {err.Message}"
                });
            }
        }
    }

}