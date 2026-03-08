using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
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
                    CurrentDutyTitle = p.AstronautDuties
                        .Where(d => d.DutyEndDate == null)
                        .OrderByDescending(d => d.DutyStartDate)
                        .Select(d => d.DutyTitle)
                        .FirstOrDefault() ?? string.Empty
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (person is null)
                return new GetAstronautDutiesByNameResult { Person = null, AstronautDuties = new List<AstronautDutyDto>() };

            var duties = await _context.AstronautDuties
                .AsNoTracking()
                .Where(d => d.PersonId == person.PersonId)
                .OrderByDescending(d => d.DutyStartDate)
                .Select(d => new AstronautDutyDto
                {
                    Id = d.Id,
                    PersonId = d.PersonId,
                    RankId = d.RankId,
                    RankLevel = d.Rank != null ? d.Rank.Level : 0,
                    RankName = d.Rank != null ? d.Rank.Name : string.Empty,
                    DutyTitle = d.DutyTitle,
                    DutyStartDate = d.DutyStartDate,
                    DutyEndDate = d.DutyEndDate
                })
                .ToListAsync(cancellationToken);

            return new GetAstronautDutiesByNameResult
            {
                Person = person,
                AstronautDuties = duties
            };
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDutyDto> AstronautDuties { get; set; } = new List<AstronautDutyDto>();
    }
}
