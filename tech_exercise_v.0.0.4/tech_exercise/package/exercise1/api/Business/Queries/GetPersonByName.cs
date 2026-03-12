using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;

        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadHttpRequestException("Name is required.", StatusCodes.Status400BadRequest);

            var person = await _context.People
                .AsNoTracking()
                .Where(p => p.Name == request.Name)
                .Select(p => new PersonAstronaut
                {
                    PersonId = p.Id,
                    Name = p.Name,
                    CareerStartDate = p.AstronautDetail != null ? p.AstronautDetail.CareerStartDate : null,
                    CareerEndDate = p.AstronautDetail != null ? p.AstronautDetail.CareerEndDate : null,
                    CurrentRank = p.AstronautDuties
                        .Where(d => d.DutyEndDate == null)
                        .OrderByDescending(d => d.DutyStartDate)
                        .Select(d => d.Rank != null ? d.Rank.Name : null)
                        .FirstOrDefault() ?? string.Empty,
                    CurrentRankLevel = p.AstronautDuties
                        .Where(d => d.DutyEndDate == null)
                        .OrderByDescending(d => d.DutyStartDate)
                        .Select(d => d.Rank != null ? d.Rank.Level : 0)
                        .FirstOrDefault(),
                    CurrentDutyTitle = p.AstronautDuties
                        .Where(d => d.DutyEndDate == null)
                        .OrderByDescending(d => d.DutyStartDate)
                        .Select(d => d.DutyTitle)
                        .FirstOrDefault() ?? string.Empty
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new GetPersonByNameResult { Person = person };
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
