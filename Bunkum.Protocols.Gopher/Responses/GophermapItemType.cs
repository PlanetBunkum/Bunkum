namespace Bunkum.Protocols.Gopher.Responses;

public static class GophermapItemType
{
    #region Canonical types

        /// <summary>
    /// Item is a file
    /// </summary>
    public static readonly char File = '0';
    
    /// <summary>
    /// Item is a directory
    /// </summary>
    public static readonly char Directory = '1';
    
    /// <summary>
    /// Item is a CSO phone-book server
    /// </summary>
    public static readonly char CsoPhonebookServer = '2';
    
    /// <summary>
    /// Error
    /// </summary>
    public static readonly char Error = '3';
    
    /// <summary>
    /// Item is a BinHexed Macintosh file.
    /// </summary>
    public static readonly char BinHexMacintosh = '4';
    
    /// <summary>
    /// Item is DOS binary archive of some sort.
    /// </summary>
    /// <remarks>
    /// Client must read until the TCP connection closes.
    /// </remarks>
    public static readonly char DosBinary = '5';
    
    /// <summary>
    /// Item is a UNIX uuencoded file.
    /// </summary>
    public static readonly char UnixUuEncoded = '6';
    
    /// <summary>
    /// Item is an Index-Search server.
    /// </summary>
    public static readonly char IndexSearchServer = '7';
    
    /// <summary>
    /// Item points to a text-based telnet session.
    /// </summary>
    public static readonly char TelnetServer = '8';
    
    /// <summary>
    /// Item is a binary file!
    /// </summary>
    /// <remarks>
    /// Client must read until the TCP connection closes.
    /// </remarks>
    public static readonly char Binary = '9';
    
    /// <summary>
    /// Item is a redundant server
    /// </summary>
    public static readonly char RedundantServer = '+';

    /// <summary>
    /// Item points to a text-based tn3270 session.
    /// </summary>
    public static readonly char Tn3270 = 'T';

    /// <summary>
    /// Item is a GIF format graphics file.
    /// </summary>
    public static readonly char Gif = 'g';
    
    /// <summary>
    /// Item is some kind of image file.
    /// </summary>
    public static readonly char Image = 'I';

    #endregion

    #region gopher+ types

    /// <summary>
    /// Item is a bitmap image.
    /// </summary>
    public static readonly char BitmapImage = ':';
    
    /// <summary>
    /// Item is a movie file.
    /// </summary>
    public static readonly char MovieFile = ';';
    
    /// <summary>
    /// Item is an audio file.
    /// </summary>
    public static readonly char SoundFile = '<';

    #endregion

    #region Non-canonical types

    /// <summary>
    /// Item is a document.
    /// </summary>
    public static readonly char Document = 'd';
    
    /// <summary>
    /// Item is an HTML document.
    /// </summary>
    public static readonly char HtmlDocument = 'h';
    
    /// <summary>
    /// Item is an XML document.
    /// </summary>
    public static readonly char XmlDocument = 'X';
    
    /// <summary>
    /// Item is a PDF document.
    /// </summary>
    public static readonly char PdfDocument = 'P';
    
    /// <summary>
    /// Item is an informative message.
    /// </summary>
    public static readonly char Message = 'i';
    
    /// <summary>
    /// Item is a rich text format file.
    /// </summary>
    public static readonly char RichText = 'r';

    #endregion
}