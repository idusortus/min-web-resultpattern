using Api.Application.Abstractions;
using Api.Extensions;
using Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using SharedKernel;

namespace Api.Features.ResultQuotes;

public static class DeleteQuote
{
    public sealed record DeleteQuoteCommand(int id):IRequest<Result>;

    public class Validator : AbstractValidator<DeleteQuoteCommand>
    {
        public Validator()
        {
            RuleFor(c => c.id)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Id must be a positive integer");
        }
    }

    public class Handler(AppDbContext context) : IRequestHandler<DeleteQuoteCommand, Result>
    {
        public async Task<Result> Handle(DeleteQuoteCommand command, CancellationToken ct)
        {
            var quote = await context.Quotes.FindAsync(command.id, ct);
            return (quote is null)
                ? Result.Failure(Error.NotFound("NOT_FOUND", "The requested quote id does not exist"))
                : Result.Success("Something");            
        }
    }

    public class DeleteQuoteEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("results/quotes/{id:int}", async (ISender sender, int id, CancellationToken ct) =>
            {
                var result = await sender.Send(new DeleteQuoteCommand(id), ct);
                return result.Match(
                    Results.NoContent,
                    CustomResults.Problem
                );
            })
            .WithTags("resultpattern");
        }
    }
}