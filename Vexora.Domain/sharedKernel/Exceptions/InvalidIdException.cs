using Vexora.Domain.sharedKernel.Constants;

namespace Vexora.Domain.sharedKernel.Exceptions;

public sealed class InvalidIdException() : Exception
(Messages.IdInvalid);
