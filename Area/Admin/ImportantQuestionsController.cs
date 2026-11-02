using Microsoft.AspNetCore.Authorization;
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
    public class ImportantQuestionsController : ControllerBase
    {

        private readonly IRepository<ImportantQuestions> _importantQuestionsRepository;

        public ImportantQuestionsController ( IRepository<ImportantQuestions> ImportantQuestionsRepository )
        {
            _importantQuestionsRepository = ImportantQuestionsRepository;
        }

        [HttpGet("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetAll ( int page = 1, int pageSize = 7 )
        {
            var importantQuestions = await _importantQuestionsRepository.GetAsync(tracked: false);

            var totalCount = importantQuestions.Count();

            var pagedData = importantQuestions
                .Skip(( page - 1 ) * pageSize)
                .Take(pageSize)
                .Select(a => new ImportantQuestionsResponse
                {
                    Id = a.Id,
                    Question = a.Question,
                    Answer = a.Answer,
                })
                .ToList();

            return Ok(new
            {
                Data = pagedData,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }


        [HttpGet("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne ( int Id)
        {
            var importantQuestion = await _importantQuestionsRepository.GetOneAsync(a => a.Id == Id ,tracked: false);

            if ( importantQuestion is null )
                return NotFound();
            
            var response = new ImportantQuestionsResponse
            {
                Id = importantQuestion.Id,
                Question = importantQuestion.Question,
                Answer = importantQuestion.Answer,
            };

            return Ok(new
            {
                Data = response
            });
        }


        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create ( ImportantQuestionsRequest createImportantQuestionsRequest )
        {
            var importantQuestions = new ImportantQuestions { 
                Question = createImportantQuestionsRequest.Question,
                Answer = createImportantQuestionsRequest.Answer,
            };


            await _importantQuestionsRepository.AddAsync(importantQuestions);
            await _importantQuestionsRepository.CommitAsync();

            return CreatedAtAction(nameof(GetOne), new { id = importantQuestions.Id }, new
            {
                success_notifaction = "Add importantQuestions Successfully"
            });

        }


        [HttpPut("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit ( int Id, ImportantQuestionsRequest editImportantQuestionsRequest )
        {
            var importantQuestionsInDb = await _importantQuestionsRepository.GetOneAsync(a=> a.Id == Id, tracked:false) ;

            if ( importantQuestionsInDb is null )
                return NotFound();

            importantQuestionsInDb.Question = editImportantQuestionsRequest.Question;
            importantQuestionsInDb.Answer = editImportantQuestionsRequest.Answer;

            _importantQuestionsRepository.Update(importantQuestionsInDb);
            await _importantQuestionsRepository.CommitAsync();

            return NoContent();
        }


        [HttpDelete("{Id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete ( int Id )
        {
            var importantQuestionsInDb = await _importantQuestionsRepository.GetOneAsync(e => e.Id == Id, tracked:false);
            if ( importantQuestionsInDb is null )
                return NotFound();

            _importantQuestionsRepository.Delete(importantQuestionsInDb);
            await _importantQuestionsRepository.CommitAsync();

            return NoContent();
        }

    }
}
