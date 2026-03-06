using BusinessAccountantService.Models;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BusinessAccountantService.Managers
{
    internal class PdfExportManager
    {
        public void ExportEntryAct(Client client, RepairRecord repair)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Priemka_{client.Name}_{DateTime.Now:ddMMyy}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);

                        page.Header().Row(row => {
                            row.RelativeItem().Column(col => {
                                col.Item().Text("АКТ ПРИЕМКИ ВЕЛОСИПЕДА").FontSize(20).SemiBold().FontColor(Colors.Blue.B);
                                col.Item().Text($"Номер заказа: #00{repair.Id}").FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text(repair.Status.ToUpper()).FontSize(24).Bold().FontColor(Colors.Green.B);
                                c.Item().Text("ОПЛАЧЕНО").FontSize(10).AlignCenter();
                            });
                            row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                        });

                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            col.Item().PaddingBottom(5).Text("ДАННЫЕ КЛИЕНТА").Bold();
                            col.Item().Text($"ФИО: {client.Name}");
                            col.Item().Text($"Телефон: {client.Phone}");

                            col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Gray.R);

                            col.Item().PaddingBottom(5).Text("ОБЪЕКТ ПРИЕМКИ").Bold();
                            col.Item().Text($"Велосипед: {repair.BikeInfo}");

                            col.Item().PaddingTop(15).Text("ОПИСАНИЕ НЕИСПРАВНОСТИ:").Bold();
                            col.Item().Border(0.5f).Padding(10).Background(Colors.Gray.R)
                                .Text(repair.ProblemDescription).Italic();

                            col.Item().PaddingTop(40).Row(row => {
                                row.RelativeItem().Text("Принял: __________");
                                row.RelativeItem().AlignRight().Text("Сдал: __________");
                            });
                        });

                        page.Footer().AlignCenter().Text(x => {
                            x.Span("Стр. ");
                            x.CurrentPageNumber();
                        });
                    });
                })
                .GeneratePdf(saveFileDialog.FileName);

                Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
            }
        }
    }
}
