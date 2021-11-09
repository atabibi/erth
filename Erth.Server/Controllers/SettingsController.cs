using Erth.Server.Data;
using Erth.Shared;
using Erth.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erth.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles="admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public SettingsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetSettingsList()
        {
            try
            {
                var setting = await dbContext.Settings.ToListAsync();               

                return Ok(new TbActionResult<List<Setting>>
                {
                    Desc = $"درخواست موفقیت آمیز بود",
                    Object = setting,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return BadRequest(new TbActionResult<Exception>
                {
                    Desc = $"خطا: {err.Message}",
                    Object = err,
                    Success = false
                });
            }           
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public IActionResult GetValueOf([FromQuery]string label)
        {
            var setting = dbContext.Settings.SingleOrDefault( s => s.Label.ToUpper() == label.ToUpper());
            if (setting == null)
            {
                 return BadRequest(new TbActionResult<string> {
                    Desc = $" {label} در بانک اطلاعاتی: آیتم با این عنوان یافت نشد",
                    Object = label,
                    Success = false
                });
            }

            return  Ok(new TbActionResult<string> {
                    Desc = $"درخواست موفقیت آمیز بود",
                    Object = setting.Value,
                    Success = true
                });
        }

        [HttpPost("[action]")]        
        public async Task<IActionResult> SetValueOf([FromForm]string label, [FromForm]string value)
        {
            var setting = dbContext.Settings.SingleOrDefault( s => s.Label.ToUpper() == label.ToUpper());
            if (setting == null)
            {
                return BadRequest(new TbActionResult<Setting> {
                    Desc = $"خطا در بروزرسانی اطلاعات {label} در بانک اطلاعاتی: آیتم با این عنوان یافت نشد",
                    Object = setting,
                    Success = false
                });
            }

            try
            {
                setting.Value = value;
                dbContext.Update<Setting>(setting);
                var result = await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<Setting> {
                    Desc = $"{result} تغییر با موفقیت انجام شد",
                    Object = setting,
                    Success = true
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<Setting> {
                    Desc = $"خطا در بروزرسانی اطلاعات {label} در بانک اطلاعاتی: {err.Message}",
                    Object = setting,
                    Success = false
                });
            }
            
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateLabelAndValue([FromBody] Setting settingVal)
        {
            var setting = dbContext.Settings.SingleOrDefault(s => s.Id == settingVal.Id);
            if (setting == null)
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = $"خطا در بروزرسانی اطلاعات {settingVal.Id} در بانک اطلاعاتی: آیتم با این شماره یافت نشد",
                    Object = setting,
                    Success = false
                });
            }

            try
            {
                setting.Value = settingVal.Value;
                setting.Label = settingVal.Label;
                dbContext.Update<Setting>(setting);
                var result = await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<Setting>
                {
                    Desc = $"{result} تغییر با موفقیت انجام شد",
                    Object = setting,
                    Success = true
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = $"خطا در بروزرسانی اطلاعات {settingVal.Id} در بانک اطلاعاتی: {err.Message}",
                    Object = setting,
                    Success = false
                });
            }

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddNewSetting([FromBody] Setting newSetting)
        {
            if (string.IsNullOrWhiteSpace(newSetting.Label) || (string.IsNullOrWhiteSpace(newSetting.Value)))
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = "عنوان یا مقدار تنظیم نمی توانند تهی باشند",
                    Object = newSetting,
                    Success = false
                });
            }

            var setting = dbContext.Settings.SingleOrDefault(s => s.Label.ToUpper() == newSetting.Label.ToUpper());
            if (setting != null)
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = $"ایتم با عنوان {newSetting.Label} قبلا در بانک اطلاعاتی وجود دارد",
                    Object = newSetting,
                    Success = false
                });
            }

            try
            {
                newSetting.Id = dbContext.Settings.Max(s => s.Id) + 1;
                var result = await dbContext.Settings.AddAsync(newSetting);

                await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<Setting>
                {
                    Desc = $"{result.Entity.Label} با موفقیت ذخیره شد",
                    Object = setting,
                    Success = true
                });
            }
            catch (System.Exception err)
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = $"خطا در ذخیره سازی اطلاعات {newSetting.Label} در بانک اطلاعاتی: {err.Message}",
                    Object = setting,
                    Success = false
                });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> InsertLabelValue([FromForm]string label, [FromForm]string value)
        {
            var setting = dbContext.Settings.SingleOrDefault( s => s.Label.ToUpper() == label.ToUpper());
            if (setting != null)
            {
                return BadRequest(new TbActionResult<string> {
                    Desc = $"ایتم با عنوان {label} قبلا در بانک اطلاعاتی وجود دارد",
                    Object = label,
                    Success = false
                });
            }

            try
            {
                var result = await dbContext.Settings.AddAsync(new Setting {
                    Label = label,
                    Value = value
                 });

                await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<Setting> {
                    Desc = $"{result.Entity.Label} با موفقیت ذخیره شد",
                    Object = setting,
                    Success = true
                });
            }
            catch (System.Exception err)
            {
                 return BadRequest(new TbActionResult<Setting> {
                    Desc = $"خطا در ذخیره سازی اطلاعات {label} در بانک اطلاعاتی: {err.Message}",
                    Object = setting,
                    Success = false
                });
            }
        }
   
        [HttpGet("[action]")]
        public async Task<IActionResult> DeleteSetting(int id)
        {
            var setting = dbContext.Settings.SingleOrDefault(s => s.Id == id);
            if (setting == null)
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = $"خطا در حذف {id} در بانک اطلاعاتی: آیتم با این شماره یافت نشد",
                    Object = setting,
                    Success = false
                });
            }

            try
            {
                dbContext.Settings.Remove(setting);
                await dbContext.SaveChangesAsync();

                return Ok(new TbActionResult<Setting>
                {
                    Desc = $"{setting.Label} با موفقیت حذف شد",
                    Object = setting,
                    Success = true
                });
            }
            catch (Exception err)
            {
                return BadRequest(new TbActionResult<Setting>
                {
                    Desc = $"خطا در حذف اطلاعات {setting.Label} در بانک اطلاعاتی: {err.Message}",
                    Object = setting,
                    Success = false
                });
            }
        }
    }
}
