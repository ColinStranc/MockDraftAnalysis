using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public abstract class CreateModel
    {
        public abstract string ModelType { get; }
        public abstract string ModelName { get; }

        protected IDatabaseAccessor db = new SqlDatabaseAccessor(MockDraft.Web.MvcApplication.GetMockDraftConnectionStringName());

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