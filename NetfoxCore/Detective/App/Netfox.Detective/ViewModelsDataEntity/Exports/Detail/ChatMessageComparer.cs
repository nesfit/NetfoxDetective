using System;
using System.Collections.Generic;
using Netfox.Core.Interfaces.Model.Exports;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    public class ChatMessageComparer : IEqualityComparer<IChatMessage>
    {
        #region Implementation of IEqualityComparer<in IChatMessage>

        public bool Equals(IChatMessage x, IChatMessage y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (x.Sender == null || x.Receiver == null || y.Sender == null || y.Receiver == null)
            {
                return false;
            }

            if ((x.Sender == y.Receiver && x.Receiver == y.Sender) ||
                (x.Sender == y.Sender && x.Receiver == y.Receiver))
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(IChatMessage obj)
        {
            var h1 = 0;
            var h2 = 0;
            try
            {
                h1 = obj?.Receiver.GetHashCode() ?? 0;
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                h2 = obj?.Sender.GetHashCode() ?? 0;
            }
            catch (Exception)
            {
                // ignored
            }

            return h1 ^ h2;
        }

        #endregion
    }
}