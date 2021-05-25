namespace EntityFramework.MappingAPI.Test.CodeFirst.Domain
{
    public abstract class EntityWithTypedId<T>
    {
        public T Id { get; set; }
    }
}