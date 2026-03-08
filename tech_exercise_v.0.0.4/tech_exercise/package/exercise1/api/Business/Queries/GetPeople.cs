using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        private readonly StargateContext _context;

        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var people = await _context.People
                .AsNoTracking()
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
                .ToListAsync(cancellationToken);

            return new GetPeopleResult { People = people };
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut>();
    }
}
