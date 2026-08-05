// --------------------------------------------------------------------------------------------------
// <copyright file="ICustomerService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Shared.DTOs.People.Customers;

namespace FluentPOS.Shared.Core.IntegrationServices.People
{
    /// <summary>
    /// Integration Services for the People Module.
    /// </summary>
    public interface ICustomerService
    {
        Task<GetCustomerByIdResponse> GetDetailsAsync(Guid customerId);
    }
}
