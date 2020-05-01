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
using Amazon.SimpleNotificationService;
using AdvertApi.Models.Messages;
using Amazon.SimpleNotificationService.Model;

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
                await RaiseAdvertConfirmedMessage(model);
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

        private async Task RaiseAdvertConfirmedMessage(ConfirmAdvertModel model)
        {
            var topicArn = Configuration.GetValue<string>("TopicArn");
            var dbModel = await _advertStorageService.GetByIdAsync(model.Id);

            using (var client = new AmazonSimpleNotificationServiceClient())
            {
                var message = new AdvertConfirmedMessage
                {
                    Id = model.Id,
                    Title = dbModel.Title
                };

                var messageJson = JsonConvert.SerializeObject(message);
                await client.PublishAsync(topicArn, messageJson);
            }
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
        [Route("topics")]
        public async Task<IActionResult> ListTopics()
        {
            try
            {
                using (var client = new AmazonSimpleNotificationServiceClient())
                {
                    var topics = await client.ListTopicsAsync();
                    var list = topics.Topics;
                    return new JsonResult(list);
                }
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
        [Route("raise/{id}/{title}")]
        public async Task<IActionResult> Raise(string id, string title)
        {
            try
            {
                var topicArn = Configuration.GetValue<string>("TopicArn");

                using (var client = new AmazonSimpleNotificationServiceClient())
                {
                    var message = new AdvertConfirmedMessage
                    {
                        Id = id,
                        Title = title
                    };

                    var messageJson = JsonConvert.SerializeObject(message);
                    await client.PublishAsync(topicArn, messageJson);
                }
            }
            catch (AuthorizationErrorException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (EndpointDisabledException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (InternalErrorException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (InvalidParameterException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (InvalidParameterValueException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (InvalidSecurityException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (KMSAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (KMSDisabledException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (KMSInvalidStateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (KMSNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (KMSOptInRequiredException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (KMSThrottlingException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (PlatformApplicationDisabledException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return new OkResult();
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
