using System;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace FitBridge_Application.Features.Accounts.UpdateGymPtMinimumSlot;

public class UpdateGymPtMinimumSlotCommand : IRequest<bool>
{
    [Required]
    public int MinimumSlot { get; set; }
}
