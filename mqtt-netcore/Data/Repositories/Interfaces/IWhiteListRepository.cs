using Data.Enumerations;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Interfaces
{
    /// <summary>
    ///     An interface supporting the repository pattern to work with <see cref="WhiteList" />s.
    /// </summary>
    public interface IWhiteListRepository
    {
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="WhiteList" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<IEnumerable<WhiteList>> GetAllWhiteListItems();

        /// <summary>
        ///     Gets a <see cref="WhiteList" /> item by its id.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<WhiteList> GetWhiteListItemById(int WhiteListItemId);

        /// <summary>
        ///     Gets a <see cref="WhiteList" /> item by its type.
        /// </summary>
        /// <param name="WhiteListItemType">The <see cref="WhiteListType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<WhiteList> GetWhiteListItemByType(BlackListWhiteListType WhiteListItemType);

        /// <summary>
        ///     Gets a <see cref="WhiteList" /> item by its type.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id to query for.</param>
        /// <param name="WhiteListItemType">The <see cref="WhiteListType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<WhiteList> GetWhiteListItemByIdAndType(int WhiteListItemId, BlackListWhiteListType WhiteListItemType);

        /// <summary>
        ///     Sets the <see cref="WhiteList" />'s state to deleted. (It will still be present in the database, but with
        ///     a deleted timestamp).
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        Task<bool> DeleteWhiteListItem(int WhiteListItemId);

        /// <summary>
        ///     Deletes a <see cref="WhiteList" /> item from the database.
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> DeleteWhiteListItemFromDatabase(int WhiteListItemId);

        /// <summary>
        ///     Inserts a <see cref="WhiteList" /> item to the database.
        /// </summary>
        /// <param name="WhiteListItem">The <see cref="WhiteList" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        Task<bool> InsertWhiteListItem(WhiteList WhiteListItem);

        /// <summary>
        ///     Updates a <see cref="WhiteList" /> item in the database.
        /// </summary>
        /// <param name="WhiteListItem">The updated <see cref="WhiteList" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        // ReSharper disable once UnusedMember.Global
        Task<bool> UpdateWhiteListItem(WhiteList WhiteListItem);
    }
}
