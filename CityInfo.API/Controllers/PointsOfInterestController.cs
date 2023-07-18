using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService,
            ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointsOfInterestDto>>>  GetPointsOfInterest( int cityId)
        {
            try
            {
                //throw new Exception("Exception sample.");
                var cityExist = await _cityInfoRepository.CityExistAsync(cityId);

                if (!cityExist)
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                    return NotFound();
                }

                var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

                return Ok(_mapper.Map<IEnumerable<PointsOfInterestDto>>(pointsOfInterest));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.",ex);

                return StatusCode(500, "A problem happened while hadling your request");
            }
        }

        [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointsOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            var cityExist = await _cityInfoRepository.CityExistAsync(cityId);

            if (!cityExist)
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if (pointOfInterest == null)
            {
                _logger.LogInformation($"PointOfInterest with id {pointOfInterestId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            return Ok(_mapper.Map<PointsOfInterestDto>(pointOfInterest));
        }

        [HttpPost]
        public async Task<ActionResult<PointsOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            var city = await _cityInfoRepository.CityExistAsync(cityId);

            if (!city)
            {
                return NotFound();
            }
            //only for demo purposes
            
            var pointOfInterestEntity = _mapper.Map<PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();

            var finalPointOfInterest = _mapper.Map<PointsOfInterestDto>(pointOfInterestEntity);

            return CreatedAtRoute("GetPointOfInterest",
            new
            {
                cityId = cityId,
                pointOfInterestId = finalPointOfInterest.Id
            }, finalPointOfInterest);
        }
        [HttpPut("{pointOfInterestId}")]
        public async  Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDto pointOfInterest)
        {
            var city = await _cityInfoRepository.CityExistAsync(cityId);

            if (!city)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            await  _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{pointOfInterestId}")]
        public async  Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var city = await _cityInfoRepository.CityExistAsync(cityId);

            if (!city)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            var city = await _cityInfoRepository.CityExistAsync(cityId);

            if (!city)
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            _mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted");

            return NoContent();
        }

    }
}
