using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeSeries2.Data;
using TimeSeries2.Models;

namespace TimeSeries2.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TimeSeriesController : ControllerBase
    {
      

        private readonly ILogger<TimeSeriesController> _logger;
        private readonly IMongoRepository<TimeSeries> _repo;

        public TimeSeriesController(ILogger<TimeSeriesController> logger, IMongoRepository<TimeSeries> repo)
        {
            _logger = logger;
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            return Ok( await Task.FromResult(  _repo.AsQueryable().ToList()));
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(TimeSeries model)
        {

            try
            {
               await _repo.InsertOneAsync(model);
                _logger.LogInformation("Insert successfully");

                return NoContent();

            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to insert: " + ex.Message);
                return BadRequest("Failed to insert: " + ex.Message);
            }
        }
    }
}
