using Amazon.S3.Transfer;
using Amazon.S3;
using G24_BWallet_Backend.DBContexts;
using G24_BWallet_Backend.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System;
using Amazon;
using System.Threading.Tasks;
using System.Web;
using G24_BWallet_Backend.Models;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Twilio.Http;

namespace G24_BWallet_Backend.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly MyDBContext context;
        private readonly IConfiguration _configuration;
        public ImageRepository(MyDBContext myDB, IConfiguration _configuration)
        {
            this.context = myDB;
            this._configuration = _configuration;
        }

        public async Task<List<string>> SaveListIMGFile(string folder, IFormFileCollection files)
        {
            string fileName;
            List<string> list = new List<string>();
            foreach (var file in files)
            {
                fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + file.FileName;
                list.Add( await SaveIMMGFile(folder, file, fileName) );
            }
            return list;
        }

        public async Task<string> SaveIMMGFile(string folder, IFormFile file, string fileName)
        {
            if (file == null ||
                (!string.Equals(file.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(file.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(file.ContentType, "image/heic", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase)))
            {
                throw new IOException("file ảnh khống đúng định dạng");
            }

            string AWSS3AccessKeyId = _configuration["AWSS3:AccessKeyId"];
            string AWSS3SecretAccessKey = _configuration["AWSS3:SecretAccessKey"];

            using (var client = new AmazonS3Client(AWSS3AccessKeyId, AWSS3SecretAccessKey, RegionEndpoint.APSoutheast1))
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = fileName,
                        BucketName = "bwallets3bucket/"+folder
                    };
                    var fileTransferUtility = new TransferUtility(client);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }
            }

            return _configuration["AWSS3:ImgLink"] + folder + '/' + HttpUtility.UrlEncode(fileName);
        }

        public async Task<List<ProofImage>> AddIMGLinksDB(string ImageType, int modelId, List<string> links)
        {
            List<ProofImage> listAdd = new List<ProofImage>();
            foreach (var link in links)
            {
                listAdd.Add(new ProofImage
                {
                    ImageType = ImageType,
                    ModelId = modelId,
                    ImageLink = link
                });
            }

            await context.ProofImages.AddRangeAsync(listAdd);
            await context.SaveChangesAsync();

            return listAdd;
        }


        public bool CheckIMMGExists(string fileKey, string bucketName)
        {
            try
            {
                /*var client = new AmazonS3Client(AWSS3AccessKeyId, AWSS3SecretAccessKey, RegionEndpoint.APSoutheast1))
                var response = AmazonS3Client.GetObjectMetadata(new GetObjectMetadataRequest()
                   .WithBucketName(bucketName)
                   .WithKey(key));*/

                return true;
            }
            catch (Amazon.S3.AmazonS3Exception ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                //status wasn't not found, so throw the exception
                throw;
            }
        }


    }
}
