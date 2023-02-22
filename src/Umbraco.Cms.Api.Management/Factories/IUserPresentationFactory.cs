﻿using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IUserPresentationFactory
{
    UserResponseModel CreateResponseModel(IUser user);
}
