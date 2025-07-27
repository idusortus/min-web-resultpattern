using Api.Application.Abstractions;
using Api.Domain.Entities;
using Api.Extensions;
using Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Features.ResultQuotes;

public static class GetQuotes
{
    public record GetQuotesQuery(int PageNumber, int PageSize) : IRequest<Result<PaginatedResult<Quote>>>;

    public class Validator : AbstractValidator<GetQuotesQuery>
    {
        public Validator()
        {
            RuleFor(n => n.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page Number must be a positive integer");
            RuleFor(s => s.PageSize)
                .GreaterThan(0)
                .WithMessage("Page Size must be a positive integer.");
        }
    }

    public class Handler(AppDbContext context) : IRequestHandler<GetQuotesQuery, Result<PaginatedResult<Quote>>>
    {
        public async Task<Result<PaginatedResult<Quote>>> Handle(GetQuotesQuery request, CancellationToken ct)
        {
            var pagination = new PaginationParams
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var query = context.Quotes.OrderBy(q => q.Id);
            var result = await query.ToPaginatedResultAsync(pagination, ct);
            return Result.Success(result);
        }
    }

    public class GetQuotesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("results/quotes", async (
                ISender sender,
                CancellationToken ct,
                [FromQuery] int pNumber = 1,
                [FromQuery] int pSize = 10) =>
            {
                var result = await sender.Send(new GetQuotesQuery(pNumber, pSize));
                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("resultpattern"); 
        }
    }
}