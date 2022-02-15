using Data.Enumerations;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Implementation
{
    /// <inheritdoc cref="IWhiteListRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="WhiteList" />s.
    /// </summary>
    /// <seealso cref="IWhiteListRepository" />
    public class WhiteListRepository : RepositoryBase<WhiteList>, IWhiteListRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly DataContext _context;
        /// <summary>
        ///     Initializes a new instance of the <see cref="WhiteListRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public WhiteListRepository(DataContext context) : base(context)
        {
            _context = context;
        }


        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="WhiteList" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<IEnumerable<WhiteList>> GetAllWhiteListItems()
        {
            return await _context.WhiteList.ToListAsync();
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Gets a <see cref="WhiteList" /> item by its id.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<WhiteList> GetWhiteListItemById(int WhiteListItemId)
        {
            return await _context.WhiteList.FirstOrDefaultAsync(x => x.Id.Equals(WhiteListItemId));
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Gets a <see cref="WhiteList" /> item by its type.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id to query for.</param>
        /// <param name="WhiteListItemType">The <see cref="WhiteListType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<WhiteList> GetWhiteListItemByIdAndType(int WhiteListItemId, BlackListWhiteListType WhiteListItemType)
        {
            return await _context.WhiteList.FirstOrDefaultAsync(x => x.Id.Equals(WhiteListItemId) && x.Type.Equals(WhiteListItemType));
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Gets a <see cref="WhiteList" /> item by its type.
        /// </summary>
        /// <param name="WhiteListItemType">The <see cref="WhiteList" />'s type to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<WhiteList> GetWhiteListItemByType(BlackListWhiteListType WhiteListItemType)
        {
            return await _context.WhiteList.FirstOrDefaultAsync(x => x.Type.Equals(WhiteListItemType));
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Sets the <see cref="WhiteList" />'s state to deleted. (It will still be present in the database, but with
        ///     a deleted timestamp).
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<bool> DeleteWhiteListItem(int WhiteListItemId)
        {
            try
            {
                var item = await _context.WhiteList.FirstOrDefaultAsync(x => x.Id.Equals(WhiteListItemId));
                if (item == null) return false;
                item.DeletedAt = DateTime.Now;
                _context.WhiteList.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Deletes a <see cref="WhiteList" /> item from the database.
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="WhiteListItemId">The <see cref="WhiteList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<bool> DeleteWhiteListItemFromDatabase(int WhiteListItemId)
        {
            try
            {
                var item = await _context.WhiteList.FirstOrDefaultAsync(x => x.Id.Equals(WhiteListItemId));
                if (item == null) return false;
                _context.WhiteList.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Inserts a <see cref="WhiteList" /> item to the database.
        /// </summary>
        /// <param name="WhiteListItem">The <see cref="WhiteList" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<bool> InsertWhiteListItem(WhiteList WhiteListItem)
        {
            try
            {
                await _context.WhiteList.AddAsync(WhiteListItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="IWhiteListRepository" />
        /// <summary>
        ///     Updates a <see cref="WhiteList" /> item in the database.
        /// </summary>
        /// <param name="WhiteListItem">The updated <see cref="WhiteList" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IWhiteListRepository" />
        public async Task<bool> UpdateWhiteListItem(WhiteList WhiteListItem)
        {
            try
            {
                var item = await _context.WhiteList.FirstOrDefaultAsync(x => x.Id.Equals(WhiteListItem.Id));
                if (item == null) return false;
                WhiteListItem.UpdatedAt = DateTime.Now;
                _context.WhiteList.Update(WhiteListItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
     
    }
}
