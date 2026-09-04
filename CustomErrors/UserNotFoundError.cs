namespace ECommBackend.CustomErrors
{
    [Serializable]
    public class UserNotFoundError : Exception
    {
        public Guid UserId { get; set; }
        public UserNotFoundError()
        {

        }

        public UserNotFoundError(Guid _userId,string message) : base(message) 
            {
            UserId = _userId;
            }

        public UserNotFoundError(string message,Guid _userId ,Exception InnerException) : base(message, InnerException) {
            {
                UserId = _userId ;
            }
        }
    }
}
