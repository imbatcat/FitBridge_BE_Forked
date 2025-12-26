using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FitBridge_Application.Features.Accounts.UpdateGymPtMinimumSlot;

public class UpdateGymOwnerMinimumSlotCommand : IRequest<bool>
{
    [Required]
    public int MinimumSlot { get; set; }
}
