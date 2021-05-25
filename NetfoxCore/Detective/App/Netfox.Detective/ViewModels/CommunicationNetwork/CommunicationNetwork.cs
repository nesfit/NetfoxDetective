using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Netfox.Detective.ViewModels.CommunicationNetwork
{
    public class CommunicationNetwork<TKey>
    {
        private NodeModel[] _nodes;

        public NodeModel[] Nodes => this._nodes ?? (this._nodes = this.NodeMap.Values.ToArray());

        public ConcurrentDictionary<TKey, NodeModel> NodeMap { get; } = new ConcurrentDictionary<TKey, NodeModel>();
/*
        public void AddNode(TKey key, NodeModel node)
        {
            NodeMap.TryAdd(key, node);
            Nodes.Add(node);
        }

        public void AddLink(TKey from, TKey to)
        {
            var srcNode = NodeMap[from];
            var trgNode = NodeMap[to];
            var con = new ConnectionModel() { Target = trgNode };
            srcNode.Connections.Add(con);
        }
        */

        public void AutoMap(TKey from, Func<TKey, NodeModel> fromFunc, TKey to, Func<TKey, NodeModel> toFunc)
        {
            var srcNode = this.NodeMap.GetOrAdd(from, fromFunc);
            var trgNode = this.NodeMap.GetOrAdd(to, toFunc);

            var con = new ConnectionModel
            {
                Target = trgNode
            };
            srcNode.Connections.Add(con);
        }
    }
}