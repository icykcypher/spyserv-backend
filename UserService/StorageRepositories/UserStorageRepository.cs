﻿using AutoMapper;
using UserService.Data;
using UserService.Model;
using Microsoft.EntityFrameworkCore;

namespace UserService.StorageRepositories
{
    public class UserStorageRepository : IUserStorageRepository
    {
        private readonly IMapper _mapper;
        private readonly UserDbContext _dbContext;

        public UserStorageRepository(UserDbContext _dbContext, IMapper _mapper)
        {
            this._dbContext = _dbContext;
            this._mapper = _mapper;
        }

        public async Task<User?> AddNewUserAsync(User User)
        {
            var role = await _dbContext.Roles
                .Include(r => r.Permissions)
                .SingleOrDefaultAsync(x => x.Id == (int)Role.Admin)
                ?? throw new InvalidOperationException();

            var user = new User()
            {
                Id = User.Id,
                Name = User.Name,
                BirthdayDate = User.BirthdayDate,
                PasswordHash = User.PasswordHash,
                Email = User.Email,
                Roles = [role]
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<User?> DeleteUserByIdAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id);

            if (user == null) return null;
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>?> GetAllUsersAsync()
        {
            return await _dbContext.Users.AsNoTracking().ToListAsync();
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ICollection<RoleEntity>> GetUserPermissions(Guid id)
        {
            var roles = await _dbContext.Users
                .AsNoTracking()
                .Include(x => x.Roles)
                .ThenInclude(x => x.Permissions)
                .Where(x => x.Id == id)
                .Select(x => x.Roles)
                .ToArrayAsync();

            return roles
                .SelectMany(r => r)
                .ToArray();
        }
    }
}