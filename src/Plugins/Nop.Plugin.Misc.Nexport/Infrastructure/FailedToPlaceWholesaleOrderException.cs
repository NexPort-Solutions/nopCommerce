using System.Runtime.Serialization;

namespace Nop.Plugin.Misc.Nexport.Infrastructure;

[Serializable]
public class FailedToPlaceWholesaleOrderException : Exception
{
    public FailedToPlaceWholesaleOrderException(string? message)
        : base(message)
    { }

    public FailedToPlaceWholesaleOrderException(string? message, Exception? innerException)
        : base(message, innerException)
    { }

    protected FailedToPlaceWholesaleOrderException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}
