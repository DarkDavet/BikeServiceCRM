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
                                col.Item().Text("ВЕЛО-МАСТЕРСКАЯ \"ДВА КОЛЕСА\"").FontSize(22).ExtraBold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                                col.Item().Text("Ремонт любой сложности • Запчасти • Тюнинг").FontSize(9).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                col.Item().PaddingTop(5).Text(x => {
                                    x.Span("Тел: ").Bold();
                                    x.Span("+7 (999) 000-00-00");
                                });
                            });

                            // ПРАВАЯ СТОРОНА: Номер и статус
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text("АКТ ПРИЁМКИ").FontSize(14).SemiBold();
                                c.Item().Text($"Заказ №: #00{repair.Id}").FontSize(12).Bold();
                                c.Item().PaddingTop(5).Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            // Данные клиента
                            col.Item().PaddingBottom(5).Text("ДАННЫЕ КЛИЕНТА").Bold().FontSize(12);
                            col.Item().Text($"ФИО: {client.Name}");
                            col.Item().Text($"Телефон: {client.Phone}");

                            col.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                            // Велосипед
                            col.Item().PaddingBottom(5).Text("ОБЪЕКТ ПРИЕМКИ").Bold().FontSize(12);
                            col.Item().Text($"Модель: {repair.BikeInfo}");

                            // Проблема
                            col.Item().PaddingTop(10).Text("ОПИСАНИЕ НЕИСПРАВНОСТИ:").Bold();
                            col.Item().Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(10).Text(repair.ProblemDescription).Italic();

                            // ПРЕДВАРИТЕЛЬНЫЙ СПИСОК РАБОТ (Новое!)
                            if (!string.IsNullOrWhiteSpace(repair.WorksPerformed))
                            {
                                col.Item().PaddingTop(15).Text("ПРЕДВАРИТЕЛЬНЫЙ ПЛАН РАБОТ:").Bold();
                                col.Item().Text(repair.WorksPerformed);
                            }

                            // ФИНАНСЫ (Новое!)
                            col.Item().PaddingTop(15).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Ориентировочная стоимость: {repair.TotalCost} руб.").FontSize(14).SemiBold();
                                c.Item().Text("Цена может измениться после дефектовки").FontSize(9).Italic();
                            });

                            // Подписи
                            col.Item().PaddingTop(40).Row(row => {
                                row.RelativeItem().Column(c => {
                                    c.Item().Text("Принял (Мастер):");
                                    c.Item().PaddingTop(10).Text("____________________");
                                });
                                row.RelativeItem().AlignRight().Column(c => {
                                    c.Item().Text("Сдал (Клиент):");
                                    c.Item().PaddingTop(10).Text("____________________");
                                });
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
        public void ExportFinalAct(Client client, RepairRecord repair)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Check_{client.Name}_{DateTime.Now:ddMMyy}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(50);
                        page.Header().Row(row => {
                            row.RelativeItem().Column(col => {
                                col.Item().Text("ВЕЛО-МАСТЕРСКАЯ \"ДВА КОЛЕСА\"").FontSize(22).ExtraBold().FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                                col.Item().Text("Ремонт любой сложности • Запчасти • Тюнинг").FontSize(9).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                col.Item().PaddingTop(5).Text(x => {
                                    x.Span("Тел: ").Bold();
                                    x.Span("+7 (999) 000-00-00"); // Укажите ваш реальный номер
                                });
                            });

                            // ПРАВАЯ СТОРОНА: Номер и статус
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text("АКТ ВЫПОЛНЕННЫХ РАБОТ").FontSize(14).SemiBold();
                                c.Item().Text($"Заказ №: #00{repair.Id}").FontSize(12).Bold();
                                c.Item().PaddingTop(5).Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(10);
                            });
                        });

                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            // Блок клиента
                            col.Item().PaddingBottom(5).Text("ЗАКАЗЧИК").Bold().FontSize(12);
                            col.Item().Text(client.Name);
                            col.Item().Text($"Тел: {client.Phone}");

                            col.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                            // Объект
                            col.Item().Text(x => {
                                x.Span("ОБЪЕКТ: ").Bold();
                                x.Span(repair.BikeInfo);
                            });

                            // СПИСОК РАБОТ И ЗАПЧАСТЕЙ
                            col.Item().PaddingTop(20).PaddingBottom(5).Text("ДЕТАЛИЗАЦИЯ РАБОТ И МАТЕРИАЛОВ:").Bold();
                            col.Item().Border(0.5f).Padding(10).Text(repair.WorksPerformed).FontSize(11);

                            // ИТОГО
                            col.Item().PaddingTop(20).AlignRight().Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text($"ИТОГО К ОПЛАТЕ: {repair.TotalCost} руб.").FontSize(16).Bold();
                                c.Item().Text("НДС не облагается").FontSize(8).Italic();
                            });

                            // Гарантия и подписи
                            col.Item().PaddingTop(30).Text("Гарантия на выполненные работы и установленные запчасти: 14 дней со дня выдачи.").FontSize(10).Italic();

                            col.Item().PaddingTop(40).Row(row => {
                                row.RelativeItem().Text("Выдал (Мастер): ________________");
                                row.RelativeItem().AlignRight().Text("Получил (Клиент): ________________");
                            });
                        });

                        page.Footer().AlignCenter().Text(x => {
                            x.Span("Благодарим за обращение в наш сервис!");
                        });
                    });
                })
                .GeneratePdf(saveFileDialog.FileName);

                Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
            }
        }

    }
}
