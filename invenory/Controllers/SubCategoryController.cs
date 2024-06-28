using invenory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace invenory.Controllers
{
    public class SubCategoryController : Controller
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        JwtAuthorization jwt;
        public SubCategoryController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            jwt = new JwtAuthorization(configuration, _httpContextAccessor);
            connectionString = configuration["ConnectionStrings:PostgresDb"] ?? "";
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("api/addsubcategory")]
        public ActionResult addSubCategory([FromBody] SubCategoryModel subCategoryModel)
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
                        string sql = "SELECT public.fn_insert_subcategory(@subCategory_name, @category_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {

                            command.Parameters.AddWithValue("subCategory_name", subCategoryModel.subCategory_name);
                            command.Parameters.AddWithValue("category_id", subCategoryModel.category_id);

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
                    return Ok(new { message = "sub-category inserted successfully.", StatusCodes.Status200OK });
                }
                else
                {
                    return BadRequest(new { message = "Unable to insert sub-category.", status = StatusCodes.Status400BadRequest });


                }

            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again.", status = StatusCodes.Status401Unauthorized });

            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/api/getsubcategorybyid/{subCategory_id}")]
        public ActionResult getSubCategoryById(int subCategory_id)
        {

            SubCategoryModel subCategoryModel = new SubCategoryModel();

            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from public.fn_get_subcategory_by_id(@subCategory_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@subCategory_id", subCategory_id);
                            using (var subcategoryReader = command.ExecuteReader())
                            {
                                if (subcategoryReader.Read())
                                {
                                    subCategoryModel.subCategory_id = subcategoryReader.GetInt32(0);
                                    subCategoryModel.subCategory_name = subcategoryReader.GetString(1);
                                    subCategoryModel.category_id = subcategoryReader.GetInt32(2);
                                    subCategoryModel.category_name = subcategoryReader.GetString(3);
                                }
                                else
                                {
                                    return BadRequest(new { message = "sub-category does not exists.", status = StatusCodes.Status400BadRequest });

                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });
                }
                if (subCategoryModel.subCategory_name != null && subCategoryModel.category_name != null)
                {
                    return Ok(new { message = "sub-category reterived successfully.", status = StatusCodes.Status200OK, subCategoryModel });
                }
                else
                {
                    return BadRequest(new { message = "sub-category does not exists.", status = StatusCodes.Status400BadRequest });
                }


            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again.", status = StatusCodes.Status401Unauthorized });

            }
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("/api/getallsubcategories")]
        public ActionResult getAllSubCategories()
        {

            List<SubCategoryModel> subcategories = new List<SubCategoryModel>();

            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from public.fn_get_all_subcategory()";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            using (var readersubCategories = command.ExecuteReader())
                            {
                                while (readersubCategories.Read())
                                {
                                    SubCategoryModel subCategoryModel = new SubCategoryModel();
                                    subCategoryModel.subCategory_id = readersubCategories.GetInt32(0);
                                    subCategoryModel.subCategory_name = readersubCategories.GetString(1);
                                    subCategoryModel.category_id = readersubCategories.GetInt32(2);
                                    subCategoryModel.category_name = readersubCategories.GetString(3);
                                    subcategories.Add(subCategoryModel);
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });

                }
                if (subcategories.Count > 0)
                {
                    return Ok(new { message = "all sub-categories found.", subcategories, status = StatusCodes.Status200OK });

                }
                else
                {
                    return BadRequest(new { message = "unable to fetch sub-categories.", status = StatusCodes.Status400BadRequest });

                }

            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again.", status = StatusCodes.Status401Unauthorized });

            }



        }


        [AllowAnonymous]
        [HttpPut]
        [Route("api/updatesubcategory/{subcategory_id}")]
        [Authorize]
        public ActionResult updateSubCategory(int subcategory_id, [FromBody] SubCategoryModel subCategoryModel)
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
                        string sql = "SELECT public.fn_edit_subcategory(@subCategory_id,@subCategory_name, @category_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@subCategory_id", subcategory_id);
                            command.Parameters.AddWithValue("@subCategory_name", subCategoryModel.subCategory_name);
                            command.Parameters.AddWithValue("@category_id", subCategoryModel.category_id);
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
                    return Ok(new { message = "sub-categories updated successfully.", StatusCodes.Status200OK });
                }
                else
                {
                    return BadRequest(new { message = "Unable to update sub-category.", status = StatusCodes.Status400BadRequest });


                }

            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again.", status = StatusCodes.Status401Unauthorized });

            }
        }


    }


}



