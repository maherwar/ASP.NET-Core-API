using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : ControllerBase 
    {

        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;

        private ICityInfoRepository _cityInfoRepository;
        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository)
        {
            _logger = logger;
            _mailService = mailService;
            _cityInfoRepository = cityInfoRepository;
        }

        //it is a resource of resource
        //it is a resource of cities
        //we can get the point of interests for any specific cities

        //[HttpGet("{cityId}/pointsofinterest")]
        //public IActionResult GetPointsOfInterest(int cityId)
        //{
        //    try
        //    {
        //        //throw new Exception("sample exception to work with catch block logging");
        //        var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        //        if (city == null)
        //        {
        //            _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
        //            return NotFound();
        //        }

        //        return Ok(city.PointsOfInterest);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
        //        return StatusCode(500, "A problem happened while handling your request.");
        //    }
        //}


        //--------With Repository Pattern--------//


        [HttpGet("{cityId}/pointsofinterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                    return NotFound();
                }

                var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);
                var pointsOfInterestForCityResults =
                    Mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);

                return Ok(pointsOfInterestForCityResults);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }


        //[HttpGet("{cityId}/pointsofinterest/{id}", Name = "GetPointOfInterest")]
        //public IActionResult GetPointOfInterest(int cityId, int id)
        //{
        //    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    var pointOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);

        //    if (pointOfInterest == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(pointOfInterest);
        //}


        //--------With Repository Pattern--------//


        [HttpGet("{cityId}/pointsofinterest/{id}", Name = "GetPointOfInterest")]
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            var pointOfInterestResult = Mapper.Map<PointOfInterestDto>(pointOfInterest);
            return Ok(pointOfInterestResult);

        }

        //[HttpPost("{cityId}/pointsofinterest")] 
        //public IActionResult CreatePointOfInterest(int cityId,
        //    [FromBody] PointOfInterestForCreationDto pointOfInterest)
        //{
        //    if (pointOfInterest == null)
        //    {
        //        return BadRequest();
        //    }

        //    //Add custom error/validation to model state
        //    if (pointOfInterest.Description == pointOfInterest.Name)
        //    {
        //        ModelState.AddModelError("Description", "The provided description should be different from the name.");
        //    }

        //    //validate parameters of Model Class
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    //if (pointOfInterest.Description == pointOfInterest.Name)
        //    //{
        //    //    ModelState.AddModelError("Description", "The provided description should be different from the name.");
        //    //}

        //    //if (!ModelState.IsValid)
        //    //{
        //    //    return BadRequest(ModelState);
        //    //}

        //    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    // demo purposes - to be improved
        //    //Not worked with multiple consumers.
        //    var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(
        //        c => c.PointsOfInterest).Max(p => p.Id);

        //    var finalPointOfInterest = new PointOfInterestDto()
        //    {
        //        Id = ++maxPointOfInterestId,
        //        Name = pointOfInterest.Name,
        //        Description = pointOfInterest.Description
        //    };

        //    city.PointsOfInterest.Add(finalPointOfInterest);

        //    return CreatedAtRoute("GetPointOfInterest", new
        //        { cityId = cityId, id = finalPointOfInterest.Id }, finalPointOfInterest);
        //}


        //--------With Repository Pattern--------//


        [HttpPost("{cityId}/pointsofinterest")]
        public IActionResult CreatePointOfInterest(int cityId,
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = Mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            var createdPointOfInterestToReturn = Mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest", new
                { cityId = cityId, id = createdPointOfInterestToReturn.Id }, createdPointOfInterestToReturn);
        }


        //[HttpPut("{cityId}/pointsofinterest/{id}")]
        //public IActionResult UpdatePointOfInterest(int cityId, int id,
        //    [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        //{
        //    if (pointOfInterest == null)
        //    {
        //        return BadRequest();
        //    }

        //    if (pointOfInterest.Description == pointOfInterest.Name)
        //    {
        //        ModelState.AddModelError("Description", "The provided description should be different from the name.");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p =>
        //    p.Id == id);

        //    if (pointOfInterestFromStore == null)
        //    {
        //        return NotFound();
        //    }

        //    pointOfInterestFromStore.Name = pointOfInterest.Name;
        //    pointOfInterestFromStore.Description = pointOfInterest.Description;

        //    //Return 200 status code with no content.
        //    return NoContent();
        //}


        //------With Repository Pattern------//

        [HttpPut("{cityId}/pointsofinterest/{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            Mapper.Map(pointOfInterest, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return NoContent();
        }

        //[HttpPatch("{cityId}/pointsofinterest/{id}")]
        //public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
        //    [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        //{
        //    if (patchDoc == null)
        //    {
        //        return BadRequest();
        //    }

        //    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == id);
        //    if (pointOfInterestFromStore == null)
        //    {
        //        return NotFound();
        //    }

        //    var pointOfInterestToPatch =
        //           new PointOfInterestForUpdateDto()
        //           {
        //               Name = pointOfInterestFromStore.Name,
        //               Description = pointOfInterestFromStore.Description
        //           };

        //    patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
        //    {
        //        ModelState.AddModelError("Description", "The provided description should be different from the name.");
        //    }

        //    TryValidateModel(pointOfInterestToPatch);

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
        //    pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

        //    return NoContent();
        //}


        //--------With Repository Pattern--------//

        [HttpPatch("{cityId}/pointsofinterest/{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
            [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = Mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
            {
                ModelState.AddModelError("Description", "The provided description should be different from the name.");
            }

            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return NoContent();
        }

        //[HttpDelete("{cityId}/pointsofinterest/{id}")]
        //public IActionResult DeletePointOfInterest(int cityId, int id)
        //{
        //    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
        //    if (city == null)
        //    {
        //        return NotFound();
        //    }

        //    var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == id);
        //    if (pointOfInterestFromStore == null)
        //    {
        //        return NotFound();
        //    }

        //    city.PointsOfInterest.Remove(pointOfInterestFromStore);

        //    _mailService.Send("Point of interest deleted.",
        //        $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");

        //    return NoContent();
        //}

        //------With repository Pattern-------///

        [HttpDelete("{cityId}/pointsofinterest/{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

            return NoContent();
        }

    }
}
