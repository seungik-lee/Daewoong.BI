﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IHostingEnvironment hostingEnvironment;
        public FileUploadController(IHostingEnvironment environment)
        {
            hostingEnvironment = environment;
        }

        [HttpPost]
        [Route("UploadAttachFiles")]
        public async Task<IActionResult> UploadAttachFiles(List<IFormFile> files)
        //public async Task<IActionResult> UploadAttachFiles([FromForm]List<IFormFile> files)
        {
            List<BusinessFile> results = new List<BusinessFile>();

            try
            {
                if (files == null || files.Count == 0)
                {
                    Response.StatusCode = 500;

                    return Content("업로드할 파일이 없습니다.");
                }

                // 10 * 1024 * 1024 => 10485760(10MB)
                foreach (var file in files)
                {
                    if (file.Length >= 10485760)
                    {
                        Response.StatusCode = 500;

                        return Content("업로드할 파일은 10MB를 넘길 수 없습니다.");
                    }
                }
            
                string uploadFilePath = $"{hostingEnvironment.WebRootPath}\\uploads\\{DateTime.Now.Year.ToString()}\\{DateTime.Now.Month.ToString().PadLeft(2, '0')}\\{DateTime.Now.Day.ToString().PadLeft(2, '0')}";
                //string uploadFilePath = $"/home/www/Daewoong.BI/WebApiJwt-master/wwwroot/uploads/{DateTime.Now.Year.ToString()}/{DateTime.Now.Month.ToString().PadLeft(2, '0')}/{DateTime.Now.Day.ToString().PadLeft(2, '0')}";

                if (!Directory.Exists(uploadFilePath))
                {
                    Directory.CreateDirectory(uploadFilePath);
                }

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var filePath = Path.Combine(uploadFilePath, Path.GetFileName(file.FileName));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        BusinessFile result = new BusinessFile();
                        result.FileName = Path.GetFileName(file.FileName);
                        result.FileSize = file.Length;
                        result.FileURL = filePath.Replace(hostingEnvironment.WebRootPath, "").Replace("\\", "/");
                        results.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return Ok(results);
        }
    }
}