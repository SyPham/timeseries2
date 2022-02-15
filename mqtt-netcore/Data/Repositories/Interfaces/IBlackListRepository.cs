using Data.Enumerations;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Interfaces
{
    /// <summary>
    ///     An interface supporting the repository pattern to work with <see cref="BlackList" />s.
    /// </summary>
    public interface IBlackListRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="BlackList" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<IEnumerable<BlackList>> GetAllBlacklistItems();

        /// <summary>
        ///     Gets a <see cref="BlackList" /> item by its id.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlackList> GetBlacklistItemById(int blacklistItemId);

        /// <summary>
        ///     Gets a <see cref="BlackList" /> item by its type.
        /// </summary>
        /// <param name="blacklistItemType">The <see cref="BlackList" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<BlackList> GetBlacklistItemByType(BlackListWhiteListType blacklistItemType);

        /// <summary>
        ///     Gets a <see cref="BlackList" /> item by its type.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id to query for.</param>
        /// <param name="blacklistItemType">The <see cref="BlackList" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<BlackList> GetBlacklistItemByIdAndType(int blacklistItemId, BlackListWhiteListType blacklistItemType);

        /// <summary>
        ///     Sets the <see cref="BlackList" />'s state to deleted. (It will still be present in the database, but with
        ///     a deleted timestamp).
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        Task<bool> DeleteBlacklistItem(int blacklistItemId);

        /// <summary>
        ///     Deletes a <see cref="BlackList" /> item from the database.
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> DeleteBlacklistItemFromDatabase(int blacklistItemId);

        /// <summary>
        ///     Inserts a <see cref="BlackList" /> item to the database.
        /// </summary>
        /// <param name="blacklistItem">The <see cref="BlackList" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertBlacklistItem(BlackList blacklistItem);

        /// <summary>
        ///     Updates a <see cref="BlackList" /> item in the database.
        /// </summary>
        /// <param name="blacklistItem">The updated <see cref="BlackList" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> UpdateBlacklistItem(BlackList blacklistItem);
    }
}
