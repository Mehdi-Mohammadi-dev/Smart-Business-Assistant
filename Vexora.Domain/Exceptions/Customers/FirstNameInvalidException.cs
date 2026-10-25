using Vexora.Domain.Constants.Customers;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Customers;

public sealed class FirstNameInvalidException() : Exception(CustomerConstants.FirstNameInvalid);
