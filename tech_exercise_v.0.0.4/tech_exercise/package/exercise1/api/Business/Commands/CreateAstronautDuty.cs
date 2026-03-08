using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public int RankId { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is null) throw new BadHttpRequestException("Bad Request");

            var requestDate = request.DutyStartDate.Date;
            var nextDay = requestDate.AddDays(1);
            var verifyNoDuplicateForPerson = await _context.AstronautDuties
                .AsNoTracking()
                .AnyAsync(z => z.PersonId == person.Id && z.DutyTitle == request.DutyTitle && z.DutyStartDate >= requestDate && z.DutyStartDate < nextDay, cancellationToken);

            if (verifyNoDuplicateForPerson)
                throw new BadHttpRequestException("This person already has a duty with this title and start date.", StatusCodes.Status400BadRequest);
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);
            if (person is null)
                throw new InvalidOperationException("Person not found.");

            var requestedStart = request.DutyStartDate.Date;
            var today = DateTime.Today;
            if (requestedStart > today)
                throw new BadHttpRequestException(
                    "A duty cannot have a start date in the future. The start date must be today or earlier.",
                    StatusCodes.Status400BadRequest);

            var hasAnyDuty = await _context.AstronautDuties
                .AnyAsync(d => d.PersonId == person.Id, cancellationToken);

            var astronautDetail = await _context.AstronautDetails
                .FirstOrDefaultAsync(a => a.PersonId == person.Id, cancellationToken);

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail
                {
                    PersonId = person.Id,
                    CareerStartDate = request.DutyStartDate.Date
                };
                if (request.DutyTitle == "RETIRED")
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;

                await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
            }
            else
            {
                if (request.DutyTitle == "RETIRED")
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDetails.Update(astronautDetail);
            }

            var currentDuty = await _context.AstronautDuties
                .Where(d => d.PersonId == person.Id && d.DutyEndDate == null)
                .OrderByDescending(d => d.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentDuty != null)
            {
                var currentStart = currentDuty.DutyStartDate.Date;
                var minNewStartDate = currentStart.AddDays(1);
                if (request.DutyStartDate.Date < minNewStartDate)
                {
                    var message = currentStart == DateTime.Today
                        ? "The current duty was assigned today. A new duty or retirement cannot be assigned until tomorrow."
                        : "The new duty cannot start on the same day as the current duty or any date before it. " +
                          $"Current duty started {currentStart:yyyy-MM-dd}; the new duty must start on or after {minNewStartDate:yyyy-MM-dd}.";
                    throw new BadHttpRequestException(message, StatusCodes.Status400BadRequest);
                }

                currentDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(currentDuty);
            }

            var newAstronautDuty = new AstronautDuty
            {
                PersonId = person.Id,
                RankId = await GetRankIdForNewDutyAsync(request.RankId, hasAnyDuty, request.DutyTitle, cancellationToken),
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new CreateAstronautDutyResult { Id = newAstronautDuty.Id };
        }

        private async Task<int> GetRankIdForNewDutyAsync(int requestRankId, bool hasAnyDuty, string dutyTitle, CancellationToken cancellationToken)
        {
            if (!hasAnyDuty && string.Equals(dutyTitle, "RETIRED", StringComparison.OrdinalIgnoreCase))
            {
                var lowestRank = await _context.Ranks
                    .OrderBy(r => r.Level)
                    .FirstOrDefaultAsync(cancellationToken);
                return lowestRank?.Id ?? requestRankId;
            }
            return requestRankId;
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
