using System.Net.Sockets;
using Clients.API.Models;
using Microsoft.AspNetCore.Mvc;
using Clients.API.Repository;
using Serilog;

namespace Clients.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController: ControllerBase
{
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var users = await _repository.GetUsers();
        if(!users.Any()) {
            return NotFound();
        }
        
        return Ok(users);
    }
        

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var Dbuser = await _repository.GetUserById(id);
        if(Dbuser == null) {
            return NotFound("User not found");
        }
        
        
        return Ok(Dbuser);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        if(user.age == 0)
        {
            Log.Warning("Error on creating new user, the attribute age is required");
            return BadRequest("Attribute age is required");
        }
        else if(user.firstName == null || user.firstName.Trim() == "")
        {
            Log.Warning("Error on creating new user, the attribute firstName is required");
            return BadRequest("Attribute firstName is required");
        }

        user.firstName = user.firstName.Trim();
        if(user.surname != null)
            user.surname = user.surname.Trim();
        
        user.creationDate = DateTime.Now;
        _repository.AddUser(user);
        
        // return await _repository.SaveChangesAsync()
        // ? Ok("User created successfully")
        // : BadRequest("Error while trying to add new user");
        if(!await _repository.SaveChangesAsync())
        {
            return BadRequest("Error while trying to add new user");
        }
        
        Log.Information("Created user {@User}", user);
        return Ok("User created successfully");
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, User user)
    {
        if(user.age == 0)
        {
            Log.Warning("Error on updating user with id {id}, the attribute age is required", id);
            return BadRequest("Attribute age is required");
        }
        else if(user.firstName == null || user.firstName.Trim() == "")
        {
            Log.Warning("Error on updating user with id {id}, the attribute firstName is required", id);
            return BadRequest("Attribute firstName is required");
        }

        var Dbuser = await _repository.GetUserById(id);
        if(Dbuser == null)
        {
            Log.Warning("Error on updating user, there is no user with id {id}", id);
            return BadRequest("User not found");
        }

        user.firstName = user.firstName.Trim();
        if(user.surname != null)
            user.surname = user.surname.Trim();
        
        Dbuser.firstName = user.firstName ?? Dbuser.firstName;
        Dbuser.surname = user.surname ?? Dbuser.surname;
        Dbuser.age = user.age == 0 ? Dbuser.age : user.age;

         _repository.UpdateUser(Dbuser);
         Log.Information("Updated user {@User}", Dbuser);

         return await _repository.SaveChangesAsync()
         ? Ok("User updated successfully")
         : BadRequest("Erro while updating user");
        
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
       var Dbuser = await _repository.GetUserById(id);
       if(Dbuser == null)
       {
            Log.Warning("Error on deleting user, there is no user with id {id}", id);
            return BadRequest($"User with id {id} not found");
       }
        
        _repository.DeleteUser(Dbuser);
        Log.Information("Deleted user {@User}", Dbuser);

        return await _repository.SaveChangesAsync()
            ? Ok("User successfully removed")
            : BadRequest("Erro while deleting user");
    }
}
