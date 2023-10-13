using System.Text;
using System.Xml;

namespace Bunkum.Core.Responses.Serialization;

/// <summary>
/// A custom XML text writer that strips namespaces and attributes from elements.
/// </summary>
public class BunkumXmlTextWriter : XmlTextWriter
{
    public BunkumXmlTextWriter(Stream stream) : base(stream, Encoding.Default)
    { }

    public override void WriteEndElement()
    {
        base.WriteFullEndElement();
    }

    public override Task WriteEndElementAsync()
    {
        return base.WriteFullEndElementAsync();
    }

    public override void WriteStartDocument() {}
    public override void WriteStartDocument(bool standalone) {}
    
    private bool _skipNextAttribute;
    
    public override void WriteStartAttribute(string? prefix, string localName, string? ns)
    {
        if (!string.IsNullOrEmpty(ns))
        {
            this._skipNextAttribute = true;
            return;
        }

        base.WriteStartAttribute(prefix, localName, ns);
        this._skipNextAttribute = false;
    }

    public override void WriteEndAttribute()
    {
        if (this._skipNextAttribute)
        {
            this._skipNextAttribute = false;
            return;
        }

        base.WriteEndAttribute();
    }

    public override void WriteString(string? text)
    {
        if (!this._skipNextAttribute)
        {
            base.WriteString(text);
        }
    }
}