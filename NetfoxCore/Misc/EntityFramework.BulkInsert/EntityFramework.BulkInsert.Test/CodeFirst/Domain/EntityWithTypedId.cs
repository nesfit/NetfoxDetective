namespace EntityFramework.BulkInsert.Test.CodeFirst.Domain
{
    public abstract class EntityWithTypedId<T>
    {
        public T Id { get; set; }
    }
}