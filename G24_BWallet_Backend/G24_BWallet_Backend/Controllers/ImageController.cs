using G24_BWallet_Backend.Models.ObjectType;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
