using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using AdvertApi.Services;
//using AutoMapper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;

namespace AdvertApi.Controllers
{
    [ApiController]
    [Route("api/v1/adverts")]
    [Produces("application/json")]
    public class AdvertController : ControllerBase
    {
        private readonly ILogger<AdvertController> _logger;
        private readonly IAdvertStorageService _advertStorageService;
        public IConfiguration Configuration { get; }

        public AdvertController(
            ILogger<AdvertController> logger,
            IAdvertStorageService advertStorageService, 
            IConfiguration configuration)
        {
            _logger = logger;
            _advertStorageService = advertStorageService;
            Configuration = configuration;
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateAdvertResponse))]
        public async Task<IActionResult> Create(AdvertModel model)
        {
            string recordId;
            try
            {
                recordId = await _advertStorageService.AddAsync(model);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return StatusCode(StatusCodes.Status201Created, new CreateAdvertResponse() { Id = recordId });
        }


        [HttpPut]
        [Route("Confirm")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Confirm(ConfirmAdvertModel model)
        {
            try
            {
                await _advertStorageService.ConfirmAsync(model);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return new OkResult();
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var advert = await _advertStorageService.GetByIdAsync(id);
                return new JsonResult(advert);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("test")]
        public async Task<string> GetTest()
        {
            return await Task.FromResult("Hello World");
        }

        [HttpGet]
        [Route("all")]
        [ProducesResponseType(200)]
        [EnableCors("AllOrigin")]
        public async Task<IActionResult> All()
        {
            return new JsonResult(await _advertStorageService.GetAllAsync());
        }
    }
}
