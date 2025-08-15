using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyApp.Controllers.Resources;
using MyApp.Core.Models;
using static MyApp.Controllers.Resources.UserResponse;



namespace MyApp.Controllers
{ 
 [ApiController]
[Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IMapper mapper;
        private readonly string Securitykey = "EverySingleUser"; 
        private readonly SignInManager<User> signinmanager;
        private readonly UserManager<User> usermanager;
        private readonly IConfiguration configuration;
        public UsersController(SignInManager<User> signinmanager, UserManager<User> usermanager, IMapper mapper,IConfiguration configuration)
        {
            this.configuration = configuration;
            this.usermanager = usermanager;
            this.signinmanager = signinmanager;
            this.mapper = mapper;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginResources loginResources){
            var user = await usermanager.FindByNameAsync(loginResources.UserName);
            if(user == null){
                return NotFound("Username not Found");
            }
            var result = await signinmanager.CheckPasswordSignInAsync(user, loginResources.Password,lockoutOnFailure : true);
            if(result.Succeeded){
                var token = generateToken(user);
                return Ok(new{Token = token});
            }
            return BadRequest("Invalid Password");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterResources registerResources)
        {
           
             var user = await usermanager.FindByNameAsync(registerResources.UserName);
             if(user!= null){
                return BadRequest("Username already exists");
            }
             user = new User
            {
                UserName = registerResources.UserName,
                Email = registerResources.Email,
                FirstName = registerResources.FirstName,
                LastName = registerResources.LastName,
                PhoneNumber = registerResources.PhoneNumber,
                Country = registerResources.Country,
                City = registerResources.City
            };

            var result = await usermanager.CreateAsync(user, registerResources.Password);

            if (result.Succeeded)
            {
               var token = generateToken(user);
              
               return Ok(new {Token = token});
            }
            return BadRequest("Failed to Register");

        }
       [HttpPut("update")]
public async Task<IActionResult> Update([FromBody] UserDTO userdto)
{
    var user = await usermanager.FindByIdAsync(userdto.Id.ToString());
    if (user == null)
    {
        return NotFound("User not found");
    }

    // Update user properties
    user.FirstName = userdto.Firstname;
    user.LastName = userdto.LastName;
    user.Email = userdto.Email;
    user.PhoneNumber = userdto.PhoneNumber;
    user.Country = userdto.Country;
    user.City = userdto.City;

    var result = await usermanager.UpdateAsync(user);
    if (result.Succeeded)
    {
        var updatedUser = mapper.Map<User, UserDTO>(user);
        return Ok(updatedUser);
    }

    return BadRequest("Failed to update user");
}


public string generateToken(User user)
{
    // Define claims based on user information
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
        new Claim(ClaimTypes.Country, user.Country),
        new Claim(ClaimTypes.StateOrProvince, user.City)
    };

    // Create the security key from the signing key in the configuration
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwt:SigningKey"]));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Create the JWT token
    var token = new JwtSecurityToken(
        issuer: configuration["jwt:Issuer"],
        audience: configuration["jwt:Audience"],
        expires: DateTime.UtcNow.AddMinutes(30), // Token expiration time
        claims: claims,
        signingCredentials: credentials
    );

    // Return the generated token as a string
    return new JwtSecurityTokenHandler().WriteToken(token);
}

    }
}