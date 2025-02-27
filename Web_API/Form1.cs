using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Web_API.Helpers;
using Web_API.Models;
using Web_API.Services;

namespace Web_API.Forms
{
    public partial class MainForm : Form
    {
        private readonly MakeupApiService _apiService = new MakeupApiService();
        private readonly ImageHelper _imageHelper;

        private List<Product> _products = new List<Product>();
        private List<string> _brands = new List<string>();
        private List<string> _productTypes = new List<string>();
        private Dictionary<string, List<string>> _categoriesByType = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> _tagsByType = new Dictionary<string, List<string>>();

        public MainForm()
        {
            InitializeComponent();
            _imageHelper = new ImageHelper(_apiService);
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
            imageColumn.Width = 60;
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
            categoryColumn.DataPropertyName = "category";
            dgvProducts.Columns.Add(categoryColumn);

            DataGridViewTextBoxColumn tagsColumn = new DataGridViewTextBoxColumn();
            tagsColumn.HeaderText = "Tags";
            tagsColumn.DataPropertyName = "TagsDisplay";
            dgvProducts.Columns.Add(tagsColumn);

            cmbType.SelectedIndexChanged += CmbType_SelectedIndexChanged;
            dgvProducts.Scroll += DgvProducts_Scroll;

            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            try
            {
                string statusMessage = "Carregando dados...";
                lblStatus.Text = statusMessage;

                _products = await _apiService.GetAllProductsAsync();

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

                    ProcessProductCategories();

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

        private void ProcessProductCategories()
        {
            foreach (var product in _products)
            {
                if (!string.IsNullOrEmpty(product.product_type))
                {
                    if (!_categoriesByType.ContainsKey(product.product_type))
                    {
                        _categoriesByType[product.product_type] = new List<string>();
                    }

                    if (!string.IsNullOrEmpty(product.category) && !_categoriesByType[product.product_type].Contains(product.category))
                    {
                        _categoriesByType[product.product_type].Add(product.category);
                    }
                }
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
                    var categorias = _categoriesByType[selectedType].ToArray();
                    MessageBox.Show($"Categorias encontradas: {string.Join(", ", categorias)}");
                    cmbCategory.Items.AddRange(_categoriesByType[selectedType].ToArray());
                }
                else
                {
                    MessageBox.Show("Nenhuma categoria encontrada para esse tipo.");
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

                string selectedBrand = cmbBrand.SelectedIndex > 0 ? cmbBrand.SelectedItem.ToString() : null;
                string selectedType = cmbType.SelectedIndex > 0 ? cmbType.SelectedItem.ToString() : null;

                List<Product> filteredProducts;
                if (!string.IsNullOrEmpty(selectedBrand) || !string.IsNullOrEmpty(selectedType))
                {
                    filteredProducts = await _apiService.GetFilteredProductsAsync(selectedBrand, selectedType);
                }
                else
                {
                    filteredProducts = new List<Product>(_products);
                }

                // Filtrar por categoria se selecionada
                bool hasCategory = cmbCategory.SelectedIndex > 0;
                if (hasCategory)
                {
                    string selectedCategory = cmbCategory.SelectedItem.ToString();
                    filteredProducts = filteredProducts.Where(p => p.category == selectedCategory).ToList();
                }

                // Filtrar por tag se selecionada
                bool hasTag = cmbTag.SelectedIndex > 0;
                if (hasTag)
                {
                    string selectedTag = cmbTag.SelectedItem.ToString();
                    filteredProducts = filteredProducts.Where(p =>
                        p.tag_list != null && p.tag_list.Contains(selectedTag)).ToList();
                }

                // Atualizar o DataGridView com os produtos filtrados
                var productsBindingList = new BindingList<Product>(filteredProducts);
                var dataSource = new BindingSource(productsBindingList, null);
                dgvProducts.DataSource = dataSource;

                // Atualizar as imagens visíveis
                await LoadImagesForVisibleRows();

                // Exibir o número de produtos encontrados
                lblStatus.Text = $"Produtos encontrados: {filteredProducts.Count}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erro ao filtrar produtos.";
                MessageBox.Show($"Erro ao filtrar produtos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadImagesForVisibleRows()
        {
            await _imageHelper.LoadImagesForVisibleRows(dgvProducts, GetImageUrlForRow);
        }

        private string GetImageUrlForRow(int rowIndex)
        {
            if (dgvProducts.DataSource is BindingSource bs)
            {
                if (rowIndex < bs.Count)
                {
                    Product product = (Product)bs[rowIndex];
                    string imageUrl = product?.api_featured_image;
                    if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("//"))
                    {
                        // Converte para URL absoluta
                        imageUrl = "https:" + imageUrl;
                    }
                    return imageUrl;
                }
            }
            return null;
        }



        private async void DgvProducts_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                await LoadImagesForVisibleRows();
            }
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            dgvProducts.DataSource = null;
            cmbBrand.SelectedIndex = 0;
            cmbType.SelectedIndex = 0;
            cmbCategory.SelectedIndex = 0;
            cmbTag.SelectedIndex = 0;
        }
    }
}