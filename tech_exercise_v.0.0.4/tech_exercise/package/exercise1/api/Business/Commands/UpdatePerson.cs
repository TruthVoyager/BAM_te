using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public string CurrentName { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
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
            if (string.IsNullOrWhiteSpace(request.CurrentName))
                throw new InvalidOperationException("Current name is required.");

            var trimmedNewName = request.NewName?.Trim() ?? string.Empty;
            if (trimmedNewName.Length == 0)
                throw new InvalidOperationException("New name is required.");

            var person = await _context.People.FirstOrDefaultAsync(p => p.Name == request.CurrentName.Trim(), cancellationToken);
            if (person is null)
                throw new InvalidOperationException("Person not found.");

            if (trimmedNewName != request.CurrentName.Trim())
            {
                var exists = await _context.People.AnyAsync(p => p.Name == trimmedNewName, cancellationToken);
                if (exists)
                    throw new InvalidOperationException("A person with that name already exists.");
                person.Name = trimmedNewName;
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
