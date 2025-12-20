using System;
using AutoMapper;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Application.Dtos.Gym;
using FitBridge_Application.Features.GymCourses.AssignPtToCourse;
using FitBridge_Application.Features.GymCourses.CreateGymCourse;

namespace FitBridge_Application.MappingProfiles;

public class GymCoursesProfiles : Profile
{
    public GymCoursesProfiles()
    {
        CreateMap<CreateGymCourseCommand, GymCourse>()
        .ForMember(dest => dest.PtPrice, opt => opt.MapFrom(src => src.PtPrice));

        CreateMap<AssignPtToCourseCommand, GymCoursePT>();

        CreateProjection<GymCourse, GetGymCourseDto>()
            .ForMember(dest => dest.Image, opt => opt.MapFrom(
                src => src.ImageUrl))
            .ForMember(dest => dest.PtPrice, opt => opt.MapFrom(src => src.PtPrice));
    }
}