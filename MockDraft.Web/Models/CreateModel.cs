using Database;

namespace MockDraft.Web.Models
{
    public abstract class CreateModel
    {
        public abstract string ModelType { get; }
        public abstract string ModelName { get; }

        protected IDatabaseAccessor Db = new SqlDatabaseAccessor(MvcApplication.GetMockDraftConnectionStringName());

        public string SuccessMessage 
        { 
            get 
            { 
                return ModelType + " " + ModelName + " successfully created."; 
            } 
        }

        public string AlreadyExistedErrorMessage
        {
            get
            {
                return ModelType + " " + ModelName + " already exists.";
            }
        }
    }
}