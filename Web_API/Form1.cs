using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using System.ComponentModel;

namespace Web_API
{
    public partial class Form1 : Form
    {
        private const string BaseUrl = "https://makeup-api.herokuapp.com/api/v1/products";
        private readonly HttpClient _httpClient = new HttpClient();
        private List<Product> _products = new List<Product>();
        private List<string> _brands = new List<string>();
        private List<string> _productTypes = new List<string>();
        private Dictionary<string, List<string>> _categoriesByType = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _tagsByType = new Dictionary<string, List<string>>();

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {

            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.HeaderText = "Imagem";
            imageColumn.Name = "imgProduct";
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            imageColumn.Width = 80;
            dgvProducts.Columns.Add(imageColumn);

            DataGridViewTextBoxColumn brandColumn = new DataGridViewTextBoxColumn();
            brandColumn.HeaderText = "Marca";
            brandColumn.DataPropertyName = "brand";
            dgvProducts.Columns.Add(brandColumn);

            DataGridViewTextBoxColumn typeColumn = new DataGridViewTextBoxColumn();
            typeColumn.HeaderText = "Tipo";
            typeColumn.DataPropertyName = "product_type";
            dgvProducts.Columns.Add(typeColumn);

            DataGridViewTextBoxColumn categoryColumn = new DataGridViewTextBoxColumn();
            categoryColumn.HeaderText = "Categoria";
            categoryColumn.DataPropertyName = "product_category";
            dgvProducts.Columns.Add(categoryColumn);

            DataGridViewTextBoxColumn tagsColumn = new DataGridViewTextBoxColumn();
            tagsColumn.HeaderText = "Tags";
            tagsColumn.DataPropertyName = "TagsDisplay";
            dgvProducts.Columns.Add(tagsColumn);

            cmbType.SelectedIndexChanged += CmbType_SelectedIndexChanged;

            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            try
            {
                string statusMessage = "Carregando dados...";
                lblStatus.Text = statusMessage;

                string jsonResponse = await _httpClient.GetStringAsync(BaseUrl + ".json");
                _products = JsonConvert.DeserializeObject<List<Product>>(jsonResponse);

                if (_products != null)
                {
                    _categoriesByType.Clear();
                    _tagsByType.Clear();
                    
                    _brands = _products.Where(p => !string.IsNullOrEmpty(p.brand))
                                        .Select(p => p.brand)
                                        .Distinct()
                                        .OrderBy(b => b)
                                        .ToList();

                    _productTypes = _products.Where(p => !string.IsNullOrEmpty(p.product_type))
                                             .Select(p => p.product_type)
                                             .Distinct()
                                             .OrderBy(t => t)
                                             .ToList();
                    
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

                    UpdateComboBox(cmbBrand, _brands);
                    UpdateComboBox(cmbType, _productTypes);

                    lblStatus.Text = $"Produtos encontrados: {_products.Count}";
                    await FilterProductsAsync();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro ao carregar dados.";
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateComboBox(ComboBox comboBox, List<string> items)
        {
            if (comboBox.InvokeRequired)
            {
                comboBox.Invoke(new Action(() =>
                {
                    comboBox.Items.Clear();
                    comboBox.Items.Add("Todas as opções");
                    comboBox.Items.AddRange(items.ToArray());
                    comboBox.SelectedIndex = 0;
                }));
            }
            else
            {
                comboBox.Items.Clear();
                comboBox.Items.Add("Todas as opções");
                comboBox.Items.AddRange(items.ToArray());
                comboBox.SelectedIndex = 0;
            }
        }

        private void CmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbCategory.Items.Clear();
            cmbTag.Items.Clear();

            cmbCategory.Items.Add("Todas as categorias");
            cmbTag.Items.Add("Todas as tags");

            if (cmbType.SelectedIndex > 0)
            {
                string selectedType = cmbType.SelectedItem.ToString();

                if (_categoriesByType.ContainsKey(selectedType))
                {
                    cmbCategory.Items.AddRange(_categoriesByType[selectedType].ToArray());
                }

                if (_tagsByType.ContainsKey(selectedType))
                {
                    cmbTag.Items.AddRange(_tagsByType[selectedType].ToArray());
                }
            }

            cmbCategory.SelectedIndex = 0;
            cmbTag.SelectedIndex = 0;
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await FilterProductsAsync();
        }

        private async Task FilterProductsAsync()
        {
            try
            {
                string statusMessage = "Buscando produtos...";
                lblStatus.Text = statusMessage;

                StringBuilder queryParams = new StringBuilder();

                if (cmbBrand.SelectedIndex > 0)
                {
                    string brand = cmbBrand.SelectedItem.ToString();
                    queryParams.Append($"?brand={Uri.EscapeDataString(brand)}");
                }

                if (cmbType.SelectedIndex > 0)
                {
                    string productType = cmbType.SelectedItem.ToString();
                    if (queryParams.Length == 0)
                        queryParams.Append($"?product_type={Uri.EscapeDataString(productType)}");
                    else
                        queryParams.Append($"&product_type={Uri.EscapeDataString(productType)}");
                }

                bool hasCategory = cmbCategory.SelectedIndex > 0;
                bool hasTag = cmbTag.SelectedIndex > 0;
                string selectedCategory = hasCategory ? cmbCategory.SelectedItem.ToString() : null;
                string selectedTag = hasTag ? cmbTag.SelectedItem.ToString() : null;

                List<Product> filteredProducts;
                if (queryParams.Length > 0)
                {
                    string url = BaseUrl + ".json" + queryParams.ToString();
                    string jsonResponse = await _httpClient.GetStringAsync(url);
                    filteredProducts = JsonConvert.DeserializeObject<List<Product>>(jsonResponse);
                }
                else
                {
                    filteredProducts = new List<Product>(_products);
                }

                if (hasCategory)
                {
                    filteredProducts = filteredProducts.Where(p => p.product_category == selectedCategory).ToList();
                }

                if (hasTag)
                {
                    filteredProducts = filteredProducts.Where(p =>
                        p.tag_list != null && p.tag_list.Contains(selectedTag)).ToList();
                }

                // Atualizar o DataGridView com os produtos filtrados
                var productsBindingList = new BindingList<Product>(filteredProducts);
                var dataSource = new BindingSource(productsBindingList, null);
                dgvProducts.DataSource = dataSource;

                // Atualizar as imagens visíveis
                LoadImagesForVisibleRows();

                // Exibir o número de produtos encontrados
                lblStatus.Text = $"Produtos encontrados: {filteredProducts.Count}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro ao filtrar produtos.";
                MessageBox.Show($"Erro ao filtrar produtos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadImagesForVisibleRows()
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                if (row.Visible && row.Index < dgvProducts.RowCount)
                {
                    Product product = dgvProducts.DataSource is BindingSource bs
                        ? (Product)bs[row.Index]
                        : null;

                    if (product != null && !string.IsNullOrEmpty(product.image_link))
                    {
                        LoadImageForCell(row.Index, 0, product.image_link);
                    }
                }
            }
        }

        private async void LoadImageForCell(int rowIndex, int columnIndex, string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                using (HttpResponseMessage response = await _httpClient.GetAsync(imageUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (Stream imageStream = await response.Content.ReadAsStreamAsync())
                        {
                            if (dgvProducts.Rows.Count > rowIndex)
                            {

                                Image img = Image.FromStream(imageStream);


                                dgvProducts.Rows[rowIndex].Cells[columnIndex].Value = img;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void dgvProducts_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                LoadImagesForVisibleRows();
            }
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            dgvProducts.Rows.Clear();
            cmbBrand.SelectedIndex = -1;
            cmbType.SelectedIndex = -1;
            cmbCategory.SelectedIndex = -1;
            cmbTag.SelectedIndex = -1;
        }
    }

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