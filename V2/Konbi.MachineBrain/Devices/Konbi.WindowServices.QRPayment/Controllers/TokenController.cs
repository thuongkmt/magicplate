using Konbi.WindowServices.QRPayment.Configuration;
using Konbi.WindowServices.QRPayment.SelfHost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Konbi.WindowServices.QRPayment.Controllers
{

    public class TokenController : ControllerBase
    {
        private readonly IOptions<AuthConfiguration> Config;
        public TokenController(IOptions<AuthConfiguration> config)
        {
            Config = config;
        }

        [HttpGet]
        [AllowAnonymous]
        [ApiExplorerSettings(GroupName = "token")]
        [Route("token")]
        public IActionResult Token(string username, string password)
        {
            try
            {
                TokenObject result = null;
                if (username == string.Empty || password == string.Empty)
                {
                    return Unauthorized("Invalid username or password");
                }

                if (username == Config.Value.Username && password == Config.Value.Password)
                {
                    var claimsdata = new[] { new Claim(ClaimTypes.Name, username) };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.Value.SigningKey));
                    var signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
                    var token = new JwtSecurityToken(
                             issuer: Config.Value.Issuer,
                             audience: Config.Value.Audience,
                             expires: DateTime.Now.AddHours(12),
                             claims: claimsdata,
                             signingCredentials: signInCred
                        );
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var tokenString = tokenHandler.WriteToken(token);

                    result = new TokenObject()
                    {
                        Token = tokenString,
                        TokenType = "bearer",
                        ExpiresIn = (int)Math.Truncate((token.ValidTo.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                    };
                    return Ok(result);
                }
                return Unauthorized("Invalid username or password");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }


        [HttpPost]
        [ReadableBodyStream]
        [AllowAnonymous]
        [ApiExplorerSettings(GroupName = "token")]
        [Route("token")]
        public IActionResult Token()
        {
            try
            {
                var body = string.Empty;
                TokenObject result = null;
                using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
                {
                    body = stream.ReadToEnd();
                }

                if (body != null)
                {
                    var username = string.Empty;
                    var password = string.Empty;

                    var dataFromBody = body.Split("&");
                    var usernameData = dataFromBody.FirstOrDefault(x => x.StartsWith("username", StringComparison.OrdinalIgnoreCase));
                    var passwordData = dataFromBody.FirstOrDefault(x => x.StartsWith("password", StringComparison.OrdinalIgnoreCase));

                    if (usernameData == string.Empty || passwordData == string.Empty)
                    {
                        return Unauthorized("Invalid body data");
                    }

                    username = usernameData.Split("=")[1];
                    password = passwordData.Split("=")[1];

                    if (username == string.Empty || password == string.Empty)
                    {
                        return Unauthorized("Invalid username or password");
                    }

                    if (username == Config.Value.Username && password == Config.Value.Password)
                    {
                        var claimsdata = new[] { new Claim(ClaimTypes.Name, username) };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.Value.SigningKey));
                        var signInCred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
                        var token = new JwtSecurityToken(
                                 issuer: Config.Value.Issuer,
                                 audience: Config.Value.Audience,
                                 expires: DateTime.Now.AddHours(12),
                                 claims: claimsdata,
                                 signingCredentials: signInCred
                            );
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenString = tokenHandler.WriteToken(token);

                        result = new TokenObject()
                        {
                            Token = tokenString,
                            TokenType = "bearer",
                            ExpiresIn = (int)Math.Truncate((token.ValidTo.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds)
                        };
                        return Ok(result);
                    }
                }
                return BadRequest(null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
    }
}
