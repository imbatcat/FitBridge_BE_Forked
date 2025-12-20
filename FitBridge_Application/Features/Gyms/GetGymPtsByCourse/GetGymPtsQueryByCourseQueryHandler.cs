using AutoMapper;
using FitBridge_Application.Dtos;
using FitBridge_Application.Dtos.Gym;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Specifications.Gym.GetGymPtsByCourse;
using FitBridge_Domain.Entities.Gyms;
using MediatR;

namespace FitBridge_Application.Features.Gyms.GetGymPtsByCourse
{
    internal class GetGymPtsQueryByCourseQueryHandler(
        IUnitOfWork _unitOfWork,
        IMapper mapper) : IRequestHandler<GetGymPtsByCourseQuery, PagingResultDto<GetGymPtsDto>>
    {
        public async Task<PagingResultDto<GetGymPtsDto>> Handle(GetGymPtsByCourseQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetGymPtsByGymCourseSpecification(request.GymCourseId, request.GetGymPtsByCourseParams);
            var results = await _unitOfWork.Repository<GymCoursePT>().GetAllWithSpecificationProjectedAsync<GetGymPtsDto>(spec, mapper.ConfigurationProvider);
            var totalItems = await _unitOfWork.Repository<GymCoursePT>().CountAsync(spec);

            return new PagingResultDto<GetGymPtsDto>(totalItems, results);
        }
    }
}