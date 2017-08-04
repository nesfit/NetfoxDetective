using System;

namespace Spid
{
    public class AttributeMeterSetting
    {
        public AttributeMeterSetting(String name) { this.Name = name; }
        public String Name { get; }
        public Boolean Active { get; set; }
    }
}