using System;
using MediatR;

namespace FitBridge_Application.Features.GymCourses.DeletePtFromCourse;

public class DeletePtFromCourseCommand : IRequest<bool>
{
    public Guid GymCoursePTId { get; set; }
}
