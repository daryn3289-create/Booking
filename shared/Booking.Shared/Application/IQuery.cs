namespace Booking.Shared.Application;

public interface IQuery
{
}

public interface IQuery<out TResponse>
{
}

public interface IQueryHandler<in TQuery>
    where TQuery : IQuery
{
    Task Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}