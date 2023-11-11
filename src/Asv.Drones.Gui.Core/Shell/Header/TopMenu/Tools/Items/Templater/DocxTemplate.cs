using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Word.DrawingShape;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using SkiaSharp;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

using Path = System.IO.Path;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using RunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using TableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Anchor = DocumentFormat.OpenXml.Drawing.Wordprocessing.Anchor;
using Picture = DocumentFormat.OpenXml.Wordprocessing.Picture;
using Shape = DocumentFormat.OpenXml.Vml.Shape;

namespace Asv.Drones.Gui.Core;

public class DocxTemplate : IDocxTemplate
{
    private WordprocessingDocument _document;
    private readonly string _tmpFilePath;
    
    /// <summary>
    /// Initializes a DocxTemplate object with a Stream
    /// </summary>
    /// <param name="stream"></param>
    public DocxTemplate(Stream stream)
    {
        _tmpFilePath = Path.GetTempFileName();
        try
        {
            if (stream.Length != 0)
                using (var fileStream = File.Create(_tmpFilePath, (int)stream.Length))
                {
                    //Allocates an array with a total size equal to the size of the stream
                    //May have difficulty allocating memory for large volumes
                    byte[] data = new byte[stream.Length];
                    _ = stream.Read(data, 0, data.Length);
                    fileStream.Write(data, 0, data.Length);
                }
            _document = WordprocessingDocument.Open(_tmpFilePath, true);
        }
        catch
        {
            _document = null;
        }
    }

    /// <summary>
    /// Initializes a DocxTemplate object with a file path
    /// </summary>
    /// <param name="filePath"></param>
    public DocxTemplate(string filePath)
    {
        try
        {
            _tmpFilePath = Path.GetTempFileName();
            File.Copy(filePath, _tmpFilePath, true);
            _document = WordprocessingDocument.Open(_tmpFilePath, true);
        }
        catch
        {
            _document = null;
        }
    }

    /// <summary>
    /// Insert an image referenced either by a file path or a memory stream
    /// at a location marked by a tag in the document.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="imgFileName">path to image</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Image(string tag, string imgFileName)
    {
        var mainPart = _document.MainDocumentPart;
        var imagePart = mainPart.AddImagePart(ImagePartType.Png);
        
        using (var stream = new FileStream(imgFileName, FileMode.Open))
        {
            imagePart.FeedData(stream);
        }
        using(var stream = new FileStream(imgFileName, FileMode.Open))
        using (SKManagedStream skStream = new SKManagedStream(stream))
        using (SKBitmap skBitmap = SKBitmap.Decode(skStream))
        {
            AddImageToBody(_document, mainPart.GetIdOfPart(imagePart), tag, ImagePartType.Png, skBitmap.Width, skBitmap.Height);
        }
       
    }

    /// <summary>
    /// Insert an image referenced either by a file path or a memory stream
    /// at a location marked by a tag in the document. 
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="imageStream">image stream</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Image(string tag, MemoryStream imageStream)
    {
        var mainPart = _document.MainDocumentPart;
        var imagePart = mainPart.AddImagePart(ImagePartType.Png);
        imagePart.FeedData(imageStream);
        using (SKManagedStream skStream = new SKManagedStream(imageStream))
        using (SKBitmap skBitmap = SKBitmap.Decode(skStream))
        {
            AddImageToBody(_document, mainPart.GetIdOfPart(imagePart), tag, ImagePartType.Png, skBitmap.Width, skBitmap.Height);
        }
    }
    
    /// <summary>
    /// This method is used to replace a specific tag
    /// with a provided text in headers, footers, and the document body.
    /// This could be useful for things like template placeholders.
    /// </summary>
    /// <param name="tagName">what to replace</param>
    /// <param name="toString">replace string</param>
    /// <returns>Returns replacement result.</returns>
    public bool Tag(string tagName, string toString)
    {
        if (_document == null) return false;

        //Finding (and replacing) the tag in the headers
        var headerParts = _document.MainDocumentPart.HeaderParts;

        foreach (var headerPart in headerParts)
        {
            foreach (var paragraph in headerPart.Header.ChildElements.OfType<Paragraph>())
                ReplaceTagInFooterParagraph(paragraph, tagName, toString);

            foreach (var table in headerPart.Header.ChildElements.OfType<Table>())
                foreach (var tableRow in table.Elements<TableRow>())
                    foreach (var tableCell in tableRow.Elements<TableCell>())
                    {
                        if (!CheckIsMarkedColumn(tableCell, tagName)) continue;
                        foreach (var element in tableCell.Elements<Paragraph>())
                            ReplaceTagInParagraph(element, tagName, toString);
                    }

            foreach (var sdtBlock in headerPart.Header.ChildElements.OfType<SdtBlock>())
                foreach (var paragraph in sdtBlock.SdtContentBlock.ChildElements.OfType<Paragraph>())
                    ReplaceTagInFooterParagraph(paragraph, tagName, toString);
        }

        //Finding (and replacing) the tag in the footers
        var footerParts = _document.MainDocumentPart.FooterParts;
        foreach (var footerPart in footerParts)
        {
            if (footerPart.Footer.Elements<SdtBlock>().Any())
                foreach (var sdtBlock in footerPart.Footer.Elements<SdtBlock>())
                    foreach (var paragraph in sdtBlock.SdtContentBlock.ChildElements.OfType<Paragraph>())
                        ReplaceTagInFooterParagraph(paragraph, tagName, toString);
            else
                foreach (var paragraph in footerPart.Footer.ChildElements.OfType<Paragraph>())
                    ReplaceTagInParagraphMod(paragraph, tagName, toString);
        }

        //Finding (and replacing) a tag in the document body
        var body = _document.MainDocumentPart.Document.Body;
        foreach (var paragraph in body.Elements<Paragraph>())
            ReplaceTagInParagraph(paragraph, tagName, toString);

        //Finding (and replacing) the tag in the document body tables
        InsertTagToTable(_document, tagName, toString);
        return true;
    }
    
    /// <summary>
    /// This method tries to insert the data into the existing table.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="data"></param>
    public void FixedTable(string column, IEnumerable<string> data)
    {
        InsertDataToTable(column, data, false);
    }

    /// <summary>
    /// This method can expand the table if the data size exceeds the table size.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="data"></param>
    public void DynamicTable(string column, IEnumerable<string> data)
    {
        InsertDataToTable(column, data, true);
    }
    
    /// <summary>
    /// This method saves any modifications to the document.
    /// </summary>
    /// <param name="filePath"></param>
    public void Save(string filePath)
    {
        if (_document == null) return;
        _document.MainDocumentPart.Document.Save();
        _document.Dispose();
        CreateDirectory(filePath);

        File.Copy(_tmpFilePath, filePath, true);
        _document = WordprocessingDocument.Open(_tmpFilePath, true);
    }

    /// <summary>
    /// This method disposes of the document
    /// and deletes any temporary files created during operations.
    /// </summary>
    public void Dispose()
    {
        if (_document == null) return;
        _document.Dispose();
        File.Delete(_tmpFilePath);
    }
    
    /// <summary>
    /// This method inserts an image into the body of the document
    /// at the location marked by a tag.
    /// </summary>
    /// <param name="wordDoc"></param>
    /// <param name="relationshipId"></param>
    /// <param name="tag"></param>
    /// <param name="imageType"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId, string tag, ImagePartType imageType, double width, double height)
    {
        var picName = $"{tag}.{(imageType == ImagePartType.Jpeg ? "jpg" : $"{imageType:G}".ToLower())}";
        //Define the reference of the image.
        var element =
                new Drawing(
                    new DW.Inline(
                        new DW.Extent { Cx = width.Inches(), Cy = height.Inches() }, //Width and Height of the image in inches. 1" = 1000000L
                        new DW.EffectExtent
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new DW.DocProperties
                        {
                            Id = 1U,
                            Name = tag //Make sure all of the images have a different name
                        },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new GraphicFrameLocks { NoChangeAspect = true }),
                        new Graphic(
                            new GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties
                                        {
                                            Id = (UInt32Value)0U,
                                            Name = picName //Make sure all of the images have a different name
                                        },
                                        new PIC.NonVisualPictureDrawingProperties()),
                                    new PIC.BlipFill(
                                        new Blip(
                                            new BlipExtensionList(
                                                new BlipExtension
                                                {
                                                    Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                })
                                        )
                                        {
                                            Embed = relationshipId,
                                            CompressionState = BlipCompressionValues.Print
                                        },
                                        new Stretch(
                                            new FillRectangle())),
                                    new PIC.ShapeProperties(
                                        new Transform2D(
                                            new Offset { X = 0L, Y = 0L },
                                            new Extents { Cx = width.Inches(), Cy = height.Inches() }), //Width and Height of the image in inches. 1" = 1000000L
                                        new PresetGeometry(
                                            new AdjustValueList()
                                        )
                                        { Preset = ShapeTypeValues.Rectangle }))
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    )
                    {
                        DistanceFromTop = 0U,
                        DistanceFromBottom = 0U,
                        DistanceFromLeft = 0U,
                        DistanceFromRight = 0U,
                        EditId = "50D07946"
                    });

        //Append the reference to the specific paragraph that contains the 'tag'.
        
        var paragraphs = wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>()
            .Where(f => f.InnerText.Contains('$' + tag + '$'));
        var tables = wordDoc.MainDocumentPart.Document.Body.Elements<Table>();
        paragraphs = tables.Aggregate(paragraphs, (current2, table) => table.Elements<TableRow>().Aggregate(current2, (current1, row) => row.Elements<TableCell>().Aggregate(current1, (current, cell) => current.Concat(cell.Elements<Paragraph>().Where(_ => _.InnerText.Contains('$' + tag + '$'))))));

        foreach (var paragraph in paragraphs)
            ReplaceImageTagInParagraph(paragraph, element);
    }

    /// <summary>
    /// This method used to replace a specified tag with text in different parts
    /// of the paragraph. It's essential for the Tag method which
    /// provides the functionality of replacing tags in the document.
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="tag"></param>
    /// <param name="tagValue"></param>
    private static void ReplaceTagInParagraphMod(Paragraph paragraph, string tag, string tagValue)
    {
        if (paragraph == null || string.IsNullOrEmpty(tag)) return;

        RemoveProofErrorFromParagraph(paragraph);

        var strBld = new StringBuilder("");

        var strPattern = $@"\${tag}\$";

        var startIndex = -1;
        var elements = paragraph.Elements().ToList();
        for (var i = 0; i < elements.Count; i++)
        {
            int count;
            if (elements[i] is Run)
            {
                if (elements[i].Elements().Any(_ => _ is Text) && !string.IsNullOrWhiteSpace(elements[i].InnerText))
                {
                    if (startIndex == -1) startIndex = i;
                    foreach (var element in elements[i].Elements())
                    {
                        strBld.Append(element.InnerText);
                    }
                }
                else
                {
                    count = startIndex != -1 ? i - startIndex : 0;

                    if (Regex.Match(strBld.ToString(), strPattern, RegexOptions.IgnoreCase) != Match.Empty)
                    {
                        var str = Regex.Replace(strBld.ToString(), strPattern, tagValue, RegexOptions.IgnoreCase);
                        ModifyParagraph(paragraph, str, startIndex, count);
                    }
                    strBld.Clear();
                    startIndex = -1;
                }
            }
            else
            {
                count = startIndex != -1 ? i - startIndex : 0;
                if (Regex.Match(strBld.ToString(), strPattern, RegexOptions.IgnoreCase) != Match.Empty)
                {
                    var str = Regex.Replace(strBld.ToString(), strPattern, tagValue, RegexOptions.IgnoreCase);
                    ModifyParagraph(paragraph, str, startIndex, count);
                }
                strBld.Clear();
                startIndex = -1;
            }
        }
    }

    /// <summary>
    /// This method used to replace a specified tag with image in different parts
    /// of the paragraph. It's essential for the Tag method which
    /// provides the functionality of replacing tags in the document.
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="img"></param>
    private static void ReplaceImageTagInParagraph(Paragraph paragraph, OpenXmlElement img)
    {
        paragraph.RemoveAllChildren<Run>();

        if (img != null) paragraph.AppendChild(new Run(img));
    }

    /// <summary>
    /// This method used to replace a specified tag with text in different parts
    /// of the paragraph. It's essential for the Tag method which
    /// provides the functionality of replacing tags in the document.
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="tag"></param>
    /// <param name="tagValue"></param>
    private static void ReplaceTagInParagraph(Paragraph paragraph, string tag, string tagValue)
    {
        var strBld = new StringBuilder("");

        foreach (var run in paragraph.ChildElements.OfType<Run>())
        {
            foreach (var element in run.ChildElements)
            {
                if (element is TabChar)
                    strBld.Append('\t');
                else
                    strBld.Append(element.InnerText);
            }
        }

        var strPattern = $@"\${tag}\$";

        if (Regex.Match(strBld.ToString(), strPattern, RegexOptions.IgnoreCase).Length == 0) return;
        var str = Regex.Replace(strBld.ToString(), strPattern, tagValue, RegexOptions.IgnoreCase);
        ModifyParagraph(paragraph, str);
    }
    
    /// <summary>
    /// This method used to replace a specified tag with text in different parts
    /// of the paragraph. It's essential for the Tag method which
    /// provides the functionality of replacing tags in the document.
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="tagName"></param>
    /// <param name="toString"></param>
    private static void ReplaceTagInFooterParagraph(Paragraph paragraph, string tagName, string toString)
    {
        foreach (var run in paragraph.ChildElements.OfType<Run>())
        {
            if (!run.InnerText.Contains(tagName)) continue;

            var choice = run.GetFirstChild<AlternateContent>()?.GetFirstChild<AlternateContentChoice>();
            //ReSharper disable once PossibleNullReferenceException
            if (choice != null)
                foreach (var par in choice.GetFirstChild<Drawing>().GetFirstChild<Anchor>().GetFirstChild<Graphic>().GraphicData.GetFirstChild<WordprocessingShape>().GetFirstChild<TextBoxInfo2>().TextBoxContent.ChildElements.OfType<Paragraph>())
                {
                    ReplaceTagInParagraph(par, tagName, toString);
                }

            var fallback = run.GetFirstChild<AlternateContent>()?.GetFirstChild<AlternateContentFallback>();

            if (fallback != null)
                foreach (var par in fallback.GetFirstChild<Picture>().GetFirstChild<Shape>().GetFirstChild<TextBox>().GetFirstChild<TextBoxContent>().ChildElements.OfType<Paragraph>())
                {
                    ReplaceTagInParagraph(par, tagName, toString);
                }

            ReplaceTagInParagraphMod(paragraph, tagName, toString);
        }
    }

    /// <summary>
    /// This method changes the text within a paragraph to the suggested one.
    /// It is used by other methods to replace the text corresponding to the
    /// tag with the given text.
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="str"></param>
    /// <exception cref="ArgumentNullException">paragraph must be not null</exception>
    private static void ModifyParagraph(Paragraph paragraph, string str)
    {
        if (paragraph == null) throw new ArgumentNullException(nameof(paragraph));
        if (str == null) return;
        var run = new Run();
        var text = new Text { Text = str };

        var runProperties = paragraph.ChildElements.OfType<Run>()
            ?.FirstOrDefault(_ => _.ChildElements.OfType<RunProperties>().Any())
            ?.GetFirstChild<RunProperties>();
        if (runProperties != null) run.Append(runProperties.CloneNode(true));

        run.Append(text);
        if (paragraph.Elements<Run>().Any()) paragraph.RemoveAllChildren<Run>();
        paragraph.Append(run);
    }
    
    /// <summary>
    /// This method changes the text within a paragraph to the suggested one.
    /// It is used by other methods to replace the text corresponding to the
    /// tag with the given text.
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="str"></param>
    /// <param name="startIndex"></param>
    /// <param name="count"></param>
    /// <exception cref="ArgumentNullException">paragraph must be not null</exception>
    private static void ModifyParagraph(Paragraph paragraph, string str, int startIndex, int count)
    {
        if (paragraph == null) throw new ArgumentNullException(nameof(paragraph));
        if (str == null) return;

        if (count == 0) return;


        var elements = paragraph.Elements().Skip(startIndex).Take(count).ToList();
        var properties = elements.FirstOrDefault(_ => _.ChildElements.OfType<RunProperties>().Any())?.ChildElements.OfType<RunProperties>().FirstOrDefault()?.CloneNode(true);

        var run = new Run();
        var text = new Text { Text = str };
        if (properties != null)
            run.Append(properties);
        run.Append(text);

        foreach (var element in elements)
        {
            paragraph.RemoveChild(element);
        }
        paragraph.InsertAt(run, startIndex);
    }

    /// <summary>
    /// This method is used to insert data into a table in the document.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="data"></param>
    /// <param name="isDynamic"></param>
    private void InsertDataToTable(string column, IEnumerable<string> data, bool isDynamic)
    {
        if (_document == null) return;
        var body = _document.MainDocumentPart.Document.Body;
        IEnumerable<string> strings = data as string[] ?? data.ToArray();
        foreach (var table in body.Elements<Table>())
        {
            var rowNumber = 0;
            
            foreach (var tableRow in table.Elements<TableRow>())
            {
                foreach (var tableCell in tableRow.Elements<TableCell>())
                {
                    if (!CheckIsMarkedColumn(tableCell, column)) continue;

                    //cellNumber is returned given the GridSpan
                    var cellNumber = GetCellNumber(tableRow.Elements<TableCell>(), tableCell);
                    InsertDataToTableBeginningWith(table, rowNumber, cellNumber, strings, isDynamic);
                    
                }
                rowNumber++;
            }
        }
    }

    /// <summary>
    /// This method is used to insert data into a table in the document.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="data"></param>
    /// <param name="isDynamic"></param>
    private static void InsertDataToTableBeginningWith(Table table, int rowNumber, int cellNumber, IEnumerable<string> data, bool isDynamicTable)
    {
        IEnumerable<string> strings = data as string[] ?? data.ToArray();

        var deltaRowNumber = 0; //A variable is needed to shift the row number if the FIXED table has vertically merged cells

        var tableRowCount = table.Elements<TableRow>().Count();
        var emplyRow = table.Elements<TableRow>().ElementAt(rowNumber).Clone() as TableRow;
        var cells = emplyRow?.Elements<TableCell>();
        ClearTextInCells(cells);
        var emplyLastRow = table.Elements<TableRow>().ElementAt(tableRowCount - 1).Clone() as TableRow;
        cells = emplyLastRow?.Elements<TableCell>();
        ClearTextInCells(cells);

        for (var i = 0; i < strings.Count(); i++)
        {
            if (i + rowNumber + deltaRowNumber >= table.Elements<TableRow>().Count())
            {
                if (isDynamicTable)
                {
                    if (i == strings.Count() - 1)
                        table.Append(emplyLastRow.Clone() as TableRow);
                    else
                        table.Append(emplyRow.Clone() as TableRow);
                }
                else break;
            }
            else if (i + rowNumber + deltaRowNumber == tableRowCount - 1)
            //Last row of the table
            {
                if (i < strings.Count() - 1 && rowNumber != tableRowCount - 1 && isDynamicTable)
                //Not the last line of data
                {
                    //The last row is replaced by an intermediate empty row
                    table.ReplaceChild(emplyRow.Clone() as TableRow,
                        table.Elements<TableRow>().ElementAt(i + rowNumber + deltaRowNumber));
                }

            }
            //In fixed tables, we can insert data into merged cells

            var cell = GetCell(
                table.Elements<TableRow>().ElementAt(i + rowNumber + deltaRowNumber).Elements<TableCell>(),
                cellNumber);

            var mergeCount = 0;
            if (!isDynamicTable)
            {
                //Check if the cell is a continuation of the combined cell
                if (cell.TableCellProperties.VerticalMerge != null &&
                    (cell.TableCellProperties.VerticalMerge.Val?.Value == MergedCellValues.Continue ||
                     cell.TableCellProperties.VerticalMerge.Val?.Value == MergedCellValues.Restart))
                {
                    while (i + rowNumber + deltaRowNumber + mergeCount + 1 < tableRowCount)
                    {
                        cell = GetCell(
                            table.Elements<TableRow>().ElementAt(i + rowNumber + deltaRowNumber + mergeCount + 1)
                                .Elements<TableCell>(), cellNumber);
                        if (cell.TableCellProperties.VerticalMerge != null &&
                            (cell.TableCellProperties.VerticalMerge.Val?.Value == MergedCellValues.Continue ||
                             cell.TableCellProperties.VerticalMerge.Val == null))
                            mergeCount++;
                        else break;
                    }
                }
            }

            CopyRunToParagraph(table, rowNumber + i + deltaRowNumber, cellNumber, rowNumber);
            ModifyParagraph(
                GetCell(table.Elements<TableRow>().ElementAt(i + rowNumber + deltaRowNumber).Elements<TableCell>(),
                        cellNumber)
                    .GetFirstChild<Paragraph>(), strings.ElementAt(i));

            deltaRowNumber += mergeCount;
        }
    }

    /// <summary>
    /// This method performs the insertion of a specified tag into the table
    /// of a document.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    private static void InsertTagToTable(WordprocessingDocument document, string tag, string value)
    {
        if (document == null) return;

        var tables = document.MainDocumentPart.Document.Body.Elements<Table>();

        IEnumerable<Paragraph> paragraphs = new List<Paragraph>();
        paragraphs = tables.Aggregate(paragraphs, (current2, table) => table.Elements<TableRow>()
            .Aggregate(current2, (current1, tableRow) => tableRow.Elements<TableCell>()
                .Aggregate(current1, (current, tableCell) => current.Concat(tableCell.Elements<Paragraph>()
                    .Where(_ => _.InnerText.Contains('$' + tag + '$'))))));

        foreach (var paragraph in paragraphs)
        {
            ReplaceTagInParagraph(paragraph, tag, value);
        }
    }

    /// <summary>
    /// This method is used to clear the text in the cells of a table
    /// and a run, respectively.
    /// </summary>
    /// <param name="cells"></param>
    private static void ClearTextInCells(IEnumerable<TableCell> cells)
    {
        foreach (var cell in cells)
        foreach (var paragpaph in cell.Elements<Paragraph>())
        foreach (var run in paragpaph.Elements<Run>())
            ClearTextInRun(run);
    }

    /// <summary>
    /// This method is used to clear the text in the cells of a table
    /// and a run, respectively.
    /// </summary>
    /// <param name="run"></param>
    private static void ClearTextInRun(Run run)
    {
        if (run.Elements<Text>().Any())
            run.RemoveAllChildren<Text>();
        var text = new Text { Text = "" };
        run.Append(text);
    }
    
    /// <summary>
    /// This is helper method used for dealing with tables.
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="cell"></param>
    /// <returns>It returns the cell number and a reference to a cell.</returns>
    private static int GetCellNumber(IEnumerable<TableCell> cells, TableCell cell)
    {
        var cellNumber = 0;

        foreach (var c in cells)
        {
            if (c == cell) break;

            if (c.TableCellProperties.GridSpan?.Val != null)
                cellNumber += c.TableCellProperties.GridSpan.Val.Value;
            else
                cellNumber++;
        }

        return cellNumber;
    }

    /// <summary>
    /// This is helper method used for dealing with tables.
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="cellNum"></param>
    /// <returns>It returns a reference to a cell.</returns>
    private static TableCell GetCell(IEnumerable<TableCell> cells, int cellNum)
    {
        var index = 0;
        foreach (var cell in cells)
        {
            if (index == cellNum)
                return cell;

            if (cell.TableCellProperties.GridSpan?.Val != null)
                index += cell.TableCellProperties.GridSpan.Val.Value;
            else
                index++;
        }
        return null;
    }
    
    /// <summary>
    /// This method creates directories in the specified path if
    /// they don't exist yet.
    /// </summary>
    /// <param name="templFilePath"></param>
    private static void CreateDirectory(string templFilePath)
    {
        //If there are folders in the template path (in templFilePath), then when unloading, we also create folders
        var directoryName = Path.GetDirectoryName(templFilePath);

        if (!string.IsNullOrWhiteSpace(directoryName) && Directory.Exists(directoryName) == false)
            Directory.CreateDirectory(directoryName);
    }
    
    /// <summary>
    /// This method copies a run (a block of text) from an old location
    /// to a new one.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="newRowNumber"></param>
    /// <param name="cellNumber"></param>
    /// <param name="oldRowNumber"></param>
    private static void CopyRunToParagraph(Table table, int newRowNumber, int cellNumber, int oldRowNumber)
    {
        var cell = GetCell(table.Elements<TableRow>().ElementAt(newRowNumber).Elements<TableCell>(), cellNumber);

        if (cell.GetFirstChild<Paragraph>().Elements<Run>().Any()) return;

        var oldCell = GetCell(table.Elements<TableRow>().ElementAt(oldRowNumber).Elements<TableCell>(), cellNumber);

        cell.GetFirstChild<Paragraph>().Append(oldCell.GetFirstChild<Paragraph>().GetFirstChild<Run>().Clone() as Run);

        ClearTextInRun(cell.GetFirstChild<Paragraph>().GetFirstChild<Run>());
    }
    
    /// <summary>
    /// This method checks whether the specified table cell contains
    /// the specified tag.
    /// </summary>
    /// <param name="tableCell"></param>
    /// <param name="column"></param>
    /// <returns>Returns true if contains.</returns>
    private static bool CheckIsMarkedColumn(TableCell tableCell, string column)
    {
        var strBld = new StringBuilder("");
        var strPattern = $@"\${column}\$";
        foreach (var paragraph in tableCell.Elements<Paragraph>())
            foreach (var run in paragraph.Elements<Run>())
                strBld.Append(run.InnerText);

        return Regex.Match(strBld.ToString(), strPattern, RegexOptions.IgnoreCase).Length != 0;
    }

    /// <summary>
    /// Removes ProofError elements from a paragraph.
    /// </summary>
    /// <param name="paragraph"></param>
    private static void RemoveProofErrorFromParagraph(OpenXmlElement paragraph)
    {
        var error = paragraph.ChildElements.OfType<ProofError>().ToList();
        foreach (var er in error)
        {
            paragraph.RemoveChild(er);
        }
    }
}