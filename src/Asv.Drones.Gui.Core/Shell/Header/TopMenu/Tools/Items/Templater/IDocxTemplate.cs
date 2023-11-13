namespace Asv.Drones.Gui.Core;

public interface IDocxTemplate : IDisposable
{
    /// <summary>
    /// Insert an image referenced either by a file path or a memory stream
    /// at a location marked by a tag in the document.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="imgFileName">path to image</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void Image(string tag, string path);
    /// <summary>
    /// Insert an image referenced either by a file path or a memory stream
    /// at a location marked by a tag in the document. 
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="imageStream">image stream</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void Image(string tag, MemoryStream imageStream);
    /// <summary>
    /// This method is used to replace a specific tag
    /// with a provided text in headers, footers, and the document body.
    /// This could be useful for things like template placeholders.
    /// </summary>
    /// <param name="tagName">what to replace</param>
    /// <param name="toString">replace string</param>
    /// <returns>Returns replacement result.</returns>
    bool Tag(string tagName, string value);
    /// <summary>
    /// This method saves any modifications to the document.
    /// </summary>
    /// <param name="filePath"></param>
    void Save(string filePath);
    /// <summary>
    /// This method tries to insert the data into the existing table.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="data"></param>
    void FixedTable(string column, IEnumerable<string> data);
    /// <summary>
    /// This method can expand the table if the data size exceeds the table size.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="data"></param>
    void DynamicTable(string column, IEnumerable<string> data);
}