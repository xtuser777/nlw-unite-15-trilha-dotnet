using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassIn.Application.UseCases.Events.Register
{
    public class RegisterEventUseCase
    {
        public ResponseRegisteredEventJson Execute(RequestEventJson request)
        {
            Validate(request);

            var context = new PassInDbContext();

            var entity = new Infrastructure.Entities.Event()
            {
                Title = request.Title,
                Details = request.Details,
                Slug = request.Title.ToLower().Replace(" ", "-"),
                Maximum_Attendees = request.MaximumAttendees,
            };

            context.Events.Add(entity);
            context.SaveChanges();

            return new ResponseRegisteredEventJson()
            {
                Id = entity.Id,
            };
        }

        public void Validate(RequestEventJson request)
        {
            if (request.MaximumAttendees <= 0)
            {
                throw new ErrorOnValidationException("The maximum attendees is invalid.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ErrorOnValidationException("This title is invalid.");
            }

            if (string.IsNullOrWhiteSpace(request.Details))
            {
                throw new ErrorOnValidationException("This details is invalid.");
            }
        }
    }
}
