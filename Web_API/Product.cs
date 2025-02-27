using System.Collections.Generic;

namespace Web_API.Models
{
    public class Product
    {
        public string brand { get; set; }
        public string product_type { get; set; }
        public string product_category { get; set; }
        public List<string> tag_list { get; set; }
        public string image_link { get; set; }
        public string TagsDisplay => tag_list != null ? string.Join(", ", tag_list) : string.Empty;
    }
}