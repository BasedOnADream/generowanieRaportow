using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace generowanieRaportow;

public partial class MainWindow : Window
{
    static string dataFolder = @""; // Add path for txt files here
    static string pdfFolder = @""; // Add path for pdf files here

    public MainWindow()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        InitializeComponent();
        displayReports();
        date.Text = DateOnly.FromDateTime(DateTime.Today).ToString();
    }

    private void converter(object sender, RoutedEventArgs e) 
    {
        var document = QuestPDF.Fluent.Document.Create(container => 
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.Header().Element(header);
                page.Content().PaddingVertical(40).Element(table);

            });
        });

        void header(IContainer container) 
        {
            container.Row(row => 
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item()
                        .Text(title.Text)
                        .FontSize(32).SemiBold();

                    column.Item().Text(text =>
                    {
                        text.Span($"Data stworzenia: {date.Text}").SemiBold();
                    });
                });
            });
        }

        void table(IContainer container) 
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn();
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Element");
                    header.Cell().Element(CellStyle).Text("Szczegóły");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                    }
                });

                int lp = 1;
                for(int i = 2; i < (report.Children.Count); i += 2)
                {
                    table.Cell().Element(CellStyle).Text(lp.ToString());
                    table.Cell().Element(CellStyle).Text((report.Children[i] as TextBlock).Text.Substring(0, (report.Children[i] as TextBlock).Text.Length - 1));
                    table.Cell().Element(CellStyle).Text((report.Children[i + 1] as TextBlock).Text);
                    lp++;

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        }
        document.GeneratePdf($@"{pdfFolder}//{title.Text}.pdf");
        document.GeneratePdfAndShow();
    }

    private void adjustReport(object sender, SizeChangedEventArgs e){report.Height = Window.GetWindow(this).Height - 58;}

    private void addDetail(object sender, RoutedEventArgs e)
    {
        report.Children.Add(new TextBlock { FontSize = 18, Text = element.Text+":"});
        report.Children.Add(new TextBlock { FontSize = 16, Text = details.Text, Margin = new Thickness(0, 0, 0, 5) });
        details.Text = ""; element.Text = "";
    }

    private void selectReport(object sender, RoutedEventArgs e) 
    {
        report.Children.RemoveRange(2, report.Children.Count - 2);
        string filePath = System.IO.Path.Combine(dataFolder, $"{(sender as Button).Content}.txt");
        string[] content = File.ReadAllText(filePath).Split('\n');
        title.Text = content[0].Substring(0, content[0].Length-1);
        date.Text = content[1];
        for (int i = 3; i < content.Length-1; i += 3)
        {
            report.Children.Add(new TextBlock { FontSize = 18, Text = content[i].Substring(0, content[i].Length - 1) });
            report.Children.Add(new TextBlock { FontSize = 16, Text = content[i+1], Margin = new Thickness(0, 0, 0, 5) });
        }
    }

    private void deleteReport(object sender, RoutedEventArgs e)
    {
        string filePath = System.IO.Path.Combine(dataFolder, $"{title.Text}.txt");
        if (File.Exists(filePath)) 
        {
            File.Delete(filePath);
            displayReports();
        }
        title.Text = "";
        date.Text = DateOnly.FromDateTime(DateTime.Today).ToString();
        report.Children.RemoveRange(2, report.Children.Count - 2);
    }

    void displayReports() 
    {
        archive.Children.RemoveRange(1, archive.Children.Count - 1);
        foreach (string i in Directory.GetFiles(dataFolder)) 
        {
            Button btn = new();
            string path = System.IO.Path.GetFileName(i);
            btn.Content = path.Substring(0, path.Length - 4);
            btn.Click += selectReport;
            archive.Children.Add(btn);
        }
    }

    private void addReport(object sender, RoutedEventArgs e) 
    {
        try 
        {
            string filePath = System.IO.Path.Combine(dataFolder, $"{title.Text}.txt");
            List<string> content = new List<string>{title.Text,date.Text+"\n"};
            for (int i = 2; i < (report.Children.Count); i += 2)
            {
                content.Add((report.Children[i] as TextBlock).Text);
                content.Add((report.Children[i + 1] as TextBlock).Text + "\n");
            }
            File.WriteAllText(filePath, string.Join(Environment.NewLine, content));
            displayReports();
            MessageBox.Show("Raport został zapisany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch(Exception err) {MessageBox.Show(err.ToString(), "Błąd przy zapisywaniu pliku",MessageBoxButton.OK, MessageBoxImage.Error);}
    }
}