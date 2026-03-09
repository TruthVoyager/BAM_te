using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    /// <summary>Normalizes a date to UTC midnight for consistent storage and comparison.</summary>
    internal static class DateUtc
    {
        public static DateTime ToUtcDate(DateTime value) =>
            new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
    }

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

            if (person is null) throw new BadHttpRequestException("Person not found.", StatusCodes.Status400BadRequest);

            var requestDate = DateUtc.ToUtcDate(request.DutyStartDate);
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

            if (request.RankId <= 0)
                throw new BadHttpRequestException("A valid rank is required.", StatusCodes.Status400BadRequest);

            var rankExists = await _context.Ranks.AnyAsync(r => r.Id == request.RankId, cancellationToken);
            if (!rankExists)
                throw new BadHttpRequestException("The selected rank was not found.", StatusCodes.Status400BadRequest);

            var requestedStart = DateUtc.ToUtcDate(request.DutyStartDate);
            var todayUtc = DateTime.UtcNow.Date;
            if (requestedStart > todayUtc)
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
                    CareerStartDate = requestedStart
                };
                if (string.Equals(request.DutyTitle, "RETIRED", StringComparison.OrdinalIgnoreCase))
                    astronautDetail.CareerEndDate = requestedStart.AddDays(-1);

                await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
            }
            else
            {
                if (string.Equals(request.DutyTitle, "RETIRED", StringComparison.OrdinalIgnoreCase))
                    astronautDetail.CareerEndDate = requestedStart.AddDays(-1);
                _context.AstronautDetails.Update(astronautDetail);
            }

            var currentDuty = await _context.AstronautDuties
                .Where(d => d.PersonId == person.Id && d.DutyEndDate == null)
                .OrderByDescending(d => d.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentDuty != null)
            {
                var currentStart = currentDuty.DutyStartDate.Kind == DateTimeKind.Utc
                    ? currentDuty.DutyStartDate.Date
                    : DateUtc.ToUtcDate(currentDuty.DutyStartDate);
                var minNewStartDate = currentStart.AddDays(1);
                if (requestedStart < minNewStartDate)
                {
                    var message = currentStart == todayUtc
                        ? "The current duty was assigned today. A new duty or retirement cannot be assigned until tomorrow."
                        : "The new duty cannot start on the same day as the current duty or any date before it. " +
                          $"Current duty started {currentStart:yyyy-MM-dd} UTC; the new duty must start on or after {minNewStartDate:yyyy-MM-dd} UTC.";
                    throw new BadHttpRequestException(message, StatusCodes.Status400BadRequest);
                }

                currentDuty.DutyEndDate = requestedStart.AddDays(-1);
                _context.AstronautDuties.Update(currentDuty);
            }

            var newAstronautDuty = new AstronautDuty
            {
                PersonId = person.Id,
                RankId = await GetRankIdForNewDutyAsync(request.RankId, hasAnyDuty, request.DutyTitle, cancellationToken),
                DutyTitle = request.DutyTitle,
                DutyStartDate = requestedStart,
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
