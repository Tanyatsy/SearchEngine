namespace IndexService.Context
{
    public class IndexDatabaseSettings : IIndexDatabaseSettings
    {
        public string IndexCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IIndexDatabaseSettings
    {
        public string IndexCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }  
}
