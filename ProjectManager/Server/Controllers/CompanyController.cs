using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Shared.Dto;
using ProjectManager.Shared.Interfaces;

namespace ProjectManager.Server.Controllers {
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase {

        public CompanyController(ICompany companyModel) {
            CompanyModel = companyModel;
        }

        [Inject]
        protected ICompany CompanyModel { get; set; }

        


        [HttpGet]
        public async Task<IActionResult> Get() {
            try
            {
                List<CompanyDto> companies = await CompanyModel.List();
                return Ok(companies);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
        }

                [HttpGet("{id:guid}", Name = "Get")]
        public async Task<IActionResult> Get(Guid id) {
            try
            {
                CompanyDto company = await CompanyModel.Get(id);
                return Ok(company);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
        }

                [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyDto company) {
            try
            {
                CompanyDto newCompany = await CompanyModel.Create(company);
                return Ok(newCompany);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
        }

                [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] CompanyDto company) {
            try
            {
                CompanyDto updatedCompany = await CompanyModel.Update(id, company);
                return Ok(updatedCompany);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
        }

                [HttpPatch("{id:guid}")]
        [Obsolete("Use PUT: api/user/company")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] UserCompanyDto user) {
            try
            {
                CompanyDto company = await CompanyModel.ModifyUser(id, user);
                return Ok(company);
            }
            catch (Exception e)
            {
                return e switch
                {
                    InvalidDataException => BadRequest(e.Message),
                    UnauthorizedAccessException => Unauthorized(e.Message),
                    _ => e is DbUpdateException ? BadRequest(e.Message) : StatusCode(500, "Something went wrong")
                };
            }
        }

                [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) {
            try
            {
                Boolean deleted = await CompanyModel.Delete(id);
                return Ok(deleted);
            }
            catch (InvalidDataException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
