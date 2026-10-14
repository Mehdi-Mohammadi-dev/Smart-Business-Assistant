namespace Vexora.Application.Caching.Conversations;

public enum ConversationStatus
{
    None = 0,

    WaitingForCustomerSelection = 1,

    WaitingForCustomerCreationConfirmation = 2,

    WaitingForMissingInformation = 3,

    WaitingForCustomerName = 4,

   WaitingForCheckDate= 5
}