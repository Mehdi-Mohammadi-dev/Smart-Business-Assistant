namespace Vexora.Application.Dtos.Customers;

public sealed record CustomerResolutionResultDto
(
    CustomerResolutionStatus Status ,

    CustomerCandidateDto? Customer,

    IReadOnlyCollection<CustomerCandidateDto> Candidates
);
