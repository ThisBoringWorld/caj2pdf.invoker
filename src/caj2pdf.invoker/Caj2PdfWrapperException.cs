using System.Runtime.Serialization;

namespace caj2pdf;

[Serializable]
public class Caj2PdfWrapperException : Exception
{
    protected Caj2PdfWrapperException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public Caj2PdfWrapperException()
    {
    }

    public Caj2PdfWrapperException(string? message) : base(message)
    {
    }

    public Caj2PdfWrapperException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
