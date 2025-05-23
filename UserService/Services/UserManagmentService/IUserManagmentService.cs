﻿using UserService.Model;

namespace UserService.Services.UserManagmentService
{
    public interface IUserManagmentService
    {
        Task<(string, User)> Login(SignInUserDto signInUserDto);
        Task<User> Register(RegisterUserDto registerUserDto);
    }
}