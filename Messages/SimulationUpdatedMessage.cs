using CommunityToolkit.Mvvm.Messaging.Messages;

public class SimulationUpdatedMessage : ValueChangedMessage<bool>
{
    public SimulationUpdatedMessage() : base(true) { }
}