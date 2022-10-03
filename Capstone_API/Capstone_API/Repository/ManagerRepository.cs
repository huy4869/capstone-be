using Capstone_API.DBContexts;
using Capstone_API.IRepository;
using Capstone_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Twilio.Http;

namespace Capstone_API.Repository
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly MyDBContext _myDB;
        private readonly IConfiguration _configuration;

        public ManagerRepository()
        {
        }

        public ManagerRepository(MyDBContext myDB, IConfiguration configuration)
        {
            _myDB = myDB;
            _configuration = configuration;
        }

        public bool CheckPhoneNumberExist(string phone)
        {
            List<Account> list = _myDB.Accounts.ToList();
            Account account2 = list.Find(a =>
                a.PhoneNumber.Equals(phone)
            );
            return (account2 == null) ? false : true;
        }

        public string JWTGenerate(string phone, string? pass)
        {
            //create claims details based on the user information
            var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("PhoneNumber", phone),
                    new Claim("Password", pass)
                   };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddSeconds(60), signingCredentials: signIn);
            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenHandler;
        }
    }
}
