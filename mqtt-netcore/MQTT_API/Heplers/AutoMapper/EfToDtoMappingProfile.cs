using AutoMapper;
using Data.Dto;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTT_API.Heplers.AutoMapper
{
    public class EfToDtoMappingProfile : Profile
    {
        public EfToDtoMappingProfile()
        {

            CreateMap<User, DtoCreateUpdateUser>();
            CreateMap<User, DtoReadUser>();

        }

    }
}
