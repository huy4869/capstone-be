using G24_BWallet_Backend.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G24_BWallet_Backend.Repository.Interface
{
    public interface IImageRepository
    {
        Task<string> SaveIMMGFile(string folder, IFormFile file, string fileName);

        Task<List<ProofImage>> AddIMGLinksDB(string ImageType, int modelId, List<string> links);

        Task<string> SaveIMGBase64(string folder, string base64, string fileName);

        Task<string> DeleteS3FileByLink(string link);
    }
}
