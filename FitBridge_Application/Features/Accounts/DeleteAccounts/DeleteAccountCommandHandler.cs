using FitBridge_Application.Commons.Constants;
using FitBridge_Application.Interfaces.Repositories;
using FitBridge_Application.Interfaces.Services;
using FitBridge_Application.Interfaces.Utils;
using FitBridge_Application.Specifications.Accounts.GetAllGymPts;
using FitBridge_Application.Specifications.Accounts.GetUsersByIds;
using FitBridge_Application.Specifications.GymCoursePts.GetGymCoursePtByPtId;
using FitBridge_Domain.Entities.Gyms;
using FitBridge_Domain.Entities.Identity;
using FitBridge_Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitBridge_Application.Features.Accounts.DeleteAccounts
{
    internal class DeleteAccountCommandHandler(
        IApplicationUserService applicationUserService,
        IUserUtil userUtil,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<DeleteAccountCommand>
    {
        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var userId = userUtil.GetAccountId(httpContextAccessor.HttpContext!)
             ?? throw new NotFoundException(nameof(ApplicationUser));
            var currentUser = await applicationUserService.GetByIdAsync(userId);

            var userRole = await applicationUserService.GetUserRoleAsync(currentUser);

            if (userRole.Equals(ProjectConstant.UserRoles.Admin))
            {
                await DeleteAccounts(request.UserIdDeleteList);
            }
            else
            {
                await DeleteGymPts(request.UserIdDeleteList, userId);
            }
            
            await unitOfWork.CommitAsync();
        }

        private async Task DeleteAccounts(List<Guid> userIdDeleteList)
        {
            var spec = new GetUsersByIdsSpec(userIdDeleteList);
            var users = await applicationUserService.GetAllUsersWithSpecAsync(spec, asNoTracking: false);

            foreach (var user in users)
            {
                user.IsEnabled = false;
            }
            
        }

        private async Task DeleteGymPts(List<Guid> userIdDeleteList, Guid gymOwnerId)
        {
            var spec = new GetAllGymPtsSpec(userIdDeleteList, gymOwnerId);
            var users = await applicationUserService.GetAllUsersWithSpecAsync(spec, asNoTracking: false);

            foreach (var user in users)
            {
                user.IsEnabled = false;
                
                var gymCoursePtByPtIdSpec = new GetGymCoursePtByPtIdSpec(user.Id);
                var gymCoursePTs = await unitOfWork.Repository<GymCoursePT>()
                    .GetAllWithSpecificationAsync(gymCoursePtByPtIdSpec);
                
                foreach (var gymCoursePT in gymCoursePTs)
                {
                    unitOfWork.Repository<GymCoursePT>().Delete(gymCoursePT);
                }
            }            
        }
    }
}