namespace invenory.Models.product
{
    public class Product
    {
        public int product_id { get; set; }
        public string product_name { get; set; } = "";
        public string product_description { get; set; } = "";
        public decimal product_price { get; set; }

        public decimal product_available_qty { get; set; }
        public decimal product_total_qty { get; set; }

        public decimal product_mrp { get; set; }
        public decimal product_discount { get; set; }
        public bool is_available { get; set; }
        public bool is_pieces { get; set; }
        public string[] product_images { get; set; }

        public int user_id { get; set; }
        public int subCategory_id { get; set; }

    }
}
