using Capstone_API.DBContexts;
using Capstone_API.Models;
using Capstone_API.Repository.Interface;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Capstone_API.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly MyDBContext _myDB;
       

        public LoginRepository(MyDBContext myDB)
        {
            _myDB = myDB;
        }

        public Account GetAccount(string phone, string password)
        {
            List<Account> list = _myDB.Accounts.ToList();
            Account account2 = list.Find(a =>
                a.PhoneNumber.Equals(phone) &&
                 a.Password.Equals(password)
            );
            return account2;
        }
    }
}
