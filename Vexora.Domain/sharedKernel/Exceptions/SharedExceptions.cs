using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Domain.sharedKernel.Constants;

namespace Vexora.Domain.sharedKernel.Exceptions;

public static class SharedExceptions
{
    public sealed class InvalidIdException() : Exception
        (Messages.IdInvalid);
}
