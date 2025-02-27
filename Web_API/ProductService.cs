using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Web_API.Models;

namespace Web_API
{
    public class ProductService
    {
        private List<Product> _products = new List<Product>();
        private Dictionary<string, List<string>> _categoriesByType = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _tagsByType = new Dictionary<string, List<string>>();

        public void ConfigureDataGridView(DataGridView dgv)
        {
            dgv.Columns.Clear();
            dgv.AutoGenerateColumns = false;

            dgv.Columns.Add(new DataGridViewImageColumn
            {
                HeaderText = "Imagem",
                Name = "imgProduct",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 80
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Marca", DataPropertyName = "brand" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", DataPropertyName = "product_type" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoria", DataPropertyName = "product_category" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tags", DataPropertyName = "TagsDisplay" });
        }

        public void SetProducts(List<Product> products)
        {
            _products = products;
            _categoriesByType.Clear();
            _tagsByType.Clear();

            foreach (var product in _products)
            {
                if (!string.IsNullOrEmpty(product.product_type))
                {
                    if (!_categoriesByType.ContainsKey(product.product_type))
                    {
                        _categoriesByType[product.product_type] = new List<string>();
                    }
                    if (!string.IsNullOrEmpty(product.product_category) && !_categoriesByType[product.product_type].Contains(product.product_category))
                    {
                        _categoriesByType[product.product_type].Add(product.product_category);
                    }

                    if (!_tagsByType.ContainsKey(product.product_type))
                    {
                        _tagsByType[product.product_type] = new List<string>();
                    }
                    if (product.tag_list != null)
                    {
                        foreach (var tag in product.tag_list)
                        {
                            if (!_tagsByType[product.product_type].Contains(tag))
                            {
                                _tagsByType[product.product_type].Add(tag);
                            }
                        }
                    }
                }
            }
        }

        public void FillComboBox(ComboBox comboBox, List<string> items)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("Todas as opções");
            if (items != null && items.Count > 0)
            {
                comboBox.Items.AddRange(items.ToArray());
            }
            comboBox.SelectedIndex = 0;
        }

        public List<string> GetBrands() => _products.Select(p => p.brand).Distinct().OrderBy(b => b).ToList();
        public List<string> GetProductTypes() => _products.Select(p => p.product_type).Distinct().OrderBy(t => t).ToList();
        public int GetProductCount() => _products.Count;
        public List<string> GetCategoriesByType(string type) => _categoriesByType.ContainsKey(type) ? _categoriesByType[type] : new List<string>();
        public List<string> GetTagsByType(string type) => _tagsByType.ContainsKey(type) ? _tagsByType[type] : new List<string>();
    }
}
