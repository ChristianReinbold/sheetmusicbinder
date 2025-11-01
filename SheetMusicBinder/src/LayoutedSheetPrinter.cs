using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;

namespace de.creinbold.SheetMusicBinder
{
    class LayoutedSheetPrinter
    {
        private static readonly float ANGLE_90 = (float)(2 * Math.PI / 4);

        public string Source { get; private set; }
        public int NumberOfPages { get; private set; }

        public bool InsertEmptyPages { get; set; }


        public LayoutedSheetPrinter(string source)
        {
            // Check if the input path is a valid pdf.
            using (PdfDocument srcDoc = new PdfDocument(new PdfReader(source)))
            {
                NumberOfPages = srcDoc.GetNumberOfPages();
            }
            Source = source;
        }

        private void _PrintLayouted(IList<int> pages, Layout layout, string layoutedPath)
        {
            using (PdfDocument srcDoc = new PdfDocument(new PdfReader(Source)))
            using (PdfDocument resultDoc = new PdfDocument(new PdfWriter(layoutedPath)))
            {
                var pageSize = new PageSize(srcDoc.GetPage(1).GetPageSize());
                resultDoc.SetDefaultPageSize(pageSize);

                foreach (var paper in layout.GetPapers())
                {
                    foreach (var page in paper)
                    {
                        if (page.Index == AbsStrip.EMPTY_PAGE && !InsertEmptyPages) continue;
                        if (page.Index == AbsStrip.EMPTY_PAGE) resultDoc.AddNewPage();
                        else srcDoc.CopyPagesTo(pages[page.Index - 1], pages[page.Index - 1], resultDoc);
                    }
                }
            }
        }

        private void _PrintMarked(Layout layout, string layoutedPath, string markedPath)
        {
            int pdfPageIndex = 0;
            using (var pdfDoc = new PdfDocument(new PdfReader(layoutedPath), new PdfWriter(markedPath)))
            {
                foreach (var paper in layout.GetPapers())
                {
                    foreach (var page in paper)
                    {
                        if (page.Index == AbsStrip.EMPTY_PAGE && !InsertEmptyPages) continue;
                        var pdfCanvas = new PdfCanvas(pdfDoc.GetPage(++pdfPageIndex));
                        pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(0.3f));
                        var pageSize = pdfDoc.GetPage(pdfPageIndex).GetPageSize();

                        if (page.LeftMarkIndex != PageWithMarkings.NO_MARK)
                        {
                            var canvas = new Canvas(pdfCanvas, pageSize);
                            canvas.ShowTextAligned(page.LeftMarkIndex.ToString(),
                                                   pageSize.GetX(),
                                                   pageSize.GetY() + pageSize.GetHeight() / 2,
                                                   iText.Layout.Properties.TextAlignment.CENTER,
                                                   iText.Layout.Properties.VerticalAlignment.TOP,
                                                   ANGLE_90);
                            canvas.Close();
                        }
                        if (page.RightMarkIndex != PageWithMarkings.NO_MARK)
                        {
                            var canvas = new Canvas(pdfCanvas, pageSize);
                            canvas.ShowTextAligned(page.RightMarkIndex.ToString(),
                                                   pageSize.GetX() + pageSize.GetWidth(),
                                                   pageSize.GetY() + pageSize.GetHeight() / 2,
                                                   iText.Layout.Properties.TextAlignment.CENTER,
                                                   iText.Layout.Properties.VerticalAlignment.BOTTOM,
                                                   ANGLE_90);
                            canvas.Close();
                        }

                        pdfCanvas.SetExtGState(new PdfExtGState().SetFillOpacity(1.0f));

                        if (page.FlipCue)
                        {
                            var canvas = new Canvas(pdfCanvas, pageSize);
                            var text = new Paragraph("Blättern");
                            text.SetBold();
                            text.SetFontSize(20);
                            canvas.ShowTextAligned(text,
                                                   pageSize.GetX() + pageSize.GetWidth(),
                                                   pageSize.GetY(),
                                                   iText.Layout.Properties.TextAlignment.RIGHT,
                                                   iText.Layout.Properties.VerticalAlignment.BOTTOM);
                            canvas.Close();
                        }
                        if (page.FlipInPage)
                        {
                            var canvas = new Canvas(pdfCanvas, pageSize);
                            var text = new Paragraph("In Seite blättern!");
                            text.SetBold();
                            text.SetFontSize(20);
                            text.SetFontColor(ColorConstants.RED);
                            canvas.ShowTextAligned(text,
                                                   pageSize.GetX() + pageSize.GetWidth() / 2,
                                                   pageSize.GetY() + pageSize.GetHeight(),
                                                   iText.Layout.Properties.TextAlignment.CENTER,
                                                   iText.Layout.Properties.VerticalAlignment.TOP);
                            canvas.Close();
                        }
                    }
                }
            }
        }

        public string Print(IList<int> pages, Layout layout)
        {
            var dotIdx = Source.LastIndexOf('.');
            if (dotIdx == -1) dotIdx = Source.Length;
            var path = Source.Substring(0, dotIdx);
            var layoutedPath = path + ".layouted.pdf";
            var markedPath = path + ".layouted_and_marked.pdf";
            _PrintLayouted(pages, layout, layoutedPath);
            _PrintMarked(layout, layoutedPath, markedPath);
            return markedPath;
        }
    }
}
