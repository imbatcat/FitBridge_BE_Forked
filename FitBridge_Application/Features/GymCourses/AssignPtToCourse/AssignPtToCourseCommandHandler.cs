using AutoMapper;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Exceptions;
using MediatR;

namespace FitBridge_Application.Features.GymCourses.AssignPtToCourse
{
    internal class AssignPtToCourseCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper) : IRequestHandler<AssignPtToCourseCommand, Guid>
    {
        public async Task<Guid> Handle(AssignPtToCourseCommand request, CancellationToken cancellationToken)
        {
            var gymCourse = await unitOfWork.Repository<GymCourse>().GetByIdAsync(Guid.Parse(request.GymCourseId), includes: new List<string> { "GymCoursePTs" });
            if (gymCourse == null)
            {
                throw new NotFoundException("Không tìm thấy khóa học");
            }
            if (gymCourse.GymCoursePTs.Any(x => x.PTId == Guid.Parse(request.PtId)))
            {
                throw new DuplicateException("PT đã tồn tại trong khóa học này");
            }
            var mappedEntity = mapper.Map<AssignPtToCourseCommand, GymCoursePT>(request);
            var newId = Guid.NewGuid();
            mappedEntity.Id = newId;
            unitOfWork.Repository<GymCoursePT>().Insert(mappedEntity);

            await unitOfWork.CommitAsync();

            return newId;
        }
    }
}