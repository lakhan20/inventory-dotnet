using invenory.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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


        //add category controller
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



                            cnt = Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });
                }
                if (cnt > 0)
                {
                    return Ok(new { message = "category added successfully.", status = StatusCodes.Status200OK });
                }
                else
                {
                    return BadRequest(new { message = "something went wrong while adding category.", status = StatusCodes.Status400BadRequest });

                }
            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });

            }
        }


        //get category by id 
        [AllowAnonymous]
        [HttpGet]
        [Route("/api/getCategoryById/{category_id}")]
        [Authorize]
        public ActionResult getCategoryById(int category_id)
        {

           
            CategoryModel categoryModel = new CategoryModel();
            var authorize = jwt.authorizeUser();

            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from public.fn_get_category_by_id(@category_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {

                            command.Parameters.AddWithValue("@category_id", category_id);
                            using (var readerCategory = command.ExecuteReader())
                            {
                                if (readerCategory.Read())
                                {
                                    //var id = readerCategory.GetInt64(0);
                                    categoryModel.category_id = readerCategory.GetInt32(0);
                                    categoryModel.category_name = readerCategory.GetString(1);
                                }
                                else
                                {
                                    return BadRequest(new { message = "category does not exists.", status = StatusCodes.Status400BadRequest });

                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                if (categoryModel.category_name != null)
                {
                    return Ok(new { message = "category reterived successfully", category = categoryModel, status = StatusCodes.Status200OK });
                }
                else
                {
                    return BadRequest(new { message = "category does not exists.", status = StatusCodes.Status400BadRequest });
                }
            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });
            }

        }

        //get all category

        [AllowAnonymous]
        [HttpGet]
        [Route("/api/getallcategories")]
        [Authorize]
        public ActionResult getAllCategories()
        {
            List<CategoryModel> categories = new List<CategoryModel>();
            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from  public.fn_get_all_categories()";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            using (var readerCategory = command.ExecuteReader())
                            {
                                while (readerCategory.Read())
                                {
                                    var categoryModel = new CategoryModel();
                                    categoryModel.category_id = readerCategory.GetInt32(0);
                                    categoryModel.category_name = readerCategory.GetString(1);
                                    categories.Add(categoryModel);

                                }
                            }
                        }
                    }


                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });

                }
                if (categories.Count > 0)
                {
                    return Ok(new { message = "all categories reterived.", StatusCodes.Status200OK, categories });
                }
                else
                {
                    return BadRequest(new { message = "category does not exists.", status = StatusCodes.Status400BadRequest });

                }
            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again.", status = StatusCodes.Status401Unauthorized });
            }
        }


        //update category
        [AllowAnonymous]
        [HttpPut]
        [Route("/api/updatecategory/{category_id}")]
        [Authorize]
        public ActionResult updateCategory(int category_id, [FromBody] CategoryModel categoryModel)
        {
            var cnt = 0;
            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT public.fn_edit_category(@category_id, @category_name)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("category_id", category_id);
                            command.Parameters.AddWithValue("category_name", categoryModel.category_name);
                            cnt = Convert.ToInt32(command.ExecuteScalar());

                        }
                    }


                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });

                }

                if (cnt != 0)
                {
                    return Ok(new { message = "categories updated successfully.", StatusCodes.Status200OK });
                }
                else
                {
                    return BadRequest(new { message = "Unable to update category.", status = StatusCodes.Status400BadRequest });
                }

            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });
            }

        }

    }









}


