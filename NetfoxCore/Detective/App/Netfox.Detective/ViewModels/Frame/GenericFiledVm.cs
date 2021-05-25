namespace Netfox.Detective.ViewModels.Frame
{
    public class GenericFiledVm
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public object Content { get; set; }
        public string Id { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }
    }
}