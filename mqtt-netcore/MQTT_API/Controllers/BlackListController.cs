using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MQTT_API.Controllers
{
    [Route("api/[controller][action]")]
    [ApiController]
    public class BlackListController : ControllerBase
    {
        /// <summary>
        ///     The blacklist repository.
        /// </summary>
        private readonly IBlackListRepository _repoBlackList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlacklistController" /> class.
        /// </summary>
        /// <param name="blacklistRepository">The <see cref="IBlacklistRepository" />.</param>
        public BlackListController(IBlackListRepository repoBlackList)
        {
            this._repoBlackList = repoBlackList;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlackList>>> GetAllBlacklistItems()
        {

            var blacklistItems = await this._repoBlackList.GetAllBlacklistItems();
            return this.Ok(blacklistItems);

        }
        [HttpGet("{blacklistItemId}")]
        public async Task<ActionResult<BlackList>> GetBlacklistItemById(int blacklistItemId)
        {
            var blacklistItem = await this._repoBlackList.GetBlacklistItemById(blacklistItemId);

            if (blacklistItem != null)
            {
                return this.Ok(blacklistItem);
            }

            return this.NotFound(blacklistItemId);

        }
        [HttpPost]
        public async Task<ActionResult> CreateBlacklistItem([FromBody] BlackList createBlacklistItem)
        {

            var foundBlackListItem = this._repoBlackList.GetBlacklistItemByIdAndType(createBlacklistItem.Id, createBlacklistItem.Type);

            if (foundBlackListItem != null)
            {
                return this.Conflict(createBlacklistItem);
            }

            var inserted = await this._repoBlackList.InsertBlacklistItem(createBlacklistItem);

            if (inserted)
            {
                return this.Ok(createBlacklistItem);
            }

            return this.BadRequest(createBlacklistItem);

        }
        [HttpDelete("{blacklistItemId}")]
        public async Task<ActionResult> DeleteBlacklistItemById(int blacklistItemId)
        {

            var deleted = await this._repoBlackList.DeleteBlacklistItem(blacklistItemId);

            if (deleted)
            {
                return this.Ok(blacklistItemId);
            }

            return this.BadRequest(blacklistItemId);

        }
    }
}
