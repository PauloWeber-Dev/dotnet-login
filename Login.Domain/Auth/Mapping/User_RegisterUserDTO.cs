using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Auth.DTO;
using Domain.Auth.Entities;

namespace Domain.Auth.Mapping
{
    internal class User_RegisterUserDTO: Profile
    {
        public User_RegisterUserDTO() {
            CreateMap<RegisterUserDto, User>()
            .ReverseMap();
        }
    }
}
