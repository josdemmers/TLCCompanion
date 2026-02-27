using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using TLCCompanion.Entities;

namespace TLCCompanion.Messages
{
    public class PlayerCoordinatesUpdatedMessage(PlayerCoordinatesUpdatedMessageParams playerCoordinatesUpdatedMessageParams) : ValueChangedMessage<PlayerCoordinatesUpdatedMessageParams>(playerCoordinatesUpdatedMessageParams)
    {
    }

    public class PlayerCoordinatesUpdatedMessageParams
    {
        public PlayerCoordinates PlayerCoordinates { get; set; } = new PlayerCoordinates();
    }
}
