namespace Bunkum.Protocols.Gopher.Responses;

public enum GophermapItemType : byte
{
    #region Canonical types

    /// <summary>
    /// Item is a file
    /// </summary>
    File = (byte)'0',
    
    /// <summary>
    /// Item is a directory
    /// </summary>
    Directory = (byte)'1',
    
    /// <summary>
    /// Item is a CSO phone-book server
    /// </summary>
    CsoPhonebookServer = (byte)'2',
    
    /// <summary>
    /// Error
    /// </summary>
    Error = (byte)'3',
    
    /// <summary>
    /// Item is a BinHexed Macintosh file.
    /// </summary>
    BinHexMacintosh = (byte)'4',
    
    /// <summary>
    /// Item is DOS binary archive of some sort.
    /// </summary>
    /// <remarks>
    /// Client must read until the TCP connection closes.
    /// </remarks>
    DosBinary = (byte)'5',
    
    /// <summary>
    /// Item is a UNIX uuencoded file.
    /// </summary>
    UnixUuEncoded = (byte)'6',
    
    /// <summary>
    /// Item is an Index-Search server.
    /// </summary>
    IndexSearchServer = (byte)'7',
    
    /// <summary>
    /// Item points to a text-based telnet session.
    /// </summary>
    TelnetServer = (byte)'8',
    
    /// <summary>
    /// Item is a binary file!
    /// </summary>
    /// <remarks>
    /// Client must read until the TCP connection closes.
    /// </remarks>
    Binary = (byte)'9',
    
    /// <summary>
    /// Item is a redundant server
    /// </summary>
    RedundantServer = (byte)'+',

    /// <summary>
    /// Item points to a text-based tn3270 session.
    /// </summary>
    Tn3270 = (byte)'T',

    /// <summary>
    /// Item is a GIF format graphics file.
    /// </summary>
    Gif = (byte)'g',
    
    /// <summary>
    /// Item is some kind of image file.
    /// </summary>
    Image = (byte)'I',

    #endregion

    #region Non-canonical types

    /// <summary>
    /// Item is a document.
    /// </summary>
    Document = (byte)'d',
    
    /// <summary>
    /// Item is an HTML document.
    /// </summary>
    HtmlDocument = (byte)'h',
    
    /// <summary>
    /// Item is an XML document.
    /// </summary>
    XmlDocument = (byte)'X',
    
    /// <summary>
    /// Item is a PDF document.
    /// </summary>
    PdfDocument = (byte)'P',
    
    /// <summary>
    /// Item is an informative message.
    /// </summary>
    Message = (byte)'i',
    
    /// <summary>
    /// Item is a rich text format file.
    /// </summary>
    RichText = (byte)'r',

    /// <summary>
    /// Item is a bitmap image, most likely an image.
    /// </summary>
    ImageFile = (byte)'p',
    
    /// <summary>
    /// Item is an audio file.
    /// </summary>
    SoundFile = (byte)'s',

    #endregion
}