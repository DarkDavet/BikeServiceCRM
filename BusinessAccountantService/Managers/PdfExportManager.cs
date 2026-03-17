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
        string masterName = "Колос Глеб Юрьевич"; 
        string masterPhone = "+375 (29) 277-72-16"; 
        string shopAddress = "г. Минск, ул. Бурдейного, 22";

        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BikeLab_logo.png");
        string qrPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BikeLab_insta.png");


        public void ExportEntryAct(Client client, RepairRecord repair)
        {
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
                            col.Item().Text("BikeLab | Ремонт и обслуживание велосипедов").FontSize(8).Italic().FontColor("#7F8C8D");
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

                    page.Footer().PaddingTop(10).Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor("#DCDDE1");

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            // Левая часть: QR-код и призыв к действию
                            if (File.Exists(qrPath))
                            {
                                row.ConstantItem(100).Image(qrPath);
                                row.RelativeItem().PaddingLeft(10).Column(c =>
                                {
                                    c.Item().Text("СЛЕДИТЕ ЗА НАМИ В INSTAGRAM").FontSize(12).Bold().FontColor("#E1306C");
                                    c.Item().Text("Ремонт и обслуживание велосипедов | Веломастер").FontSize(10).FontColor("#7F8C8D");
                                });
                            }
                            else
                            {
                                row.RelativeItem().Text("BikeLab — качественный сервис для вашего велосипеда").FontSize(10).FontColor("#BDC3C7");
                            }

                            // Правая часть: Благодарность
                            row.RelativeItem().AlignRight().PaddingTop(10).Text(t =>
                            {
                                t.Span("Благодарим за доверие!").FontSize(10).Italic();
                            });
                        });
                    });
                });
            }).GeneratePdf("EntryAct.pdf");

            Process.Start(new ProcessStartInfo("EntryAct.pdf") { UseShellExecute = true });
        }



        public void ExportFinalAct(Client client, RepairRecord repair, List<RepairItem> items)
        {
            var mainColor = "#2C3E50";
            var accentColor = "#27AE60"; // Зеленый для финального акта

            // Фильтруем только услуги (где ProductId == null)
            var operations = items.Where(x => x.ProductId == null).ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(mainColor).FontFamily("Arial"));

                    // --- ХЕДЕР (Копия стиля ExportEntryAct) ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (File.Exists(logoPath))
                                col.Item().Height(60).Image(logoPath);
                            else
                                col.Item().Text("BIKE LAB").FontSize(24).ExtraBold().FontColor(accentColor);

                            col.Item().PaddingTop(2).Text(shopAddress).FontSize(9).SemiBold();
                            col.Item().Text("BikeLab | Ремонт и обслуживание велосипедов").FontSize(8).Italic().FontColor("#7F8C8D");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("АКТ ВЫПОЛНЕННЫХ РАБОТ").FontSize(18).ExtraBold().FontColor(mainColor);
                            col.Item().Text($"ЗАКАЗ №{repair.Id:D4}").FontSize(14).Bold().FontColor(accentColor);
                            col.Item().Text($"Дата выдачи: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                        });
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // --- БЛОК 1: КЛИЕНТ И ВЕЛОСИПЕД (Стиль с рамками) ---
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(0.5f).BorderColor("#DCDDE1").Padding(10).Column(c => {
                                c.Item().Text("ЗАКАЗЧИК").FontSize(8).Bold().FontColor("#7F8C8D");
                                c.Item().PaddingTop(2).Text(client.Name).FontSize(11).Bold();
                                c.Item().Text(client.Phone).FontSize(10);
                            });

                            row.ConstantItem(10);

                            row.RelativeItem().Border(0.5f).BorderColor("#DCDDE1").Padding(10).Column(c => {
                                c.Item().Text("ОБЪЕКТ ОБСЛУЖИВАНИЯ").FontSize(8).Bold().FontColor("#7F8C8D");
                                c.Item().PaddingTop(2).Text(repair.BikeInfo).FontSize(11).Bold();
                                c.Item().Text("Ремонт завершен").FontSize(9).Italic();
                            });
                        });

                        // --- БЛОК 2: ТАБЛИЦА ОПЕРАЦИЙ ---
                        col.Item().PaddingTop(25).Text("ПЕРЕЧЕНЬ ВЫПОЛНЕННЫХ РАБОТ:").FontSize(11).Bold();

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).PaddingVertical(5).Text("№").Bold();
                                header.Cell().BorderBottom(1).PaddingVertical(5).Text("Наименование услуги").Bold();
                            });

                            int index = 1;
                            foreach (var op in operations)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor("#EEEEEE").PaddingVertical(8).Text((index++).ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor("#EEEEEE").PaddingVertical(8).Text(op.Name);
                            }
                        });

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

                        // --- БЛОК 3: ИТОГО (Акцентная плашка) ---
                        col.Item().PaddingTop(20).AlignRight().Row(r => {
                            r.RelativeItem();
                            r.ConstantItem(250).Background("#F1F2F6").Padding(10).Row(inner => {
                                inner.RelativeItem().AlignLeft().Text("ИТОГО К ОПЛАТЕ:").FontSize(11).Bold();
                                inner.RelativeItem().AlignRight().Text($"{repair.TotalCost:N2} BYN").FontSize(13).Bold().FontColor(accentColor);
                            });
                        });

                        // --- БЛОК 5: ПОДПИСИ (Копия стиля ExportEntryAct) ---
                        col.Item().PaddingTop(50).Row(row => {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("Выдал (Мастер):").FontSize(9);
                                c.Item().PaddingTop(15).Text("____________________").FontSize(10);
                                c.Item().Text("(подпись)").FontSize(7).FontColor("#7F8C8D");
                            });

                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text("Получил (Заказчик):").FontSize(9);
                                c.Item().PaddingTop(15).Text("____________________").FontSize(10);
                                c.Item().Text("(подпись)").FontSize(7).FontColor("#7F8C8D");
                            });
                        });
                    });

                    // Футер
                    page.Footer().PaddingTop(10).Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor("#DCDDE1");

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            // Левая часть: QR-код и призыв к действию
                            if (File.Exists(qrPath))
                            {
                                row.ConstantItem(100).Image(qrPath);
                                row.RelativeItem().PaddingLeft(10).Column(c =>
                                {
                                    c.Item().Text("СЛЕДИТЕ ЗА НАМИ В INSTAGRAM").FontSize(12).Bold().FontColor("#E1306C");
                                    c.Item().Text("Ремонт и обслуживание велосипедов | Веломастер").FontSize(10).FontColor("#7F8C8D");
                                });
                            }
                            else
                            {
                                row.RelativeItem().Text("BikeLab — качественный сервис для вашего велосипеда").FontSize(10).FontColor("#BDC3C7");
                            }

                            // Правая часть: Благодарность
                            row.RelativeItem().AlignRight().PaddingTop(10).Text(t =>
                            {
                                t.Span("Благодарим за доверие!").FontSize(10).Italic();
                            });
                        });
                    });
                });
            }).GeneratePdf("FinalAct.pdf");

            Process.Start(new ProcessStartInfo("FinalAct.pdf") { UseShellExecute = true });
        }





    }
}
