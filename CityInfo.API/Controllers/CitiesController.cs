﻿using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly CitiesDataStore _citiesDataStore;

        public CitiesController(CitiesDataStore citiesDataStore)
        {
            _citiesDataStore = citiesDataStore;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            return Ok(
                    _citiesDataStore.Cities
                );
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CityDto> GetCity(int id)
        {
            //Find the city
            var cityToreturn = (_citiesDataStore.Cities.FirstOrDefault(c => c.Id == id));

            if (cityToreturn == null ) { return NotFound(); }

            return Ok(cityToreturn);
        }
    }
}
