using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MockDraft.Web.Models
{
    public class CreateModel
    {
        public string FeedbackMessage { get { return _feedbackMessage; } }

        private string _feedbackMessage = "";
    }
}