namespace ECommBackend.Models.ModInterfaces
{
    public interface IUser
    {

        public Guid UserId { get; set; }
       
        public string FirstName { get; set; }
       
        public string LastName { get; set; }
      
        public string Email { get; set; }
       
        public int Age { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
