using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Web_API.Services;

namespace Web_API.Helpers
{
    public class ImageHelper
    {
        private readonly MakeupApiService _apiService;

        public ImageHelper(MakeupApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadImageForDataGridViewCell(DataGridView dgv, int rowIndex, int columnIndex, string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                byte[] imageBytes = await _apiService.GetImageBytesAsync(imageUrl);

                if (imageBytes != null && dgv.Rows.Count > rowIndex)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image img = Image.FromStream(ms);
                        dgv.Rows[rowIndex].Cells[columnIndex].Value = img;
                    }
                }
            }
            catch
            {
                // Silently fail on image loading errors
            }
        }

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
    }
}