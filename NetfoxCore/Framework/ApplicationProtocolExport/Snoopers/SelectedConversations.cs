using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Framework.ApplicationProtocolExport.Snoopers
{
    /// <summary> A selected conversations.</summary>
    public class SelectedConversations
    {
        /// <summary> The selected conversations to sleuth index.</summary>
        private readonly ConcurrentDictionary<Type, Int64> _selectedConvesationsToSleuthIndex =
            new ConcurrentDictionary<Type, Int64>();

        /// <summary> The single access lock.</summary>
        private readonly Object _singleAccessLock = new Object();

        /// <summary> true to extraction begun.</summary>
        private Boolean _extractionBegun;

        /// <summary> The selected conversations.</summary>
        private ILxConversation[] _selectedConversations;

        /// <summary> Default constructor.</summary>
        public SelectedConversations()
        {
            this._selectedConversations = new L7Conversation[]
                { };
        }

        /// <summary> Constructor.</summary>
        /// <param name="selectedConversations"> The selected conversations. </param>
        public SelectedConversations(IEnumerable<ILxConversation> selectedConversations)
        {
            this._selectedConversations = selectedConversations.OrderBy(c => c.FirstSeen).ToArray();
        }

        /// <summary> Gets the number of. </summary>
        /// <value> The count.</value>
        public Int32 Count => this._selectedConversations.Count();

        /// <summary> Adds the conversations.</summary>
        /// <exception cref="ApplicationException"> Thrown when an Application error condition occurs. </exception>
        /// <param name="selectedConversations"> The selected conversations. </param>
        public void AddConversations(IEnumerable<L7Conversation> selectedConversations)
        {
            lock (this._singleAccessLock)
            {
                if (this._extractionBegun)
                {
                    throw new ApplicationException(
                        "Conversations could not be added, _selectedBidirectionalFlows are locked, most probably extraction already begun.");
                }

                this._selectedConversations = this._selectedConversations.Concat(selectedConversations).ToArray();
            }
        }

        /// <summary> Locks the selected conversations.</summary>
        public void LockSelectedConversations()
        {
            lock (this._singleAccessLock)
            {
                this._extractionBegun = true;
            }
        }

        public ILxConversation PeekCurrentConversation(Type sleuthType)
        {
            var currentIndex = this._selectedConvesationsToSleuthIndex[sleuthType];
            return this._selectedConversations.Count() <= currentIndex
                ? this._selectedConversations[currentIndex]
                : this._selectedConversations.Last();
        }

        /// <summary>
        ///     Atomically increment inner indexer and returns next conversation in line
        /// </summary>
        /// <param name="bidirectionalFlowt BidirectionalFlow or null
        /// 
        /// </param>
        /// <returns>true if BidirectionalFlow is valid, false otherwise</returns>
        public Boolean TryGetNextConversations(Type sleuthType, out ILxConversation conversation,
            out Int64 conversationIndex)
        {
            //atomic incrementation of SelectedConvesationsIndex
            conversationIndex =
                this._selectedConvesationsToSleuthIndex.AddOrUpdate(sleuthType, 0, (key, val) => val + 1);
            if (this._selectedConversations.Count() <= conversationIndex)
            {
                conversation = null;
                return false;
            }

            conversation = this._selectedConversations[conversationIndex];
            return true;
        }
    }
}