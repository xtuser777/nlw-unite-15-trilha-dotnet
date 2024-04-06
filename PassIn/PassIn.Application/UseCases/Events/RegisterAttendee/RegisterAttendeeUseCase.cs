using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;

public class RegisterAttendeeUseCase
{
    private readonly PassInDbContext _context;

    public RegisterAttendeeUseCase()
    {
        _context = new PassInDbContext();
    }

    public ResponseRegisteredEventJson Execute(Guid id, RequestRegisterEventJson request)
    {
        Validate(id, request);

        var entity = new Infrastructure.Entities.Attendee()
        {
            Name = request.Name,
            Email = request.Email,
            Event_Id = id,
            Created_At = DateTime.UtcNow,
        };

        _context.Attendees.Add(entity);
        _context.SaveChanges();

        return new ResponseRegisteredEventJson()
        {
            Id = entity.Id,
        };
    }

    private void Validate(Guid id, RequestRegisterEventJson request)
    {
        var eventExist = _context.Events.Find(id);
        if (eventExist is null) 
        {
            throw new NotFoundException("Event with this id not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ErrorOnValidationException("This name is invalid.");
        }

        if (!EmailIsValid(request.Email))
        {
            throw new ErrorOnValidationException("This email is invalid.");
        }

        var alreadyRegistered = _context.Attendees.Any(a => a.Email.Equals(request.Email) && a.Id == id);
        if (alreadyRegistered)
        {
            throw new ConflictException("Already registered.");
        }

        var attendees = _context.Attendees.Count(a => a.Event_Id == id);
        if (attendees >= eventExist.Maximum_Attendees) 
        {
            throw new ErrorOnValidationException("Event out of limit.");
        }
    }

    private bool EmailIsValid(string email) 
    {
        try
        {
            new MailAddress(email);
            return true;
        } catch
        {
            return false;
        }
    }
}

