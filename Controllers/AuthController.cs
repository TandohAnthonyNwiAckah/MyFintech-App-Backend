#nullable enable
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8629 // Nullable value type may be null.
namespace TANA.Controller
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using TANA.Data;
    using TANA.Helpers;
    using TANA.Models;

    /// <summary>
    /// Auth Controller
    /// </summary>
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly AppDBContext DB;
        private readonly EmailHelper Mailer;
        private readonly SmsHelper Sms;
        private IConfiguration Config;
        private readonly IMapper Mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IHttpContextAccessor HttpAccessor;

        public AuthController(
            AppDBContext dbContext,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration Configuration,
            EmailHelper mailer,
            SmsHelper sms
        )
            : base(dbContext, httpContextAccessor, environment, Configuration)
        {
            Config = Configuration;
            DB = dbContext;
            hostEnvironment = environment;
            HttpAccessor = httpContextAccessor;
            Mailer = mailer;
            Sms = sms;
            Mapper = mapper;
        }

        /// <summary>
        /// Authenticate and login user
        /// </summary>
        /// <param name="modeldata">Mapped Form Data to LoginModel.</param>
        /// <returns>JSON Object of Users record</returns>
        [HttpPost]
        [Route("api/auth/login")]
        public ActionResult Login([FromBody] LoginModel modeldata)
        {
            var username = modeldata.Username;
            var password = modeldata.Password;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = DB
                    .Users.Where(p => p.phone == username || p.email == username)
                    .FirstOrDefault();
                if (user == null)
                { //user not found
                    // return Unauthorized("Username or password not correct");

                    return Ok(new { code = "401", message = "Username or password not correct" });
                }

                if (!Hash.Verify(user.password, password))
                { //password not correct
                    return Ok(new { code = "401", message = "password not correct" });
                }

                SaveAuditLog(user.id.ToString());

                var userLoginData = GetUserLoginData(user);

                return Ok(userLoginData);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Save new user record
        /// </summary>
        /// <returns>JSON Object of Users record</returns>
        [HttpPost]
        [Route("api/auth/register")]
        public async Task<IActionResult> Register([FromBody] UsersRegisterDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var modeldata = new Users();
                modeldata.firstname = postdata.firstname;
                modeldata.middlename = postdata.middlename;
                modeldata.surname = postdata.surname;
                modeldata.email = postdata.email;
                modeldata.phone = postdata.phone;
                modeldata.photo = postdata.photo;
                modeldata.password = postdata.password;
                modeldata.date_created = DateTime.Now;
                modeldata.date_updated = DateTime.Now;

                // move uploaded files from temporary directory
                if (modeldata.photo != null)
                {
                    var fileInfo = await this.MoveUploadedFiles(modeldata.photo, "photo");
                    modeldata.photo = fileInfo.filepath;
                }
                modeldata.password = Hash.ComputeHash(postdata.password);

                // assign default role to user
                var roleId = DB
                    .Roles.Where(p => p.role_name == "User")
                    .Select(p => p.role_id)
                    .FirstOrDefault();
                modeldata.user_role_id = roleId;

                DB.Users.Add(modeldata);
                var audit = GetAuditLog();
                DB.SaveChanges(audit);

                var user = modeldata; // newly created user

                // Generate 6 digit OTP
                var otpCode = new Random().Next(100000, 999999).ToString();
                var otp = new Otp
                {
                    email = user.email,
                    phone = user.phone,
                    otp_code = otpCode,
                    date_created = DateTime.Now,
                };
                DB.Otp.Add(otp);
                DB.SaveChanges();

                // Send OTP to email and phone
                try
                {
                    var subject = "Account Verification OTP";
                    ViewBag.sitename = Config["SITE_NAME"];
                    ViewBag.otpCode = otpCode;
                    ViewBag.username = user.firstname;
                    var message = await this.RenderViewAsync("VerifyOtpTemplate", "");

                    var userEmail = user.email;
                    var userPhone = user.phone;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            /// SEND EMAIL TO USER EMAIL ADDRESS
                            Mailer.SendEmail(userEmail, subject, message);

                            /// SEND SMS TO USER PHONE NUMBER
                            var smsMessage = $"Your verification code is {otpCode}";
                            await Sms.SendSms(userPhone, smsMessage);
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error in background sending OTP: {innerEx.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    // Log error but don't fail registration
                    Console.WriteLine($"Error rendering OTP view: {ex.Message}");
                }

                var userLoginData = GetUserLoginData(user);

                return Ok(userLoginData);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Verify user OTP
        /// </summary>
        [HttpPost]
        [Route("api/auth/verifyotp")]
        public IActionResult VerifyOtp([FromBody] OtpVerifyDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var otp = DB
                    .Otp.Where(p =>
                        (p.email == postdata.username || p.phone == postdata.username)
                        && p.otp_code == postdata.otp_code
                        && !p.is_verified
                    )
                    .OrderByDescending(p => p.date_created)
                    .FirstOrDefault();

                if (otp == null)
                {
                    return BadRequest(new { message = "Invalid or expired OTP" });
                }

                // Check if OTP is expired (5 minutes duration)
                // var duration = Config.GetValue<int>("OTP_DURATION", 5);
                // if (otp.date_created.AddMinutes(duration) < DateTime.Now)
                // {
                //     return BadRequest(new { message = "OTP has expired" });
                // }

                otp.is_verified = true;
                DB.Otp.Update(otp);
                DB.SaveChanges();

                return Ok(new { message = "OTP verified successfully", code = "200" });
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Retry user OTP
        /// </summary>
        [HttpPost]
        [Route("api/auth/retryotp")]
        public async Task<IActionResult> RetryOtp([FromBody] OtpRetryDTO postdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = DB
                    .Users.Where(p => p.phone == postdata.username || p.email == postdata.username)
                    .FirstOrDefault();
                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                // Generate new 6 digit OTP
                var otpCode = new Random().Next(100000, 999999).ToString();
                var otp = new Otp
                {
                    email = user.email,
                    phone = user.phone,
                    otp_code = otpCode,
                    date_created = DateTime.Now,
                };
                DB.Otp.Add(otp);
                DB.SaveChanges();

                try
                {
                    var subject = "Account Verification OTP";
                    ViewBag.sitename = Config["SITE_NAME"];
                    ViewBag.otpCode = otpCode;
                    ViewBag.username = user.firstname;
                    var emailMessage = await this.RenderViewAsync("VerifyOtpTemplate", "");

                    var userEmail = user.email;
                    var userPhone = user.phone;
                    var smsMessage = $"Your verification code is {otpCode}";

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            Mailer.SendEmail(userEmail, subject, emailMessage);
                            await Sms.SendSms(userPhone, smsMessage);
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error in background sending retry OTP: {innerEx.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error rendering retry OTP view: {ex.Message}");
                }

                return Ok(new { message = "OTP sent successfully", code = "200" });
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Send password reset link to user email
        /// </summary>
        /// <param name="modeldata">Mapped Form Data to ForgotPassword Model.</param>
        /// <returns>Success message</returns>
        [HttpPost]
        [Route("api/auth/forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPassword modeldata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var email = modeldata.Email;
                var user = DB.Users.Where(p => p.email.Equals(email)).FirstOrDefault();

                if (user == null)
                {
                    return Ok(new { code = "404", message = "Email not registered" });
                }

                var token = GenerateUserToken(user);
                var siteAddr = Config["FRONTEND_URL"];
                var resetlink = $"{siteAddr}index/resetpassword?token={token}";
                var sitename = Config["SITE_NAME"];

                ViewBag.username = user.surname;
                ViewBag.sitename = Config["SITE_NAME"];
                ViewBag.resetlink = resetlink;

                var mailtitle = "Password reset";
                var mailbody = await this.RenderViewAsync("PasswordResetTemplate", "");
                
                var targetEmail = email;
                _ = Task.Run(() =>
                {
                    try
                    {
                        Mailer.SendEmail(targetEmail, mailtitle, mailbody);
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Error in background sending reset password: {innerEx.Message}");
                    }
                });
                SaveAuditLog(user.id.ToString());

                return Ok(
                    new { code = "200", message = "We have emailed your password reset link!" }
                );
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        /// <param name="id">Reset link token.</param>
        /// <param name="modeldata">Mapped Form Data to ForgotPassword Model.</param>
        /// <returns>Success message</returns>
        [HttpPost]
        [Route("api/auth/resetpassword")]
        public ActionResult ResetPassword([FromBody] ResetPassword modeldata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userid = GetUserIDFromJwt(modeldata.Token);
                var user = DB.Users.Where(p => p.id.ToString().Equals(userid)).FirstOrDefault();
                ;

                if (user == null)
                {
                    return BadRequest("Invalid Token");
                }
                user.password = Hash.ComputeHash(modeldata.Password);
                DB.Update(user);
                DB.SaveChanges();
                SaveAuditLog(user.id.ToString());
                return Ok(new { code = "200", message = "Password reset completed" });
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        /// <summary>
        /// Return user login data
        /// </summary>
        /// <param name="user">User data.</param>
        /// <returns>User data and token</returns>
        private object GetUserLoginData(Users user)
        {
            var token = GenerateUserToken(user);
            var code = "201";
            var message = "Success";
            return new
            {
                token,
                code,
                message,
            };
        }

        /// <summary>
        /// Generate JWT token
        /// </summary>
        /// <param name="user">User data.</param>
        /// <param name="expiresInMins">Token expiration duration.</param>
        /// <returns>JWT</returns>
        private string GenerateUserToken(Users user)
        {
            var expiresInMins = double.Parse(Config["Jwt:Duration"]);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, Config["Jwt:Subject"]),
                new Claim(ClaimTypes.Name, user.id.ToString()),
            };
            claims.Add(new Claim(ClaimTypes.Role, user.user_role_id.ToString()));

            var token = new JwtSecurityToken(
                Config["Jwt:Issuer"],
                Config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(expiresInMins),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="jwt">Jwt string to validate.</param>
        /// <returns>Valid token data</returns>
        private JwtSecurityToken ValidateToken(string jwt)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config["Jwt:Key"]));
            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                IssuerSigningKey = securityKey,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudience = Config["Jwt:Issuer"],
                ValidateIssuer = true,
                ValidIssuer = Config["Jwt:Issuer"],
            };
            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                    jwt,
                    validationParameters,
                    out var rawValidatedToken
                );
                return (JwtSecurityToken)rawValidatedToken;
            }
            catch (SecurityTokenValidationException stvex)
            {
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                throw new Exception($"Token was invalid: {argex.Message}");
            }
        }

        /// <summary>
        /// Get userid from jwt token
        /// </summary>
        /// <param name="jwt">Raw Jwt string.</param>
        /// <returns>user id value</returns>
        private string GetUserIDFromJwt(string jwt)
        {
            var jwtData = ValidateToken(jwt); // token from the id param
            var userid = jwtData.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            return userid;
        }
    }
}
