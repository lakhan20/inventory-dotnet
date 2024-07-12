﻿namespace invenory.Models.product
{
    public class GetProductByIdModel
    {
        public string product_name { get; set; } = "";
        public string product_description { get; set; } = "";
        public double product_price { get; set; }

        public double product_available_qty { get; set; }
        public double product_total_qty { get; set; }

        public double product_mrp { get; set; }
        public double product_discount { get; set; }
        public bool is_available { get; set; }
        public bool is_pieces { get; set; }
        public string[] product_images { get; set; }

        public string subCategory_name { get; set; }
        public string category_name { get; set; }
        //public DateTime created_at { get; set; }
        //public DateTime updated_at { get; set; }
        public string created_at_str { get; set; }
        public string updated_at_str { get; set; }
    }
}