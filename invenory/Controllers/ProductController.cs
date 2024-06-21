using invenory.Models.product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

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

        //[Authorize]
        [AllowAnonymous]
        [HttpPost]
        [Route("/api/addProduct")]
        [Authorize]
        public ActionResult addProduct([FromBody] Product product)
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
                            command.Parameters.AddWithValue("@product_images", product.product_images);
                            command.Parameters.AddWithValue("@user_id", authorize.user_id);
                            command.Parameters.AddWithValue("@subCategory_id",product.subCategory_id);


                            //   command.ExecuteNonQuery();
                            // cnt = command.ExecuteNonQuery();

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
    }
}
