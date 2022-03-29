namespace ValidatorService.Context
{
    public class ValidatorDatabaseSettings : IValidatorDatabaseSettings
    {
        public string ValidatorCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IValidatorDatabaseSettings
    {
        public string ValidatorCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }  
}
