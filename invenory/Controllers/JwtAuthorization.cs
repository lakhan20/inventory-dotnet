using invenory.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace invenory.Controllers
{

    public class JwtAuthorization
    {
        private IConfiguration _configuration=null;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtAuthorization(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;


        }
        public string GenerateToken(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {

                new Claim("user_id",user.user_id.ToString()),


               
              // new Claim(ClaimTypes.Email,user.user_email),
                //new Claim("password",user.user_password),
                //new Claim(ClaimTypes.GivenName,user.user_email),
                //new Claim(ClaimTypes.Name,user.user_name),
            };
            //var currentDateTime= DateTime.Now;
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: DateTime.Now.AddMinutes(5), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        //public UserModel authorizeUser()
        //{
        //    var identity = _httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;

        //    if (identity.IsAuthenticated) 
        //    {
        //        var userClaims = identity.Claims;
        //        return new UserModel
        //        {
        //            //user_name = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
        //          //  user_email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
        //            user_name = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value,

        //            //user_password = userClaims.ElementAt(1).Value,


        //        };


        //    }

        //    return null;

        //}

        public JwtSecurityToken  ValidateToken(IHeaderDictionary headers)
        
        {
            try
            {
                var token = headers["Authorization"].ToString().Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var expiry = Convert.ToInt64(jsonToken.Claims.ElementAt(1).Value);
                //DateTime expiry = Convert.ToDateTime(jsonToken.Claims.ElementAt(1).Value);
                var current_time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                if (current_time < expiry)
                {
                    return jsonToken;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public UserModel authorizeUser()
        {
            try
            {
                var headers = _httpContextAccessor.HttpContext.Request.Headers;
                
                JwtSecurityToken jsonToken=ValidateToken(headers);
                    if (jsonToken != null )
                {
                    var userClaims = jsonToken.Claims;
                    return new UserModel
                    {
                        user_id = Convert.ToUInt16(userClaims.ElementAt(0).Value),

                    };
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                return null ;
            }

        }

        }
}
