using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafiq.DTOs.Request;
using Rafiq.DTOs.Response;
using Rafiq.Models;
using Rafiq.Repositories.IRepositories;
using Rafiq.Utitlies;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Rafiq.Area.Admin
{
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Area("Admin")]
    public class DidYouKnowController : ControllerBase
    {

        private readonly IRepository<DidYouKnow> _DidYouKnowRepository;

        public DidYouKnowController ( IRepository<DidYouKnow> DidYouKnowRepository )
        {
            _DidYouKnowRepository = DidYouKnowRepository;
        }

        [HttpGet("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ()
        {
            var DidYouKnow = await _DidYouKnowRepository.GetAsync(tracked: false);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = DidYouKnow.Select(a => new BannerResponse
            {
                Id = a.Id,
                Img = $"{baseUrl}/Images/{a.Img}"
            }).ToList();

            return Ok(new
            {
                Data = response
            });
        }


        [HttpGet("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne ( int Id)
        {
            var DidYouKnow = await _DidYouKnowRepository.GetOneAsync(a => a.Id == Id ,tracked: false);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = new BannerResponse
            {
                Id = DidYouKnow.Id,
                Img = $"{baseUrl}/Images/{DidYouKnow.Img}"
            };

            return Ok(new
            {
                Data = response
            });
        }


        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create ( BannerRequest createDidYouKnowRequest )
        {
            var DidYouKnow = new DidYouKnow { };

            if ( createDidYouKnowRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createDidYouKnowRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);

                using ( var stream = System.IO.File.Create(filePath) ) {
                    await createDidYouKnowRequest.Img.CopyToAsync(stream);
                }

                DidYouKnow.Img = fileName;

            }

            await _DidYouKnowRepository.AddAsync(DidYouKnow);
            await _DidYouKnowRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = DidYouKnow.Id }, new
            {
                success_notifaction = "Add DidYouKnow Successfully"
            });

        }


        [HttpPut("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit ( int Id, BannerRequest editDidYouKnowRequest )
        {
            var DidYouKnowInDb = await _DidYouKnowRepository.GetOneAsync(a=> a.Id == Id, tracked:false) ;
            if ( DidYouKnowInDb is null )
                return NotFound();

            if ( editDidYouKnowRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(editDidYouKnowRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);


                using ( var stream = System.IO.File.Create(filePath) ) {
                    await editDidYouKnowRequest.Img.CopyToAsync(stream);
                }

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", DidYouKnowInDb.Img);
                if ( System.IO.File.Exists(oldPath) )
                    System.IO.File.Delete(oldPath);


                DidYouKnowInDb.Img = fileName;


            }

            _DidYouKnowRepository.Update(DidYouKnowInDb);
            await _DidYouKnowRepository.CommitAsync();

            return NoContent();
        }


        [HttpDelete("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete ( int Id )
        {
            var advertisementInDb = await _DidYouKnowRepository.GetOneAsync(e => e.Id == Id, tracked:false);
            if ( advertisementInDb is null )
                return NotFound();

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", advertisementInDb.Img);
            if ( System.IO.File.Exists(oldPath) )
                System.IO.File.Delete(oldPath);


            _DidYouKnowRepository.Delete(advertisementInDb);
            await _DidYouKnowRepository.CommitAsync();

            return NoContent();
        }

    }
}
