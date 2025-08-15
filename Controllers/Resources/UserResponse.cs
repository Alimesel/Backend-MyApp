namespace MyApp.Controllers.Resources{
    public class UserResponse{
        public string Token { get; set; }
        public  UserDTO userDTO { get;set;}
    public class UserDTO{
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    }
}
}