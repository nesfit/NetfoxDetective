using System.Collections.Generic;
using System.Windows.Input;

namespace Netfox.Detective.Models.Base.Detective
{
    public class MenuItem
    {
        public List<MenuItem> MenuItems { get; } = new List<MenuItem>();
        public string Header { get; set; }
        public ICommand Command { get; set; }
    }
}