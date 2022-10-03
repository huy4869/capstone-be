using Capstone_API.Models;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Capstone_API.Repository.Interface
{
    public interface ILoginRepository
    {

        public Account GetAccount(string phone, string password);
    }
}
