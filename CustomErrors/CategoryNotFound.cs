namespace ECommBackend.CustomErrors
{
    public class CategoryNotFound : Exception
    {
        public Guid Categoryid { get; set; }

        public CategoryNotFound()
        {

        }

        public CategoryNotFound(Guid _categoryId,string message) : base(message) 
            {
                Categoryid = _categoryId;
            }

        public CategoryNotFound(string _message, Exception InnerException) : base(_message, InnerException)
        {

        }

    }

        
}
