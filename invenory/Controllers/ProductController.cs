using invenory.Models;
using invenory.Models.product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Npgsql;
using System.IO;

namespace invenory.Controllers
{
    public class ProductController : Controller
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        JwtAuthorization jwt;
        public ProductController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            jwt = new JwtAuthorization(configuration, _httpContextAccessor);
            connectionString = configuration["ConnectionStrings:PostgresDb"] ?? "";

        }


        [AllowAnonymous]
        [HttpPost]
        [Route("/api/addproduct")]
        [Authorize]
        public ActionResult addProduct([FromForm] Product product)
        {
            int cnt = 0;
            List<string> images = new List<string>();
            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {

                try
                {
                    foreach (var item in product.product_image)
                    {

                        if (item.Length >= 51200)
                        {
                            return BadRequest(new { message = "Image size must be less then or equal to 50kb", status = StatusCodes.Status400BadRequest });
                        }
                        else if (item.ContentType != "image/png" || item.ContentType != "image/jpeg" || item.ContentType != "image/jpg")
                        {
                            return BadRequest(new { message = "Image file must contain jpeg or png extension", status = StatusCodes.Status400BadRequest });
                        }

                    }

                    foreach (var item in product.product_image)
                    {

                        string renamedfile = "_" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + item.FileName;
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", renamedfile);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            item.CopyTo(stream);
                            images.Add(renamedfile);
                        }

                    }

                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT public.fn_insert_product(@product_name, @product_description, @product_price, @product_available_qty, @product_total_qty, @product_mrp, @product_discount, @is_available, @is_pieces, @product_images, @user_id,@subCategory_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@product_name", product.product_name);
                            command.Parameters.AddWithValue("@product_description", product.product_description);
                            command.Parameters.AddWithValue("@product_price", product.product_price);
                            command.Parameters.AddWithValue("@product_available_qty", product.product_available_qty);
                            command.Parameters.AddWithValue("@product_total_qty", product.product_total_qty);
                            command.Parameters.AddWithValue("@product_mrp", product.product_mrp);
                            command.Parameters.AddWithValue("@product_discount", product.product_discount);
                            command.Parameters.AddWithValue("@is_available", product.is_available);
                            command.Parameters.AddWithValue("@is_pieces", product.is_pieces);
                            command.Parameters.AddWithValue("@product_images", images);
                            command.Parameters.AddWithValue("@user_id", authorize.user_id);
                            command.Parameters.AddWithValue("@subCategory_id", product.subCategory_id);




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
                    return Ok(new { message = "product added successfully.", status = StatusCodes.Status200OK });

                }
                else
                {
                    return BadRequest(new { message = "something went wrong while adding product...!!", status = StatusCodes.Status400BadRequest });

                }
            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });

            }
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("/api/getproductbyid/{product_id}")]
        [Authorize]

        public ActionResult getProductById(int product_id)
        {

            Product product = new Product();
            var authorize = jwt.authorizeUser();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from public.fn_get_available_product_by_id(@product_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@product_id", product_id);
                            using (var productReader = command.ExecuteReader())
                            {
                                if (productReader.Read())
                                {

                                    product.product_id = productReader.GetInt32(0);
                                    product.product_name = productReader.GetString(1);
                                    product.product_description = productReader.GetString(2);
                                    product.product_price = productReader.GetDouble(3);
                                    product.product_total_qty = productReader.GetDouble(4);
                                    product.product_available_qty = productReader.GetDouble(5);
                                    product.product_mrp = productReader.GetDouble(6);
                                    product.product_discount = productReader.GetDouble(7);
                                    product.is_available = productReader.GetBoolean(8);
                                    product.is_pieces = productReader.GetBoolean(9);

                                    product.product_images = productReader.GetFieldValue<string[]>(10);


                                    product.subCategory_id = productReader.GetInt32(11);
                                    product.subCategory_name = productReader.GetString(12);
                                    product.category_id = productReader.GetInt32(13);
                                    product.category_name = productReader.GetString(14);

                                    product.created_at = productReader.GetDateTime(15);
                                    var frmt = product.created_at.GetDateTimeFormats();
                                    product.created_at_str = frmt[11];
                                    try
                                    {
                                        product.updated_at = productReader.GetDateTime(16);
                                        var frmt2 = product.updated_at.GetDateTimeFormats();
                                        product.updated_at_str = frmt2[11];
                                    }
                                    catch
                                    {

                                    }
                                }
                                else
                                {
                                    return BadRequest(new { message = "This product does not exists.", status = StatusCodes.Status400BadRequest });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });

                }
                if (product.product_name != null && product.category_name != null)
                {
                    return Ok(new { message = "product reterived successfully.", status = StatusCodes.Status200OK, product });
                }
                else
                {
                    return BadRequest(new { message = "This product does not exists.", status = StatusCodes.Status400BadRequest });
                }
            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });

            }

        }



        [AllowAnonymous]
        [HttpGet]
        [Route("api/getallproducts")]
        [Authorize]

        public ActionResult getallproducts()
        {
            var authorize = jwt.authorizeUser();
            var products = new List<Product>();

            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from public.fn_get_all_available_products()";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            using (var productReader = command.ExecuteReader())
                            {
                                while (productReader.Read())
                                {
                                    Product product = new Product();
                                    product.product_id = productReader.GetInt32(0);
                                    product.product_name = productReader.GetString(1);
                                    product.product_description = productReader.GetString(2);
                                    product.product_price = productReader.GetDouble(3);
                                    product.product_total_qty = productReader.GetDouble(4);
                                    product.product_available_qty = productReader.GetDouble(5);
                                    product.product_mrp = productReader.GetDouble(6);
                                    product.product_discount = productReader.GetDouble(7);
                                    product.is_available = productReader.GetBoolean(8);
                                    product.is_pieces = productReader.GetBoolean(9);

                                    product.product_images = productReader.GetFieldValue<string[]>(10);


                                    product.subCategory_id = productReader.GetInt32(11);
                                    product.subCategory_name = productReader.GetString(12);
                                    product.category_id = productReader.GetInt32(13);
                                    product.category_name = productReader.GetString(14);

                                    product.created_at = productReader.GetDateTime(15);
                                    var frmt = product.created_at.GetDateTimeFormats();
                                    product.created_at_str = frmt[11];
                                    try
                                    {
                                        product.updated_at = productReader.GetDateTime(16);
                                        var frmt2 = product.updated_at.GetDateTimeFormats();
                                        product.updated_at_str = frmt2[11];
                                    }
                                    catch
                                    {

                                    }
                                    products.Add(product);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });

                }
                if (products.Count > 0)
                {
                    return Ok(new { message = "products reterived successfully.", status = StatusCodes.Status200OK, products });


                }
                else
                {
                    return BadRequest(new { message = "products don't exist.", status = StatusCodes.Status400BadRequest });

                }


            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });

            }
        }


        [AllowAnonymous]
        [HttpPut]
        [Route("api/updateproduct/{product_id}")]
        [Authorize]
        public ActionResult updateProduct(int product_id, [FromBody] Product product)
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
                        string sql = "SELECT public.fn_edit_product(@product_id,@product_name,@product_description,@product_price ,@product_available_qty,@product_total_qty,@product_mrp,@product_discount,@is_available,@is_pieces,@product_images,@user_id,@subcategory_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@product_id", product_id);
                            command.Parameters.AddWithValue("@product_name", product.product_name);
                            command.Parameters.AddWithValue("@product_description", product.product_description);
                            command.Parameters.AddWithValue("@product_price", product.product_price);
                            command.Parameters.AddWithValue("@product_available_qty", product.product_available_qty);
                            command.Parameters.AddWithValue("@product_total_qty", product.product_total_qty);
                            command.Parameters.AddWithValue("@product_mrp", product.product_mrp);
                            command.Parameters.AddWithValue("@product_discount", product.product_discount);
                            command.Parameters.AddWithValue("@is_available", product.is_available);
                            command.Parameters.AddWithValue("@is_pieces", product.is_pieces);
                            command.Parameters.AddWithValue("@product_images", product.product_images);
                            command.Parameters.AddWithValue("@user_id", authorize.user_id);
                            command.Parameters.AddWithValue("@subcategory_id", product.subCategory_id);

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
                    return Ok(new { message = "product updated successfully.", StatusCodes.Status200OK });
                }
                else
                {
                    return BadRequest(new { message = "something went wrong while updating status of product.", status = StatusCodes.Status400BadRequest });

                }


            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });

            }
        }


        [AllowAnonymous]
        [HttpPut]
        [Route("api/updateproductstatus/{product_id}")]
        [Authorize]

        public ActionResult updateProductStatus(int product_id)
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
                        string sql = "SELECT public.fn_edit_status_product(@product_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@product_id", product_id);
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
                    return Ok(new { message = "product status updated successfully.", status = StatusCodes.Status200OK });
                }
                else
                {
                    return NotFound(new { message = "something went wrong while updating status of product.", status = StatusCodes.Status404NotFound });

                }
            }
            else
            {
                return Unauthorized(new { message = "Unable to authorize you as a correct user please login again...", status = StatusCodes.Status401Unauthorized });

            }
        }


    }
}
