using AutoMapper;
using WorkLabLibrary.Models;
using WorkLabWeb.Areas.Users.Models;

namespace CodeskWeb.Mappers
{
    public class SignUpViewModel_User : Profile
    {
        public SignUpViewModel_User()
        {
            CreateMap<SignUpViewModel, User>()
                .ForMember(
                    d => d.PasswordText,
                    o => o.MapFrom(s => s.Password));
        }
    }
}