using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string CurrentName { get; set; } = string.Empty;
        public required string NewName { get; set; } = string.Empty;
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.CurrentName, cancellationToken);
            if (person is null)
                throw new InvalidOperationException("Person not found.");

            if (request.NewName.Trim() != request.CurrentName)
            {
                var exists = await _context.People.AnyAsync(p => p.Name == request.NewName.Trim(), cancellationToken);
                if (exists)
                    throw new InvalidOperationException("A person with that name already exists.");
                person.Name = request.NewName.Trim();
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new UpdatePersonResult { Id = person.Id };
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
