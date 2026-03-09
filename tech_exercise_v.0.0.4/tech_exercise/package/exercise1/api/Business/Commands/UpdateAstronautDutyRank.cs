using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands;

public class UpdateAstronautDutyRankRequest
{
    public int RankId { get; set; }
}

public class UpdateAstronautDutyRank : IRequest<UpdateAstronautDutyRankResult>
{
    public int DutyId { get; set; }
    public int RankId { get; set; }
}

public class UpdateAstronautDutyRankHandler : IRequestHandler<UpdateAstronautDutyRank, UpdateAstronautDutyRankResult>
{
    private readonly StargateContext _context;

    public UpdateAstronautDutyRankHandler(StargateContext context)
    {
        _context = context;
    }

    public async Task<UpdateAstronautDutyRankResult> Handle(UpdateAstronautDutyRank request, CancellationToken cancellationToken)
    {
        var duty = await _context.AstronautDuties
            .FirstOrDefaultAsync(d => d.Id == request.DutyId, cancellationToken);

        if (duty is null)
            throw new InvalidOperationException("Duty not found.");

        if (duty.DutyEndDate != null)
            throw new BadHttpRequestException("Only the current duty (no end date) can be promoted.", StatusCodes.Status400BadRequest);

        var rankExists = await _context.Ranks.AnyAsync(r => r.Id == request.RankId, cancellationToken);
        if (!rankExists)
            throw new InvalidOperationException("Rank not found.");

        duty.RankId = request.RankId;
        _context.AstronautDuties.Update(duty);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateAstronautDutyRankResult { Id = duty.Id };
    }
}

public class UpdateAstronautDutyRankResult : BaseResponse
{
    public int Id { get; set; }
}
