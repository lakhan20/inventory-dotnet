using invenory.Models;
using invenory.Models.product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace invenory.Controllers
{
    public class CategoryController : Controller
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        JwtAuthorization jwt;
        public CategoryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            jwt = new JwtAuthorization(configuration, _httpContextAccessor);
            connectionString = configuration["ConnectionStrings:PostgresDb"] ?? "";

        }
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/addCategory")]
        [Authorize]
        public ActionResult addCategory([FromBody] CategoryModel categoryModel)
        {
            int cnt = 0;
            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT public.fn_insert_category(@category_name)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@category_name", categoryModel.category_name);



                            cnt = (int)command.ExecuteScalar();
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (cnt > 0)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return Unauthorized();
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("/api/getCategoryById/{category_id}")]
        [Authorize]
        public ActionResult getCategoryById(int category_id)
        {

            int cnt = 0;
            CategoryModel categoryModel=new CategoryModel();
            var authorize = jwt.authorizeUser();

            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT public.fn_get_category_by_id(@category_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {

                            command.Parameters.AddWithValue("@category_id", category_id);
                            using (var readerCategory = command.ExecuteReader())
                            {
                                if (readerCategory.Read())
                                {
                                    //var id = readerCategory.GetName;
                                    //categoryModel.category_id = readerCategory.GetInt32(0);
                                    //categoryModel.category_name = readerCategory.GetString(1);
                                }
                                else
                                {
                                    return NotFound(new { message = "No Record Found...!!", status = 404 });
                                }
                            }

                        }
                    }
                    return Ok(new {category=categoryModel});
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            } else
            {
                return Unauthorized();
            }




        }
        }
    }


