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
    public class AdvertisementController : ControllerBase
    {

        private readonly IRepository<Advertisement> _AdvertisementRepository;

        public AdvertisementController ( IRepository<Advertisement> AdvertisementRepository )
        {
            _AdvertisementRepository = AdvertisementRepository;
        }

        [HttpGet("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ()
        {
            var advertisements = await _AdvertisementRepository.GetAsync(tracked: false);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = advertisements.Select(a => new BannerResponse
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
            var advertisement = await _AdvertisementRepository.GetOneAsync(a => a.Id == Id ,tracked: false);

            if ( advertisement == null )
                return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var response = new BannerResponse
            {
                Id = advertisement.Id,
                Img = $"{baseUrl}/Images/{advertisement.Img}"
            };

            return Ok(new
            {
                Data = response
            });
        }


        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create ( BannerRequest createadvertisementRequest )
        {
            var advertisement = new Advertisement { };

            if ( createadvertisementRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createadvertisementRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);

                using ( var stream = System.IO.File.Create(filePath) ) {
                    await createadvertisementRequest.Img.CopyToAsync(stream);
                }

                advertisement.Img = fileName;

            }

            await _AdvertisementRepository.AddAsync(advertisement);
            await _AdvertisementRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = advertisement.Id }, new
            {
                success_notifaction = "Add advertisement Successfully"
            });

        }


        [HttpPut("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit ( int Id, BannerRequest editadvertisementRequest )
        {
            var advertisementInDb = await _AdvertisementRepository.GetOneAsync(a=> a.Id == Id, tracked:false) ;
            if ( advertisementInDb is null )
                return NotFound();

            if ( editadvertisementRequest.Img is not null ) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(editadvertisementRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", fileName);


                using ( var stream = System.IO.File.Create(filePath) ) {
                    await editadvertisementRequest.Img.CopyToAsync(stream);
                }

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", advertisementInDb.Img);
                if ( System.IO.File.Exists(oldPath) )
                    System.IO.File.Delete(oldPath);


                advertisementInDb.Img = fileName;


            }

            _AdvertisementRepository.Update(advertisementInDb);
            await _AdvertisementRepository.CommitAsync();

            return NoContent();
        }


        [HttpDelete("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete ( int Id )
        {
            var advertisementInDb = await _AdvertisementRepository.GetOneAsync(e => e.Id == Id, tracked:false);
            if ( advertisementInDb is null )
                return NotFound();

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", advertisementInDb.Img);
            if ( System.IO.File.Exists(oldPath) )
                System.IO.File.Delete(oldPath);


            _AdvertisementRepository.Delete(advertisementInDb);
            await _AdvertisementRepository.CommitAsync();

            return NoContent();
        }

    }
}
