﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class UpdateUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    public UpdateUsersController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid key, UpdateUserRequestModel model)
    {
        IUser? existingUser = await _userService.GetAsync(key);

        if (existingUser is null)
        {
            return NotFound();
        }

        // We have to use and intermediate save model, and cannot map it directly to an IUserModel
        // This is because we need to compare the updated values with what the user already has, for audit purposes.
        UserUpdateModel updateModel = await _userPresentationFactory.CreateUpdateModelAsync(existingUser, model);

        Attempt<IUser, UserOperationStatus> result = await _userService.UpdateAsync(-1, updateModel);

        return result.Success
            ? Ok()
            : UserOperationStatusResult(result.Status);
    }
}
