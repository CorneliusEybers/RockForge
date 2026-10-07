using RockForge.Application.Exceptions;
using RockForge.Domain;
using RockForge.Domain.Enums;

namespace RockForge.Application.Validation
{
    public static class RockValidator
    {
        public static void ValidateForCreate(Rock rock)
        {
            if (string.IsNullOrWhiteSpace(rock.MemberId))
            {
                throw new RockValidationException("MemberId must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(rock.Title))
            {
                throw new RockValidationException("Title must not be empty or whitespace.");
            }

            if (rock.DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new RockValidationException("DueDate must not be in the past.");
            }

            if (!Enum.IsDefined(typeof(RockCategory), rock.Category))
            {
                throw new RockValidationException("Category must be one of the defined Rock categories.");
            }
        }
    }
}