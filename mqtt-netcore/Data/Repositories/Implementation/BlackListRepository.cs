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
    /// <inheritdoc cref="IBlacklistRepository" />
    /// <summary>
    ///     An implementation supporting the repository pattern to work with <see cref="BlackList" />s.
    /// </summary>
    /// <seealso cref="IBlacklistRepository" />
    public class BlackListRepository : RepositoryBase<BlackList>, IBlackListRepository
    {
        /// <summary>
        ///     The connection settings to use.
        /// </summary>
        private readonly DataContext _context;
        /// <summary>
        ///     Initializes a new instance of the <see cref="BlacklistRepository" /> class.
        /// </summary>
        /// <param name="connectionSettings">The connection settings to use.</param>
        public BlackListRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    

      

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Gets a <see cref="List{T}" /> of all <see cref="BlackList" /> items.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<IEnumerable<BlackList>> GetAllBlacklistItems()
        {
            return await _context.BlackList.ToListAsync();
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Gets a <see cref="BlackList" /> item by its id.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<BlackList> GetBlacklistItemById(int blacklistItemId)
        {
            return await _context.BlackList.FirstOrDefaultAsync(x => x.Id.Equals(blacklistItemId));
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Gets a <see cref="BlackList" /> item by its type.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id to query for.</param>
        /// <param name="blacklistItemType">The <see cref="BlackListType" /> to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<BlackList> GetBlacklistItemByIdAndType(int blacklistItemId, BlackListWhiteListType blacklistItemType)
        {
            return await _context.BlackList.FirstOrDefaultAsync(x => x.Id.Equals(blacklistItemId) && x.Type.Equals(blacklistItemType));
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Gets a <see cref="BlackList" /> item by its type.
        /// </summary>
        /// <param name="blacklistItemType">The <see cref="BlackList" />'s type to query for.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<BlackList> GetBlacklistItemByType(BlackListWhiteListType blacklistItemType)
        {
            return await _context.BlackList.FirstOrDefaultAsync(x => x.Type.Equals(blacklistItemType));
        }
       

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Sets the <see cref="BlackList" />'s state to deleted. (It will still be present in the database, but with
        ///     a deleted timestamp).
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<bool> DeleteBlacklistItem(int blacklistItemId)
        {
            try
            {
                var item = await _context.BlackList.FirstOrDefaultAsync(x => x.Id.Equals(blacklistItemId));
                if (item == null) return false;
                item.DeletedAt = DateTime.Now;
                _context.BlackList.Update(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Deletes a <see cref="BlackList" /> item from the database.
        ///     Returns the number of affected rows.
        /// </summary>
        /// <param name="blacklistItemId">The <see cref="BlackList" />'s id.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation providing the number of affected rows.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<bool> DeleteBlacklistItemFromDatabase(int blacklistItemId)
        {
            try
            {
                var item = await _context.BlackList.FirstOrDefaultAsync(x => x.Id.Equals(blacklistItemId));
                if (item == null) return false;
                _context.BlackList.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Inserts a <see cref="BlackList" /> item to the database.
        /// </summary>
        /// <param name="blacklistItem">The <see cref="BlackList" /> item to insert.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<bool> InsertBlacklistItem(BlackList blacklistItem)
        {
            try
            {
                await _context.BlackList.AddAsync(blacklistItem);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc cref="IBlacklistRepository" />
        /// <summary>
        ///     Updates a <see cref="BlackList" /> item in the database.
        /// </summary>
        /// <param name="blacklistItem">The updated <see cref="BlackList" />.</param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <seealso cref="IBlacklistRepository" />
        public async Task<bool> UpdateBlacklistItem(BlackList blacklistItem)
        {
            try
            {
                var item = await _context.BlackList.FirstOrDefaultAsync(x => x.Id.Equals(blacklistItem.Id));
                if (item == null) return false;
                blacklistItem.UpdatedAt = DateTime.Now;
                _context.BlackList.Update(blacklistItem);
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
