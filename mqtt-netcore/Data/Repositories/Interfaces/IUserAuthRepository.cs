using Data.Enumerations;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace Data.Repositories.Interfaces
{
    /// <summary>
    ///     An interface supporting the repository pattern to work with <see cref="User" />s.
    /// </summary>
    public interface IUserAuthRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<IEnumerable<User>> GetUsers();

        /// <summary>
        ///     Gets a <see cref="User" /> by their id.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s id to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<User> GetUserById(int userId);

        /// <summary>
        ///     Gets a <see cref="User" /> by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<User> GetUserByName(string userName);

        /// <summary>
        ///     Gets a <see cref="User" />'s name and identifier by their user name.
        /// </summary>
        /// <param name="userName">The <see cref="User" />'s name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<(string, int)> GetUserNameAndUserIdByName(string userName);

        /// <summary>
        ///     Gets a <see cref="bool" /> value indicating whether the user name already exists or not.
        /// </summary>
        /// <param name="userName">The user name to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> UserNameExists(string userName);

        /// <summary>
        ///     Inserts a <see cref="User" /> to the database.
        /// </summary>
        /// <param name="user">The <see cref="User" /> to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertUser(User user);

        /// <summary>
        ///     Sets the <see cref="User" />'s state to deleted. (It will still be present in the database, but with a deleted
        ///     timestamp).
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        Task<bool> DeleteUser(int userId);

        /// <summary>
        ///     Deletes a <see cref="User" /> from the database.
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="userId">The <see cref="User" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> DeleteUserFromDatabase(int userId);

        /// <summary>
        ///     Updates a <see cref="User" /> in the database.
        /// </summary>
        /// <param name="user">The updated <see cref="User" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> UpdateUser(User user);

        /// <summary>
        ///     Resets the password for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="hashedPassword">The hashed password to set.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> ResetPassword(int userId, string hashedPassword);

        /// <summary>
        ///     Gets the blacklist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<IEnumerable<BlackList>> GetBlacklistItemsForUser(int userId, BlackListWhiteListType type);

        /// <summary>
        ///     Gets the whitelist items for a <see cref="User" />.
        /// </summary>
        /// <param name="userId">The user identifier to query for.</param>
        /// <param name="type">The <see cref="BlacklistWhitelistType" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<IEnumerable<WhiteList>> GetWhitelistItemsForUser(int userId, BlackListWhiteListType type);

        /// <summary>
        ///     Gets the client id prefixes for all <see cref="User" />s.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<IEnumerable<string>> GetAllClientIdPrefixes();
    }
}
