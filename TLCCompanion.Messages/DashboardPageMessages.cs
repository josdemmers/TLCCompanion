using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace TLCCompanion.Messages
{
    public class PlayerCoordinatesUpdatedMessage(PlayerCoordinatesUpdatedMessageParams playerCoordinatesUpdatedMessageParams) : ValueChangedMessage<PlayerCoordinatesUpdatedMessageParams>(playerCoordinatesUpdatedMessageParams)
    {
    }

    public class PlayerCoordinatesUpdatedMessageParams
    {
        public float PositionX { get; set; } = 0;
        public float PositionY { get; set; } = 0;
        public float PositionZ { get; set; } = 0;
    }
}
