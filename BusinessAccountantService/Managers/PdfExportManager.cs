using BusinessAccountantService.Models;
using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BusinessAccountantService.Managers
{
    public class PdfExportManager
    {
        public void ExportEntryAct(Client client, RepairRecord repair)
        {
            // --- ДАННЫЕ МАСТЕРСКОЙ И МАСТЕРА ---
            string masterName = "Колос Глеб Юрьевич"; // Впишите ФИО мастера
            string masterPhone = "+375 (29) 277-72-16"; // Впишите телефон мастера
            string shopAddress = "г. Минск, ул. Бурдейного, 22";

            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BikeLab_logo.png");
            var mainColor = "#2C3E50";
            var accentColor = "#E67E22";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(mainColor).FontFamily("Arial"));

                    // --- ХЕДЕР ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            // ЛОГОТИП
                            if (File.Exists(logoPath))
                                col.Item().Height(60).Image(logoPath);
                            else
                                col.Item().Text("BIKE LAB").FontSize(24).ExtraBold().FontColor(accentColor);

                            // АДРЕС ПОД ЛОГОТИПОМ
                            col.Item().PaddingTop(2).Text(shopAddress).FontSize(9).SemiBold();
                            col.Item().Text("Профессиональный велосервис").FontSize(8).Italic().FontColor("#7F8C8D");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("АКТ ПРИЁМКИ ТС").FontSize(18).ExtraBold().FontColor(mainColor);
                            col.Item().Text($"ЗАКАЗ №{repair.Id:D4}").FontSize(14).Bold().FontColor(accentColor);
                            col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                        });
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // --- БЛОК 1: КЛИЕНТ И ВЕЛОСИПЕД ---
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(0.5f).BorderColor("#DCDDE1").Padding(10).Column(c => {
                                c.Item().Text("ВЛАДЕЛЕЦ").FontSize(8).Bold().FontColor("#7F8C8D");
                                c.Item().PaddingTop(2).Text(client.Name).FontSize(11).Bold();
                                c.Item().Text(client.Phone).FontSize(10);
                            });

                            row.ConstantItem(10);

                            row.RelativeItem().Border(0.5f).BorderColor("#DCDDE1").Padding(10).Column(c => {
                                c.Item().Text("ОБЪЕКТ ПРИЕМКИ").FontSize(8).Bold().FontColor("#7F8C8D");
                                c.Item().PaddingTop(2).Text(repair.BikeInfo).FontSize(11).Bold();;
                            });
                        });

                        // --- БЛОК 2: ОПИСАНИЕ (БЕЗ ТАБЛИЦЫ) ---
                        col.Item().PaddingTop(25).Text("ОПИСАНИЕ НЕИСПРАВНОСТЕЙ:").FontSize(11).Bold();

                        col.Item().PaddingTop(8).Background("#F9F9F9").Padding(15).Column(c => {
                            c.Item().Text(repair.ProblemDescription).LineHeight(1.5f);

                            // Пустые линии для ручных заметок, если описание короткое
                            c.Item().PaddingTop(10).Text("____________________________________________________________________________________");
                            c.Item().PaddingTop(10).Text("____________________________________________________________________________________");
                        });

                        // --- БЛОК 4: ДАННЫЕ МАСТЕРА ---
                        col.Item().PaddingTop(20).BorderTop(0.5f).BorderColor("#DCDDE1").PaddingTop(10).Row(r => {
                            r.RelativeItem().Text(t => {
                                t.Span("Ваш мастер: ").Bold();
                                t.Span(masterName);
                            });
                            r.RelativeItem().AlignRight().Text(t => {
                                t.Span("Связь с мастером: ").Bold();
                                t.Span(masterPhone);
                            });
                        });

                        col.Item().PaddingTop(60).Row(row => {
                            // Мастер
                            row.RelativeItem().Column(c => {
                                c.Item().Text("Принял (Мастер):").FontSize(9);
                                c.Item().PaddingTop(15).Text("____________________").FontSize(10);
                                c.Item().Text("(подпись)").FontSize(7).FontColor("#7F8C8D");
                            });

                            // Клиент
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text("Сдал (Клиент):").FontSize(9);
                                c.Item().PaddingTop(15).Text("____________________").FontSize(10);
                                c.Item().Text("(подпись)").FontSize(7).FontColor("#7F8C8D");
                            });
                        });
                    });

                    // Футер без номера страницы
                    page.Footer().PaddingTop(30).AlignCenter().Text("BIKE LAB — Качественный сервис для вашего велосипеда").FontSize(8).FontColor("#BDC3C7");
                });
            }).GeneratePdf("EntryAct.pdf");

            Process.Start(new ProcessStartInfo("EntryAct.pdf") { UseShellExecute = true });
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
