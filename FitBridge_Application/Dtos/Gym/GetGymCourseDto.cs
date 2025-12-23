using System;
using FitBridge_Application.Dtos.GymCoursePts;

namespace FitBridge_Application.Dtos.Gym
{
    public class GetGymCourseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public decimal PtPrice { get; set; }
        public int Duration { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;
        public List<GymCoursePtDto> GymCoursePTs { get; set; } = new List<GymCoursePtDto>();
    }
}