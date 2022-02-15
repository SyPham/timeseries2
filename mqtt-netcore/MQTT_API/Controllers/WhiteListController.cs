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
    public class WhiteListController : ControllerBase
    {
        /// <summary>
        ///     The WhiteList repository.
        /// </summary>
        private readonly IWhiteListRepository _repoWhiteList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WhiteListController" /> class.
        /// </summary>
        /// <param name="WhiteListRepository">The <see cref="IWhiteListRepository" />.</param>
        public WhiteListController(IWhiteListRepository repoWhiteList)
        {
            this._repoWhiteList = repoWhiteList;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WhiteList>>> GetAllWhiteListItems()
        {

            var WhiteListItems = await this._repoWhiteList.GetAllWhiteListItems();
            return this.Ok(WhiteListItems);

        }
        [HttpGet("{WhiteListItemId}")]
        public async Task<ActionResult<WhiteList>> GetWhiteListItemById(int WhiteListItemId)
        {
            var WhiteListItem = await this._repoWhiteList.GetWhiteListItemById(WhiteListItemId);

            if (WhiteListItem != null)
            {
                return this.Ok(WhiteListItem);
            }

            return this.NotFound(WhiteListItemId);

        }
        [HttpPost]
        public async Task<ActionResult> CreateWhiteListItem([FromBody] WhiteList createWhiteListItem)
        {

            var foundWhiteListItem = this._repoWhiteList.GetWhiteListItemByIdAndType(createWhiteListItem.Id, createWhiteListItem.Type);

            if (foundWhiteListItem != null)
            {
                return this.Conflict(createWhiteListItem);
            }

            var inserted = await this._repoWhiteList.InsertWhiteListItem(createWhiteListItem);

            if (inserted)
            {
                return this.Ok(createWhiteListItem);
            }

            return this.BadRequest(createWhiteListItem);

        }
        [HttpDelete("{WhiteListItemId}")]
        public async Task<ActionResult> DeleteWhiteListItemById(int WhiteListItemId)
        {

            var deleted = await this._repoWhiteList.DeleteWhiteListItem(WhiteListItemId);

            if (deleted)
            {
                return this.Ok(WhiteListItemId);
            }

            return this.BadRequest(WhiteListItemId);

        }
    }
}
