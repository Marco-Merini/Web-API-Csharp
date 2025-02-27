# Desafio API:

- Crie um projeto WindowsForms;
- Que irá consumir esta [API](https://makeup-api.herokuapp.com/);
- Terá que ter uma busca de maquiagem por marca, tipo, categoria e TAG (categoria e TAG estão atreladas com o tipo);
- Deverá listar as informações em um DataGrid inclusive a imagem;

# Makeup API - Buscador de Produtos de Maquiagem

Este é um aplicativo Windows Forms desenvolvido em C# que consome a API pública de maquiagem ([Makeup API](https://makeup-api.herokuapp.com/)) para buscar, visualizar e filtrar produtos de maquiagem de diferentes marcas e categorias.

## 📋 Funcionalidades

- **Busca de produtos** por marca, tipo, categoria e tags
- **Visualização dos produtos** em formato de grade com imagens
- **Filtragem dinâmica** de categorias e tags baseadas no tipo de produto selecionado
- **Carregamento assíncrono** de imagens para melhor performance
- **Interface responsiva** com controles intuitivos

## 🚀 Tecnologias Utilizadas

- **C# .NET Framework 4.8**
- **Windows Forms** para a interface gráfica
- **Newtonsoft.Json** para deserialização dos dados da API
- **HttpClient** para requisições HTTP
- **Async/Await** para operações assíncronas

## 🏗️ Arquitetura do Projeto

O projeto segue uma arquitetura simples dividida em:

### 📁 Modelos

- `Product.cs` - Classe que representa um produto de maquiagem

### 📁 Serviços

- `MakeupApiService.cs` - Serviço para comunicação com a API
- `ProductService.cs` - Serviço para gerenciamento de produtos
- `ImageHelper.cs` - Classe auxiliar para carregamento de imagens

### 📁 Interface

- `MainForm.cs` - Formulário principal da aplicação
- `Form1.Designer.cs` - Código gerado automaticamente para o design do formulário

## 📝 Como Funciona

1. Ao iniciar, o aplicativo carrega todos os produtos disponíveis na Makeup API
2. Os produtos são processados para extrair marcas, tipos, categorias e tags únicas
3. As listas de opções são preenchidas nos ComboBoxes correspondentes
4. Quando o usuário seleciona um tipo de produto, as categorias e tags são atualizadas dinamicamente
5. Ao clicar em "Buscar", a aplicação filtra os produtos conforme os critérios selecionados
6. Os resultados são exibidos em um DataGridView com imagens carregadas assincronamente
7. O botão "Limpar" restaura os filtros e limpa a grade de resultados

## 🔍 Destaques do Código

### Carregamento Assíncrono de Dados

```csharp
private async void LoadInitialDataAsync()
{
    try
    {
        string statusMessage = "Carregando dados...";
        lblStatus.Text = statusMessage;

        _products = await _apiService.GetAllProductsAsync();
        
        // Processamento dos dados...
        
        lblStatus.Text = $"Produtos encontrados: {_products.Count}";
    }
    catch (Exception ex)
    {
        lblStatus.Text = "Erro ao carregar dados.";
        MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", 
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### Filtros Dinâmicos

```csharp
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
```

### Carregamento Otimizado de Imagens

```csharp
public async Task LoadImagesForVisibleRows(DataGridView dgv, Func<int, string> getImageUrlForRow)
{
    foreach (DataGridViewRow row in dgv.Rows)
    {
        if (row.Visible && row.Index < dgv.RowCount)
        {
            string imageUrl = getImageUrlForRow(row.Index);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await LoadImageForDataGridViewCell(dgv, row.Index, 0, imageUrl);
            }
        }
    }
}
```

## 📥 Como Usar

1. Clone este repositório
2. Abra a solução no Visual Studio
3. Restaure os pacotes NuGet necessários (Newtonsoft.Json)
4. Compile e execute o projeto

---

Desenvolvido por [Marco Leone Merini](https://github.com/Marco-Merini) - 2025
