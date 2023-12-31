﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProjectManager.Server.Data;
using ProjectManager.Shared.Dto;
using ProjectManager.Shared.Entities;
using ProjectManager.Shared.Interfaces;

namespace ProjectManager.Server.Models {
    public class ProjectModel : IProject {
        private readonly ApplicationDbContext _context;
        private readonly IUtility _utilityService;

        public ProjectModel(ApplicationDbContext context, IUtility utilityService) {
            _context = context;
            _utilityService = utilityService;
        }

                                                public async Task<List<ProjectDto>> List() {
            User? user = await _utilityService.GetUser();
            if (user == null) throw new InvalidDataException("Invalid User.");

            List<ProjectDto> projects = new List<ProjectDto>();

            List<Project> userProjects = await _context.Projects.Where(project => project.Users!.Contains(user)).ToListAsync();
            projects.AddRange(userProjects.Select(userProject => new ProjectDto(userProject)));

            return projects;
        }

                                                        public async Task<List<ProjectDto>> List(Guid companyId) {
            User? user = await _utilityService.GetUser();
            if (user == null) throw new InvalidDataException("Invalid User.");

            UserCompany? userCompany = await _context.UserCompany.FirstOrDefaultAsync(userCompany =>
                userCompany.UserId == user.Id && userCompany.CompanyId == companyId);
            if (userCompany == null) throw new InvalidDataException("Invalid Company.");

            List<ProjectDto> projects = new List<ProjectDto>();

                                    if (userCompany.Role == UserRole.Admin)
            {
                List<Project> userProjects = await _context.Projects.Include("Tickets").Include("Users").Include("Company")
                    .Where(project => project.Company!.Id == userCompany.CompanyId).ToListAsync();
                projects.AddRange(userProjects.Select(userProject => new ProjectDto(userProject)));
            }
            else
            {
                List<Project> userProjects = await _context.Projects.Include("Tickets").Include("Users").Include("Company")
                    .Where(project => project.Company!.Id == userCompany.CompanyId && project.Users!.Contains(user))
                    .ToListAsync();
                projects.AddRange(userProjects.Select(userProject => new ProjectDto(userProject)));
            }


            return projects;
        }

                                                        public async Task<ProjectDto> Get(Guid projectId) {
            User? user = await _utilityService.GetUser();
            if (user == null) throw new InvalidDataException("Invalid User.");

            Project? project =
                await _context.Projects.FirstOrDefaultAsync(project =>
                    project.Id == projectId && project.Users!.Contains(user));
            if (project == null) throw new InvalidDataException("Invalid ProjectId.");

            return new ProjectDto(project);
        }

                                                                public async Task<ProjectDto> Get(String companyUri, String projectUri) {
            User? user = await _utilityService.GetUser();
            if (user == null) throw new InvalidDataException("Invalid User.");

                        Project? project = await _context.Projects.Include("Users").FirstOrDefaultAsync(p =>
                p.Uri == projectUri &&
                p.Company!.Uri == companyUri &&
                (
                    p.Users!.Contains(user) ||
                    p.Company!.Users!.FirstOrDefault(uc => uc.User!.Id == user.Id && uc.Role == UserRole.Admin) != null
                )
            );
            if (project == null) throw new InvalidDataException("Invalid Project Uri.");

            return new ProjectDto(project);
        }

                                                public async Task<ProjectDto> Create(ProjectDto project) {
            (_, Company company) = await VerifyRequest(project);
            Project newProject = new Project
            {
                Id = default,
                Name = project.Name,
                Status = project.Status,
                DueDate = project.DueDate,
                Uri = project.Uri,
                Company = company
            };
            EntityEntry<Project> result = _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return new ProjectDto(result.Entity);
        }

                                                                public async Task<ProjectDto> Update(Guid projectId, ProjectDto project) {
            await VerifyRequest(project);

            Project? targetProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (targetProject == null) throw new InvalidDataException("Invalid projectId.");

            targetProject.Name = project.Name;
            targetProject.Status = project.Status;
            targetProject.DueDate = project.DueDate;
            targetProject.Uri = project.Uri;

            await _context.SaveChangesAsync();
            return new ProjectDto(targetProject);
        }

                                                        public async Task<Boolean> Delete(Guid projectId) {
            Project? project = await _context.Projects.Include("Company")
                .FirstOrDefaultAsync(project => project.Id == projectId);
            if (project == null) throw new InvalidDataException("Invalid projectId.");

            await VerifyRequest(new ProjectDto(project));

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

                                                                [Obsolete("Please use the UserModel.ModifyProject()")]
        public async Task<ProjectDto> ModifyUser(Guid projectId, UserDto userDto) {
            Project? project = await _context.Projects.FirstOrDefaultAsync(project => project.Id == projectId);
            if (project == null) throw new InvalidDataException("Invalid projectId.");

            await VerifyRequest(new ProjectDto(project));

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id);
            if (user == null) throw new InvalidDataException("Invalid UserId.");

            if (project.Users!.Contains(user))
                project.Users.Remove(user);
            else
                project.Users.Add(user);

            await _context.SaveChangesAsync();

            return new ProjectDto(project);
        }

                                                                        private async Task<Tuple<User, Company>> VerifyRequest(ProjectDto project, Boolean admin = true) {
            User? user = await _utilityService.GetUser();
            if (user == null) throw new InvalidDataException("Invalid User.");

            Company? company = await _context.Companies.FirstOrDefaultAsync(company => company.Id == project.Company.Id);
            if (company == null) throw new InvalidDataException("Invalid Company.");

            UserCompany? userCompany = await _context.UserCompany.FirstOrDefaultAsync(userCompany =>
                userCompany.Company == company && userCompany.User == user);

            if (userCompany == null) throw new UnauthorizedAccessException("User does not have required permission");

            if (admin && userCompany.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("User does not have required permission");

            return new Tuple<User, Company>(user, company);
        }
    }
}
