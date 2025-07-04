
using AutoMapper;
using DTO.Auth;
using Domain.Repository.Entities;

namespace Login.Domain.Repository.Mapping;
internal class User_RegisterUserDTO: Profile
{
    public User_RegisterUserDTO() {
        CreateMap<RegisterUserDto, User>()
        .ReverseMap();
    }
}
