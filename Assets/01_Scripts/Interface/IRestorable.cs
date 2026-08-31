public interface IRestorable
{
    public bool TryGetById<T>(string id, out T result);
}