using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;

namespace Rafiq.Area.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Admin")]
    public class ActiveIngredientController : ControllerBase
    {
        private readonly IRepository<ActiveIngredient> _ActiveIngredientsRepository;

        public ActiveIngredientController ( IRepository<ActiveIngredient> ActiveIngredientRepository )
        {
            _ActiveIngredientsRepository = ActiveIngredientRepository;
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ()
        {
            var activeIngredients = await _ActiveIngredientsRepository.GetAsync(tracked: false);

            var response = activeIngredients.Select(a => new ActiveIngredientResponse
            {
                Id = a.Id,
                Name = a.Name,
            }).ToList();    

            return Ok(new
            {
                data = response
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne ( int id )
        {
            var activeIngredient = await _ActiveIngredientsRepository.GetOneAsync(e => e.Id == id);

            if ( activeIngredient is null )
                return NotFound();

            var response = new ActiveIngredientResponse
            {
                Id = activeIngredient.Id,
                Name = activeIngredient.Name,
            };

            return Ok(new
            {
                data = response
            });
        }

        [HttpPost()]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create ( ActiveIngredientRequest createActiveIngredientRequest )
        {

            var activeIngredient = new ActiveIngredient
            {
               Name = createActiveIngredientRequest.Name
            };

         
            await _ActiveIngredientsRepository.AddAsync(activeIngredient);
            await _ActiveIngredientsRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = activeIngredient.Id }, new
            {
                msg = "Activeingredient added successfully"
            });


        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit ( int id,ActiveIngredientRequest editActiveIngredientRequest )
        {

            var activeIngredientInDb = await _ActiveIngredientsRepository.GetOneAsync(e => e.Id == id);
            if ( activeIngredientInDb is null )
                return NotFound();

            activeIngredientInDb.Name = editActiveIngredientRequest.Name;

            _ActiveIngredientsRepository.Update(activeIngredientInDb);
            await _ActiveIngredientsRepository.CommitAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete ( int id )
        {

            var activeIngredientInDb = await _ActiveIngredientsRepository.GetOneAsync(e => e.Id == id);
            if ( activeIngredientInDb is null )
                return NotFound();

            _ActiveIngredientsRepository.Delete(activeIngredientInDb);
            await _ActiveIngredientsRepository.CommitAsync();

            return NoContent();
        }






    }
}
