namespace Web.Services
{
    public interface IStorage
    {
        string Add(string url);
        string Get(string name);
        void Clear();
    }
}
