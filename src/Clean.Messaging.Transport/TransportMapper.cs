namespace Clean.Messaging.Transport;

public interface ITransportMapper<in TSource, out TDestination>
{
    TDestination Map(
        TSource source);
}