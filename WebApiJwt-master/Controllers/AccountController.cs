using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Daewoong.BI.Datas;
using Daewoong.BI.Helper;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Daewoong.BI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        string fineKey = "~gw-biadvancement-SecretKey";
        string fineIV = "~gw-biadvancement-InitVector";

        string biKey = "~gw-biportal-SecretKey";
        string biIV = "~gw-biportal-InitVector";

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        private Task<IdentityUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        [HttpPost]
        public DWBIUser Login([FromBody] LoginDto model)
        {
           
            //RegistAll();
           
            //Register("wkhong93@bears.co.kr", "dwbi11!!");
            //return null;

            
            if (!model.Email.Contains("@"))
            {
                //string dEmail = HttpUtility.UrlDecode(model.Email);
                model.Email = model.Email;
                string loginID = Encryptor.Decrypt(model.Email, biKey, biIV);
                if (string.IsNullOrEmpty(loginID))
                    return null;

                model.Email = loginID;
                model.Password = "dwbi11!!";
            }
            try
            {

                //UserController uc = new UserController();
                //20200109 김태규 수정 배포
                UserController uc = new UserController(null, null, null);
                ApplicationDbContext context = new ApplicationDbContext();
                var result = _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false).Result;

                if (result.Succeeded)
                {
                    var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                    DWBIUser userInfo = uc.GetByKey(model.Email, Request);
                    userInfo.Token = GenerateJwtToken(model.Email, appUser).Result;
                    var user = GetCurrentUserAsync().Result;
                    Encryptor ec1 = new Encryptor(fineKey, fineIV);
                    Encryptor ec2 = new Encryptor(fineKey, fineIV);
                    userInfo.key = ec1.Encrypt(model.Email.Split('@')[0]);
                    userInfo.RoleIDKey = ec2.Encrypt(userInfo.RoleID);


                    //ClaimsPrincipal currentUser = this.User;
                    //var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                    //var user2 = await _userManager.FindByNameAsync(currentUserName);

                    return userInfo;

                }

            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine( ">>>>"+ ex.InnerException.Message + ex.Message);
                return new DWBIUser
                {
                    Token = ex.InnerException.Message + ex.Message
                };
                
            }
           // finally { }
            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
            

        }
       
        [HttpPost]
        //public async Task<object> Register([FromBody] RegisterDto model)

		//2019-12-26 김태규 수정 배포
        [HttpDelete]

        public async void DeleteUser(string email)
        {
            IdentityUser user = new IdentityUser();
            user.Email = email;
            await _userManager.DeleteAsync(user);
        }

        public async Task<object> Register([FromBody] DWBIUser model)
        {
            var user = new IdentityUser
            {
                UserName = model.UserID, 
                Email = model.UserID
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                //UserController uc = new UserController();
                //20200109 김태규 수정 배포
                UserController uc = new UserController(null, null, null);
                uc.SaveUser(model);
                return true;
            }
            
            throw new ApplicationException("UNKNOWN_ERROR");
        }

//20200109 김태규 수정 배포
/*
        public bool Register(string email, string password)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };
            var result = _userManager.CreateAsync(user, password).Result;

            if (result.Succeeded)
            {
                return true;
            }

            return false;
        }

        private void RegistAll()
        {
            Register("sjw1969@daewoong.co.kr", "dwbi11!!");
            Register("2040131@daewoong.co.kr", "dwbi11!!");
            Register("yoonjs@daewoong.co.kr", "dwbi11!!");
            Register("younjc@daewoong.co.kr", "dwbi11!!");
            Register("gyn05@daewoong.co.kr", "dwbi11!!");
            Register("leejw@daewoong.co.kr", "dwbi11!!");
            Register("stomeve@daewoong.co.kr", "dwbi11!!");
            Register("hsj1222@daewoong.co.kr", "dwbi11!!");
            Register("yangnest@daewoong-bio.co.kr", "dwbi11!!");
            Register("pyh@daewoong.co.kr", "dwbi11!!");
            Register("sjw1969@daewoong.co.kr", "dwbi11!!");
            Register("skpark@hanall.co.kr", "dwbi11!!");
            Register("sylim@hanall.co.kr", "dwbi11!!");
            Register("2150227@daewoong.co.kr", "dwbi11!!");
            Register("kbkim011@daewoong.co.kr", "dwbi11!!");
            Register("gyn05@daewoong.co.kr", "dwbi11!!");
            Register("hongjs@daewoong.co.kr", "dwbi11!!");
            Register("pyh@daewoong.co.kr", "dwbi11!!");
            Register("jhkwon215@daewoong.co.kr", "dwbi11!!");
            Register("yhjang530@daewoong.co.kr", "dwbi11!!");
            Register("jaihag@daewoong.co.kr", "dwbi11!!");
            Register("night525@daewoong.co.kr", "dwbi11!!");
            Register("cji1@daewoong.co.kr", "dwbi11!!");
            Register("jyyoon004@daewoong.co.kr", "dwbi11!!");
            Register("yann@daewoong.co.kr", "dwbi11!!");
            Register("hyun-jin.park@daewoong.co.kr", "dwbi11!!");
            Register("pjh9274@daewoong.co.kr", "dwbi11!!");
            Register("yangkiho@daewoong.co.kr", "dwbi11!!");
            Register("2080004@daewoong.co.kr", "dwbi11!!");
            Register("2030273@daewoong.co.kr", "dwbi11!!");
            Register("terr3@daewoong.co.kr", "dwbi11!!");
            Register("yjch1214@daewoong.co.kr", "dwbi11!!");
            Register("1970433@daewoong.co.kr", "dwbi11!!");
            Register("ssung67@daewoong.co.kr", "dwbi11!!");
            Register("redrooster@daewoong.co.kr", "dwbi11!!");
            Register("area202@daewoong.co.kr", "dwbi11!!");
            Register("2010115@daewoong.co.kr", "dwbi11!!");
            Register("night525@daewoong.co.kr", "dwbi11!!");
            Register("yum7711@daewoong-bio.co.kr", "dwbi11!!");
            Register("jsgon@daewoong-bio.co.kr", "dwbi11!!");
            Register("leckw@daewoong-bio.co.kr", "dwbi11!!");
            Register("shpark97@daewoong-bio.co.kr", "dwbi11!!");
            Register("bkson@hanall.co.kr", "dwbi11!!");
            Register("yslee309@daewoong.co.kr", "dwbi11!!");
            Register("shin@hanall.co.kr", "dwbi11!!");
            Register("jklee77@hanall.co.kr", "dwbi11!!");
            Register("diglers@hanall.co.kr", "dwbi11!!");
            Register("hayasi05@daewoong.co.kr", "dwbi11!!");
            Register("yuji@daewoong.co.kr", "dwbi11!!");
            Register("kbs1002@daewoong.co.kr", "dwbi11!!");
            Register("ihkim010@daewoong.co.kr", "dwbi11!!");
            Register("jhsung022@daewoong.co.kr", "dwbi11!!");
            Register("letup@daewoong.co.kr", "dwbi11!!");
            Register("cuyer@daewoong.co.kr", "dwbi11!!");
            Register("shpark079@daewoong.co.kr", "dwbi11!!");
            Register("kkoon@daewoong.co.kr", "dwbi11!!");
            Register("1980654@daewoong.co.kr", "dwbi11!!");
            Register("jangmook.lee@daewoong.co.kr", "dwbi11!!");
            Register("dcma081@daewoong.co.kr", "dwbi11!!");
            Register("univerjuny@daewoong.co.kr", "dwbi11!!");
            Register("ek0831@daewoong.co.kr", "dwbi11!!");
            Register("yclee208@daewoong.co.kr", "dwbi11!!");
            Register("9501003@daewoong.co.kr", "dwbi11!!");
            Register("bslee054@daewoong.co.kr", "dwbi11!!");
            Register("hunsang@daewoong.co.kr", "dwbi11!!");
            Register("jhpark298@daewoong.co.kr", "dwbi11!!");
            Register("yoonkyoung@daewoong.co.kr", "dwbi11!!");
            Register("hwlee160@daewoong.co.kr", "dwbi11!!");
            Register("hwangmk@daewoong.co.kr", "dwbi11!!");
            Register("skkim156@daewoong.co.kr", "dwbi11!!");
            Register("boruem0223@daewoong.co.kr", "dwbi11!!");
            Register("yum7711@daewoong-bio.co.kr", "dwbi11!!");
            Register("kylee@daewoong-bio.co.kr", "dwbi11!!");
            Register("junseo7@daewoong-bio.co.kr", "dwbi11!!");
            Register("y2k8495@daewoong-bio.co.kr", "dwbi11!!");
            Register("nano9@daewoong-bio.co.kr", "dwbi11!!");
            Register("coralboy@hanall.co.kr", "dwbi11!!");
            Register("choipk@hanall.co.kr", "dwbi11!!");
            Register("admhkim@hanall.co.kr", "dwbi11!!");
            Register("bkson@hanall.co.kr", "dwbi11!!");
            Register("sjchoi@hanall.co.kr", "dwbi11!!");
            Register("namoosh@hanall.co.kr", "dwbi11!!");
            Register("parkdh@hanall.co.kr", "dwbi11!!");
            Register("syhwang@hanall.co.kr", "dwbi11!!");
            Register("pansuk@hanall.co.kr", "dwbi11!!");
            Register("jangseob@hanall.co.kr", "dwbi11!!");
            Register("dsdo@hanall.co.kr", "dwbi11!!");
            Register("yssong@hanall.co.kr", "dwbi11!!");
            Register("kbchoi@hanall.co.kr", "dwbi11!!");
            Register("echobok@daewoong.co.kr", "dwbi11!!");
            Register("is.shim@daewoong.co.kr", "dwbi11!!");
            Register("shaiel48@daewoong.co.kr", "dwbi11!!");
            Register("smkim165@daewoong.co.kr", "dwbi11!!");
            Register("echobok@daewoong.co.kr", "dwbi11!!");
        }
*/
        [Authorize]
        [HttpGet]
        public async Task<object> Protected()
        {
            return "Protected area";
        }
        
        private async Task<object> GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public String AESEncrypt128(String Input, String key)
        {

            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(Input);
            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(PlainText, 0, PlainText.Length);
            cryptoStream.FlushFinalBlock();

            byte[] CipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string EncryptedData = Convert.ToBase64String(CipherBytes);

            return EncryptedData;
        }

        [DataContract]

        public class LoginDto
        {
            [Required]
            [DataMember]
            public string Email { get; set; }

            [Required]
            [DataMember]
            public string Password { get; set; }

        }

        [DataContract]
        public class RegisterDto
        {
            [Required]
            [DataMember]
            public string Email { get; set; }

            [Required]
            [DataMember]
            [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
            public string Password { get; set; }
        }


    }
}