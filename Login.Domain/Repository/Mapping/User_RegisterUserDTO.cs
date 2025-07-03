
using AutoMapper;
using Domain.Auth.DTO;
using Domain.Repository.Entities;

namespace Login.Domain.Repository.Mapping;
internal class User_RegisterUserDTO: Profile
{
    public User_RegisterUserDTO() {
        CreateMap<RegisterUserDto, User>()
        .ReverseMap();
    }
}
