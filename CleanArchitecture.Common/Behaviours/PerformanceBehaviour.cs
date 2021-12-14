namespace CleanArchitecture.Common.Behaviours
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly Stopwatch timer;

        public PerformanceBehaviour() => this.timer = new Stopwatch();

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            this.timer.Start();

            var response = await next();

            this.timer.Stop();

            var elapsedMilliseconds = this.timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                Logging.Logging.LogInfo($"CleanArchitecture Long Running Request: {typeof(TRequest).Name} ({elapsedMilliseconds} milliseconds)k", request);
            }

            return response;
        }
    }
}