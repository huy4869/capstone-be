using Capstone_API.Models;
using System;

namespace Capstone_API.IRepository
{
    public interface IManagerRepository
    {
        public string JWTGenerate(string phone, string pass);
        public bool CheckPhoneNumberExist(string phone);
    }
}
