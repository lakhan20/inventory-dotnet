using invenory.Models;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace invenory.Controllers
{
    //[ApiController]
    public class UserController : Controller
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        JwtAuthorization jwt;
        public UserController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            jwt = new JwtAuthorization(configuration, _httpContextAccessor);
            connectionString = configuration["ConnectionStrings:PostgresDb"] ?? "";
        }
        [AllowAnonymous]
        [HttpPost]

        [Route("api/Login")]

        public ActionResult Login( [FromBody] UserModel userModel)
        {
            var user_id = 0;
            //List<UserModel> users = new List<UserModel>();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string loginQuery = "SELECT * FROM public.fn_login(@user_email, @user_password)";
                    using (var command = new NpgsqlCommand(loginQuery, connection))
                    {
                        command.Parameters.AddWithValue("@user_email", userModel.user_email);
                        command.Parameters.AddWithValue("@user_password", userModel.user_password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user_id = reader.GetInt32(0); // Assuming the function returns the user_id as the first field
                                Console.WriteLine("user_id : " + user_id);

                                //UserModel user = new UserModel();
                                userModel.user_id = user_id;

                                // Assuming these are the other fields returned by fn_login
                                userModel.user_name = reader.GetString(1);
                                userModel.user_email = reader.GetString(2);
                                userModel.user_password = reader.GetString(3);

                                //users.Add(userModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", "it's exception: " + ex);
            }
            if (userModel.user_name!=null && userModel.user_email!=null)
            {
             var  token = jwt.GenerateToken(userModel);    
                return Ok(new {message="logged in successfully.",userModel,status=StatusCodes.Status200OK,token=token});
            }
            else
            {
                return Json(new {message="you are not registered user.",status =StatusCodes.Status401Unauthorized,email_id=userModel.user_email,password=userModel.user_password});
            }
        }

    }
}