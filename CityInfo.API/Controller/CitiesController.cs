using System;
//using AutoMapper;
using CityInfo.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controller
{
    [Route("api/cities")]
    public class CitiesController: ControllerBase
    {

        private ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }


        [HttpGet()]
        public IActionResult GetCities()
        {
            //return new JsonResult(new List<Object>()
            //{
            //    //Anaonymous data as we dont have city model class
            //    //to get the list of cities.
            //    new {id = 1, Name = "NewYork City"},
            //    new {id = 2, name = "Antwerp"},
            //}); 

            //return Ok(CitiesDataStore.Current.Cities);


            //-------With Repository Pattern--------//


            var cityEntities = _cityInfoRepository.GetCities();
            var results = Mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities);

            return Ok(results);
        }

        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            // find city

            //return new JsonResult(CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id));

            //var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
            //if (cityToReturn == null)
            //{
            //    return NotFound();
            //}

            //return Ok(cityToReturn);



            //-------With Repository Pattern--------//


            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);

            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                var cityResult = Mapper.Map<CityDto>(city);
                return Ok(cityResult);
            }

            var cityWithoutPointsOfInterestResult = Mapper.Map<CityWithoutPointsOfInterestDto>(city);
            return Ok(cityWithoutPointsOfInterestResult);
        }
    }
}
