namespace CleanArchitecture.Common.Behaviours
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR.Pipeline;

    public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    {
        public async Task Process(TRequest request, CancellationToken cancellationToken)
            => Logging.Logging.LogInfo($"CleanArchitecture Request: {typeof(TRequest).Name}", request);
    }
}