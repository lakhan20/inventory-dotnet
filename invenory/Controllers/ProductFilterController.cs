using invenory.Models.product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace invenory.Controllers
{
    public class ProductFilterController : Controller
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        JwtAuthorization jwt;
        public ProductFilterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            jwt = new JwtAuthorization(configuration, _httpContextAccessor);
            connectionString = configuration["ConnectionStrings:PostgresDb"] ?? "";

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("/api/filterByCategory/{category_id}")]
        [Authorize]
        public ActionResult filterByCategory(int category_id)
        {
            var authorize = jwt.authorizeUser();
            List<Product> productList = new List<Product>();
            if (authorize != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "SELECT * from public.fn_get_available_product_by_category_id(@category_id)";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@category_id", category_id);
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
                                    productList.Add(product);
                                }

                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message, status = StatusCodes.Status500InternalServerError });
                }
                if (productList.Count > 0)
                {
                    return Ok(new { message = "products reterived successfully.", status = StatusCodes.Status200OK, productList }) ;


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
    }
}
