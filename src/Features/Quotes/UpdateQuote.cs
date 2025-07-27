using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Extensions;
using Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using SharedKernel;

namespace Api.Features.Quotes;

public static class UpdateQuote
{
    public sealed record UpdateQuoteCommand(int Id, string Author, string Content) : IRequest<Result<Quote>>;

    public class Validator : AbstractValidator<UpdateQuoteCommand>
    {
        public Validator()
        {
            RuleFor(a => a.Author)
                .MinimumLength(5)
                .WithMessage("Author must contain 5 or more characters.");
            RuleFor(c => c.Content)
                .MinimumLength(5)
                .WithMessage("Content must contain 5 or more characters.");
        }
    }

    public class Handler(AppDbContext context) : IRequestHandler<UpdateQuoteCommand, Result<Quote>>
    {
        public async Task<Result<Quote>> Handle(UpdateQuoteCommand command, CancellationToken ct)
        {
            var quote = await context.Quotes.FindAsync(command.Id, ct);
            if (quote is null)
                return Result.Failure<Quote>(Error.NotFound(
                    "Quote.NotFound",
                    $"The quote with the Id = {command.Id} was not found."));

            quote.Author = command.Author;
            quote.Content = command.Content;
            await context.SaveChangesAsync(ct);
            return Result.Success(quote);
        }
    }

    public class UpdateQuoteEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("quotes", async (ISender sender, UpdateQuoteCommand command, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags("Quotes");
        }
    }
}