using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository imageRepo;
        //private readonly IUserR

        public ImageController(IImageRepository InitImageRepo)
        {
            imageRepo = InitImageRepo;
        }

        [HttpPost("{folder}")]
        public async Task<Respond<string>> SaveIMG(string folder, [FromForm] IFormFile imgFile)
        {
            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            string fileName = VNDateTime.ToString("yyyyMMddHHmmss") + imgFile.FileName;
            var imglinks = imageRepo.SaveIMMGFile(folder, imgFile, fileName);

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "lưu ảnh thành công",
                Data = await imglinks
            };
        }

        [HttpPost("base64/convertFile")]
        public async Task<Respond<Base64Data>> ConvertFileToBase64 ([FromForm] IFormFile imgFile)
        {
            var ms = new MemoryStream();
            imgFile.CopyTo(ms);
            var fileBytes = ms.ToArray();
            string s = Convert.ToBase64String(fileBytes);

            return new Respond<Base64Data>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "",
                Data = new Base64Data
                {
                    FileName = imgFile.FileName,
                    Folder = "user",
                    Base64String = s
                }
            };
        }

        [HttpPost("base64/savefile")]
        public async Task<Respond<string>> SaveIMGBase64(Base64Data base64Data)
        {
            DateTime VNDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            base64Data.FileName = VNDateTime.ToString("yyyyMMddHHmmss") + base64Data.FileName;
            var imglinks = imageRepo.SaveIMGBase64(base64Data.Folder, base64Data.Base64String, base64Data.FileName);

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "Lưu ảnh thành công",
                Data = await imglinks
            };
        }

        [HttpDelete()]
        public async Task<Respond<string>> DeleteIMGS(ListURL deleteList)
        {
            foreach (string url in deleteList.listUrl)
            {
                await imageRepo.DeleteS3FileByLink(url);
            }

            return new Respond<string>()
            {
                StatusCode = HttpStatusCode.Accepted,
                Error = "",
                Message = "xóa ảnh thành công",
                Data = null
            };
        }
    }
}
