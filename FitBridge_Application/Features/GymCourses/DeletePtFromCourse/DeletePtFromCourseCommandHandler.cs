using System;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.GymCourses.DeletePtFromCourse;

public class DeletePtFromCourseCommandHandler(IUnitOfWork _unitOfWork) : IRequestHandler<DeletePtFromCourseCommand, bool>
{
    public async Task<bool> Handle(DeletePtFromCourseCommand request, CancellationToken cancellationToken)
    {
        var gymCoursePT = await _unitOfWork.Repository<GymCoursePT>().GetByIdAsync(request.GymCoursePTId);
        if (gymCoursePT == null)
        {
            throw new NotFoundException("Không tìm thấy Pt trong khóa học");
        }
        _unitOfWork.Repository<GymCoursePT>().Delete(gymCoursePT);
        await _unitOfWork.CommitAsync();
        return true;
    }
}
