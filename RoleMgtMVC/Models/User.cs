namespace RoleMgtMVC.Models
{
    public enum UserRoles
    {
        External = 0,
        InternalEmployee =1,
        ContractEmployee =2,
        LeadEmployee = 3,
        AdminEmployee = 4
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public UserRoles UserRole { get; set; }
    }
}
