using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TLCCompanion.Messages
{
    public class ApplicationLoadedMessage(ApplicationLoadedMessageParams applicationLoadedMessageParams) : ValueChangedMessage<ApplicationLoadedMessageParams>(applicationLoadedMessageParams)
    {
    }

    public class ApplicationLoadedMessageParams
    {
    }
}