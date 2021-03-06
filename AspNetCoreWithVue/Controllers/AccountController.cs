﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreWithVue.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreWithVue.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
    
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
          
        }


        [HttpPost]
        public async Task<JsonResult> Register([FromBody]RegistrationModel model)
        {
           
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email , Address = model.Address , Designation = model.Designation };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded && model.TokenAvailable == "null")
                {
                  
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                     {
                        new Claim("UserId", user.Id.ToString()),
                       
                     }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                    var token = tokenHandler.WriteToken(securityToken);
                    return Json(token);
                }else if(result.Succeeded && model.TokenAvailable != "null")
                {
                    return Json("User Registration successful");
                }
                
            }
            return Json("Your Email has Alrady Exist");
        }

        [HttpPost]
        public async Task<JsonResult> Login([FromBody]Login model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if(user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {   
               
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserId", user.Id.ToString()),
                       
                    }),

                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")),SecurityAlgorithms.HmacSha256Signature) 
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                
                return Json(token);
            }
            return Json("faild To logIn");
        }

        [HttpGet]
        [Authorize]
        public async Task<JsonResult> GetProfile()
        {
            String userId = User.Claims.First(c => c.Type == "UserId").Value;
            var user = await userManager.FindByIdAsync(userId);

            return Json(user);
        }

        

    }
}
