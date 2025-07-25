using Api.Application.Abstractions;
using Api.Infrastructure.Persistence;
using SharedKernel; 
using MediatR;
using Api.Extensions;
using FluentValidation;

namespace Api.Features.ResultQuotes;

public static class GetQuoteById
{
    // 1. Define a clear DTO for the successful response data.
    public record QuoteResponse(int Id, string Content, string Author);
    // 2. The Query now explicitly uses the generic Result<T> type.
    // It declares it will return a Result containing a QuoteResponse on success.
    public record Query(int id) : IRequest<Result<QuoteResponse>>;
    
    // NEW: Add the validator class
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.id)
                .GreaterThan(0)
                .WithMessage("The Quote ID must be a positive number.");
        }
    }
    // 4. The old discriminated union (HandlerResult, HappyResult, FailResult) is GONE.
    // It is no longer needed because Result<T> replaces it.
    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<QuoteResponse>>
    {
        // 5. The Handle method signature is updated to return the standardized Result.
        public async Task<Result<QuoteResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var quote = await context.Quotes.FindAsync(request.id, cancellationToken);

            if (quote is null)
            {
                // Return a standardized Failure result with a specific Error type.
                return Result.Failure<QuoteResponse>(Error.NotFound(
                    "Quote.NotFound", // A unique error code
                    $"The quote with the Id = {request.id} was not found."));
            }

            // On success, create the DTO and wrap it in a Success result.
            var response = new QuoteResponse(quote.Id, quote.Content, quote.Author);
            // The implicit operator makes this clean, but you could also write:
            // return Result.Success(response);
            return response;
        }
    }

    public class GetQuoteByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("results/quotes/{id:int}", async (ISender sender, int id) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags("resultpattern")
            .WithName("GetQuoteByIdWithResult");
        }
    }
}