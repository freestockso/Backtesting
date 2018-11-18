using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace UUBacktesting
{
    public class TelerikGridViewHelper
    {
        IEnumerable<string> exportFormats;
        public IEnumerable<string> ExportFormats
        {
            get
            {
                if (exportFormats == null)
                {
                    exportFormats = new string[] { "Excel", "ExcelML", "Word", "Csv" };
                }

                return exportFormats;
            }
        }
        string selectedExportFormat;
        public string SelectedExportFormat
        {
            get
            {
                return selectedExportFormat;
            }
            set
            {
                if (!object.Equals(selectedExportFormat, value))
                {
                    selectedExportFormat = value;
                }
            }
        }
        public void Export(object parameter)
        {
            var grid = parameter as RadGridView;
            if (grid != null)
            {
                grid.ElementExporting -= this.ElementExporting;
                grid.ElementExporting += this.ElementExporting;

                string extension = "";
                var format = ExportFormat.Html;

                switch (SelectedExportFormat)
                {
                    case "Excel":
                        extension = "xls";
                        format = ExportFormat.Html;
                        break;
                    case "ExcelML":
                        extension = "xml";
                        format = ExportFormat.ExcelML;
                        break;
                    case "Word":
                        extension = "doc";
                        format = ExportFormat.Html;
                        break;
                    case "Csv":
                        extension = "csv";
                        format = ExportFormat.Csv;
                        break;
                }

                var dialog = new SaveFileDialog();
                dialog.DefaultExt = extension;
                dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, SelectedExportFormat);
                dialog.FilterIndex = 1;

                if (dialog.ShowDialog() == true)
                {
                    using (var stream = dialog.OpenFile())
                    {
                        var exportOptions = new GridViewExportOptions();
                        exportOptions.Format = format;
                        exportOptions.ShowColumnFooters = true;
                        exportOptions.ShowColumnHeaders = true;
                        exportOptions.ShowGroupFooters = true;
                        exportOptions.Encoding = Encoding.Unicode;

                        grid.Export(stream, exportOptions);
                    }
                }
            }
        }

        void ElementExporting(object sender, GridViewElementExportingEventArgs e)
        {
            var htmlVisualExportParameters = e.VisualParameters as GridViewHtmlVisualExportParameters;
            if (htmlVisualExportParameters != null)
            {
                if (e.Element == ExportElement.HeaderRow || e.Element == ExportElement.FooterRow
                    || e.Element == ExportElement.GroupFooterRow)
                {
                    htmlVisualExportParameters.Background = HeaderBackground;
                    htmlVisualExportParameters.Foreground = HeaderForeground;
                    htmlVisualExportParameters.FontSize = 20;
                    htmlVisualExportParameters.FontWeight = FontWeights.Bold;
                }
                else if (e.Element == ExportElement.Row)
                {
                    htmlVisualExportParameters.Background = RowBackground;
                    htmlVisualExportParameters.Foreground = RowForeground;
                }
                else if (e.Element == ExportElement.Cell &&
                    e.Value != null && e.Value.Equals("Chocolade"))
                {
                    htmlVisualExportParameters.FontFamily = new FontFamily("Verdana");
                    htmlVisualExportParameters.Background = Colors.LightGray;
                    htmlVisualExportParameters.Foreground = Colors.Blue;
                }
                else if (e.Element == ExportElement.GroupHeaderRow)
                {
                    htmlVisualExportParameters.FontFamily = new FontFamily("Verdana");
                    htmlVisualExportParameters.Background = Colors.LightGray;
                    htmlVisualExportParameters.Height = 30;
                }
                else if (e.Element == ExportElement.GroupHeaderCell &&
                    e.Value != null && e.Value.Equals("Chocolade"))
                {
                    e.Value = "MyNewValue";
                }
            }
        }
        private Color headerBackground = Colors.LightGray;
        public Color HeaderBackground
        {
            get
            {
                return this.headerBackground;
            }
            set
            {
                if (this.headerBackground != value)
                {
                    this.headerBackground = value;
                }
            }
        }

        private Color rowBackground = Colors.White;
        public Color RowBackground
        {
            get
            {
                return this.rowBackground;
            }
            set
            {
                if (this.rowBackground != value)
                {
                    this.rowBackground = value;
                }
            }
        }

        Color headerForeground = Colors.Black;
        public Color HeaderForeground
        {
            get
            {
                return this.headerForeground;
            }
            set
            {
                if (this.headerForeground != value)
                {
                    this.headerForeground = value;
                }
            }
        }

        Color rowForeground = Colors.Black;
        public Color RowForeground
        {
            get
            {
                return this.rowForeground;
            }
            set
            {
                if (this.rowForeground != value)
                {
                    this.rowForeground = value;
                }
            }
        }
    }
}
