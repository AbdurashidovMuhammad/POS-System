using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WPF.Models;
using WPF.ViewModels;

namespace WPF.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
        Loaded += SalesView_Loaded;
        Unloaded += SalesView_Unloaded;
    }

    private void SalesView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
        {
            vm.SearchDialogClosed += OnSearchDialogClosed;
            vm.ReceiptDialogClosed += OnReceiptDialogClosed;
            vm.PrintReceiptRequested += OnPrintReceiptRequested;
            vm.PropertyChanged += Vm_PropertyChanged;
        }
        BarcodeTextBox.Focus();
    }

    private void SalesView_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
        {
            vm.SearchDialogClosed -= OnSearchDialogClosed;
            vm.ReceiptDialogClosed -= OnReceiptDialogClosed;
            vm.PrintReceiptRequested -= OnPrintReceiptRequested;
            vm.PropertyChanged -= Vm_PropertyChanged;
        }
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SalesViewModel.IsSearchDialogOpen) && DataContext is SalesViewModel vm && vm.IsSearchDialogOpen)
        {
            Dispatcher.BeginInvoke(new System.Action(() => SearchDialogTextBox.Focus()),
                System.Windows.Threading.DispatcherPriority.Render);
        }

        if (e.PropertyName == nameof(SalesViewModel.SelectedSearchProduct) && DataContext is SalesViewModel vm2 && vm2.SelectedSearchProduct is not null)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (vm2.IsManualGramm)
                {
                    ManualKgTextBox.Focus();
                    ManualKgTextBox.SelectAll();
                }
                else
                {
                    ManualQuantityTextBox.Focus();
                    ManualQuantityTextBox.SelectAll();
                }
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }

    private void OnSearchDialogClosed()
    {
        Dispatcher.BeginInvoke(new System.Action(() => BarcodeTextBox.Focus()),
            System.Windows.Threading.DispatcherPriority.Render);
    }

    private void SearchOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
            vm.CloseSearchDialogCommand.Execute(null);
    }

    private void SearchDialog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void ReceiptOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is SalesViewModel vm)
            vm.CloseReceiptDialogCommand.Execute(null);
    }

    private void ReceiptDialog_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void OnReceiptDialogClosed()
    {
        Dispatcher.BeginInvoke(new System.Action(() => BarcodeTextBox.Focus()),
            System.Windows.Threading.DispatcherPriority.Render);
    }

    private void OnPrintReceiptRequested(SaleDto sale)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        var document = BuildReceiptDocument(sale);
        var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
        printDialog.PrintDocument(paginator, $"Chek #{sale.Id}");
    }

    private FlowDocument BuildReceiptDocument(SaleDto sale)
    {
        // 80mm termal printer: 302pt ≈ 80mm (WPF 96dpi)
        // Courier New 10pt ≈ 6pt/belgi → 278pt usable / 6 ≈ 46 belgi
        const double pageW = 302.0;
        const int    cols  = 38;

        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Courier New"),
            FontSize    = 10,
            PageWidth   = pageW,
            PagePadding = new Thickness(12, 24, 12, 24),
            ColumnWidth = double.PositiveInfinity
        };

        doc.Blocks.Add(MakeSep('=', cols));
        doc.Blocks.Add(MakePara($"SOTUV CHEKI #{sale.Id}", fontSize: 13, weight: FontWeights.Bold, align: TextAlignment.Center));
        doc.Blocks.Add(MakeSep('=', cols));

        doc.Blocks.Add(MakePara($"Sana   : {sale.SaleDate:dd.MM.yyyy HH:mm}", weight: FontWeights.Bold));
        doc.Blocks.Add(MakePara($"Kassir : {sale.UserFullName}", weight: FontWeights.Bold));
        doc.Blocks.Add(MakePara($"To'lov : {sale.PaymentTypeName}", weight: FontWeights.Bold));
        doc.Blocks.Add(MakeSep('-', cols));

        int idx = 1;
        foreach (var item in sale.Items)
        {
            string unit = ((WPF.Enums.UnitType)item.UnitType) switch
            {
                WPF.Enums.UnitType.Gramm => "g",
                WPF.Enums.UnitType.Litr  => "l",
                WPF.Enums.UnitType.Metr  => "m",
                _                         => "don"
            };

            doc.Blocks.Add(MakePara($"{idx++}. {item.ProductName}", weight: FontWeights.Bold, margin: new Thickness(0, 8, 0, 2)));
            doc.Blocks.Add(MakePara($"   {item.Quantity:0.##} {unit} x {item.UnitPrice:N0} = {item.Subtotal:N0}", weight: FontWeights.Bold, margin: new Thickness(0, 0, 0, 8)));
        }

        doc.Blocks.Add(MakeSep('-', cols));

        // PadLeft bilan o'ng hizalash — TextAlignment.Right ishlatilmaydi,
        // chunki u A4 da sahifaning o'ng chetiga ketadi.
        string totalStr = $"JAMI: {sale.TotalAmount:N0} so'm";
        doc.Blocks.Add(MakePara(totalStr.PadLeft(cols), fontSize: 12, weight: FontWeights.Bold));

        doc.Blocks.Add(MakeSep('=', cols));
        doc.Blocks.Add(MakePara("Rahmat! Yana tashrif buyuring.", weight: FontWeights.Bold, align: TextAlignment.Center, margin: new Thickness(0, 6, 0, 0)));

        return doc;
    }

    private static Paragraph MakePara(
        string text,
        int fontSize = 10,
        FontWeight? weight = null,
        TextAlignment align = TextAlignment.Left,
        Brush? fg = null,
        Thickness? margin = null)
    {
        var p = new Paragraph(new Run(text))
        {
            FontSize      = fontSize,
            FontWeight    = weight ?? FontWeights.Normal,
            TextAlignment = align,
            Margin        = margin ?? new Thickness(0, 2, 0, 2)
        };
        if (fg != null) p.Foreground = fg;
        return p;
    }

    private static Paragraph MakeSep(char ch, int count)
        => new(new Run(new string(ch, count))) { Margin = new Thickness(0, 4, 0, 4) };

    private void GrammTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void GrammTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }

    private void GrammTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out var value))
            {
                value = Math.Clamp(value, 0, 999);
                textBox.Text = value.ToString("000");
            }
            else
            {
                textBox.Text = "000";
            }
        }
    }
}
