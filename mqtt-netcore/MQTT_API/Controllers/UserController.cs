using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Dto;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MQTT_API.Controllers
{
    [Route("api/[controller][action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repoUser;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configMapper;
        private readonly IPasswordHasher<User> passwordHasher;

        public UserController(IUserRepository repoUser, MapperConfiguration configMapper, IMapper mapper)
        {
            _repoUser = repoUser;
            _configMapper = configMapper;
            _mapper = mapper;
            this.passwordHasher = new PasswordHasher<User>();
        }
        [HttpGet]
        public async Task<ActionResult> GetUsers()
        {
            var users = await this._repoUser.GetUsers();

            if (users?.Count() == 0)
            {
                return Ok(users);
            }
            var returnUsers = this._mapper.Map<IEnumerable<DtoReadUser>>(users);
            return Ok(returnUsers);

        }
        [HttpGet("{userId}")]
        public async Task<ActionResult<DtoReadUser>> GetUserById(int userId)
        {

            var user = await this._repoUser.GetUserById(userId);
            if (user == null)
            {
                return NotFound(userId);
            }
            var returnUser = this._mapper.Map<DtoReadUser>(user);
            return Ok(returnUser);

        }
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] DtoCreateUpdateUser createUser)
        {

            var user = this._mapper.Map<User>(createUser);
            var userExists = await this._repoUser.UserNameExists(createUser.Username);

            if (userExists)
            {
                return Conflict(createUser);
            }

            var inserted = await this._repoUser.InsertUser(user);

            if (!inserted)
            {
                return BadRequest(createUser);
            }

            var returnUser = this._mapper.Map<DtoReadUser>(user);
            return Ok(returnUser);

        }
        [HttpPut("{userId}")]
        public async Task<ActionResult> UpdateUser(int userId, [FromBody] DtoCreateUpdateUser updateUser)
        {
            var resultUser = await this._repoUser.GetUserById(userId);

            if (resultUser == null)
            {
                return NotFound(userId);
            }

            resultUser = this._mapper.Map<User>(updateUser);
            resultUser.Id = userId;
            resultUser.PasswordHash = this.passwordHasher.HashPassword(resultUser, updateUser.Password);

            var updated = await this._repoUser.UpdateUser(resultUser);

            if (!updated)
            {
                return BadRequest(userId);
            }

            var returnUser = this._mapper.Map<DtoReadUser>(resultUser);
            return Ok(returnUser);

        }
        [HttpDelete]
        public async Task<ActionResult> DeleteUserById(int userId)
        {
            var deleted = await this._repoUser.DeleteUser(userId);

            if (deleted)
            {
                return Ok(userId);
            }

            return BadRequest(userId);

        }
    }
}
