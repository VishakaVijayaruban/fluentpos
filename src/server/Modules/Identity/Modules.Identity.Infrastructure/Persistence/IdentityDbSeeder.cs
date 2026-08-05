// --------------------------------------------------------------------------------------------------
// <copyright file="IdentityDbSeeder.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Constants;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Helpers;
using FluentPOS.Shared.Core.Constants;
using FluentPOS.Shared.Core.Interfaces.Services;
using FluentPOS.Shared.Infrastructure.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SharedPermissions = FluentPOS.Shared.Core.Constants.Permissions;

namespace FluentPOS.Modules.Identity.Infrastructure.Persistence
{
    internal class IdentityDbSeeder : IDatabaseSeeder
    {
        private readonly ILogger<IdentityDbSeeder> _logger;
        private readonly IIdentityDbContext _db;
        private readonly UserManager<FluentUser> _userManager;
        private readonly IStringLocalizer<IdentityDbSeeder> _localizer;
        private readonly RoleManager<FluentRole> _roleManager;

        public IdentityDbSeeder(
            ILogger<IdentityDbSeeder> logger,
            IIdentityDbContext db,
            RoleManager<FluentRole> roleManager,
            UserManager<FluentUser> userManager,
            IStringLocalizer<IdentityDbSeeder> localizer)
        {
            _logger = logger;
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _localizer = localizer;
        }

        public void Initialize()
        {
            AddDefaultRoles();
            AddSuperAdmin();
            AddStaff();
            AddStaffPermissions();
            AddFranchiseeManager();
            AddManagerPermissions();
            _db.SaveChanges();
        }

        // Sample franchisee manager: org-scoped (all of Northern Franchise's stores), not store-scoped.
        private void AddFranchiseeManager()
        {
            Task.Run(async () =>
            {
                var user = new FluentUser
                {
                    FirstName = "Farah",
                    LastName = "North",
                    Email = "franchisee@fluentpos.com",
                    UserName = "franchisee",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    OrganizationId = OrganizationConstants.FranchiseeOrganizationId
                };
                var userInDb = await _userManager.FindByEmailAsync(user.Email);
                if (userInDb == null)
                {
                    await _userManager.CreateAsync(user, UserConstants.DefaultPassword);
                    await _userManager.AddToRoleAsync(user, RoleConstants.Manager);
                    _logger.LogInformation(_localizer["Seeded Franchisee Manager."]);
                }
            }).GetAwaiter().GetResult();
        }

        private void AddManagerPermissions()
        {
            Task.Run(async () =>
            {
                var managerRole = await _roleManager.FindByNameAsync(RoleConstants.Manager);
                if (managerRole == null)
                {
                    return;
                }

                string[] managerPermissions =
                {
                    SharedPermissions.Reporting.View,
                    SharedPermissions.Reporting.Royalties,
                    SharedPermissions.Sales.View,
                    SharedPermissions.Sales.ViewAll,
                    SharedPermissions.Sales.Refund,
                    SharedPermissions.Stores.View,
                    SharedPermissions.Stores.ViewAll,
                    SharedPermissions.TillSessions.View,
                    SharedPermissions.TillSessions.ViewAll,
                    SharedPermissions.Products.View,
                    SharedPermissions.Products.ViewAll
                };

                foreach (string permission in managerPermissions)
                {
                    await _roleManager.AddPermissionClaimAsync(managerRole, permission);
                }
            }).GetAwaiter().GetResult();
        }

        // A cashier needs enough permissions to run the POS flow in their own store.
        private void AddStaffPermissions()
        {
            Task.Run(async () =>
            {
                var staffRole = await _roleManager.FindByNameAsync(RoleConstants.Staff);
                if (staffRole == null)
                {
                    return;
                }

                string[] staffPermissions =
                {
                    SharedPermissions.Products.View,
                    SharedPermissions.Products.ViewAll,
                    SharedPermissions.Brands.View,
                    SharedPermissions.Brands.ViewAll,
                    SharedPermissions.Categories.View,
                    SharedPermissions.Categories.ViewAll,
                    SharedPermissions.VatRates.ViewAll,
                    SharedPermissions.Customers.View,
                    SharedPermissions.Customers.ViewAll,
                    SharedPermissions.Customers.Register,
                    SharedPermissions.Carts.View,
                    SharedPermissions.Carts.ViewAll,
                    SharedPermissions.Carts.Create,
                    SharedPermissions.Carts.Remove,
                    SharedPermissions.CartItems.View,
                    SharedPermissions.CartItems.ViewAll,
                    SharedPermissions.CartItems.Add,
                    SharedPermissions.CartItems.Update,
                    SharedPermissions.CartItems.Remove,
                    SharedPermissions.Sales.View,
                    SharedPermissions.Sales.ViewAll,
                    SharedPermissions.Sales.Register,
                    SharedPermissions.Reporting.View
                };

                foreach (string permission in staffPermissions)
                {
                    await _roleManager.AddPermissionClaimAsync(staffRole, permission);
                }
            }).GetAwaiter().GetResult();
        }

        private void AddDefaultRoles()
        {
            Task.Run(async () =>
            {
                var roleList = new List<string> { RoleConstants.SuperAdmin, RoleConstants.Admin, RoleConstants.Manager, RoleConstants.Accountant, RoleConstants.Cashier, RoleConstants.Staff };
                foreach (string roleName in roleList)
                {
                    var role = new FluentRole(roleName);
                    var roleInDb = await _roleManager.FindByNameAsync(roleName);
                    if (roleInDb == null)
                    {
                        await _roleManager.CreateAsync(role);
                        _logger.LogInformation(string.Format(_localizer["Added '{0}' to Roles"], roleName));
                    }
                }
            }).GetAwaiter().GetResult();
        }

        private void AddSuperAdmin()
        {
            Task.Run(async () =>
            {
                // Check if Role Exists
                var superAdminRole = new FluentRole(RoleConstants.SuperAdmin);
                var superAdminRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.SuperAdmin);
                if (superAdminRoleInDb == null)
                {
                    await _roleManager.CreateAsync(superAdminRole);
                    superAdminRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.SuperAdmin);
                }

                // Check if User Exists
                var superUser = new FluentUser
                {
                    FirstName = "Mukesh",
                    LastName = "Murugan",
                    Email = "superadmin@fluentpos.com",
                    UserName = "superadmin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };
                var superUserInDb = await _userManager.FindByEmailAsync(superUser.Email);
                if (superUserInDb == null)
                {
                    await _userManager.CreateAsync(superUser, UserConstants.DefaultPassword);
                    var result = await _userManager.AddToRoleAsync(superUser, RoleConstants.SuperAdmin);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation(_localizer["Seeded Default SuperAdmin User."]);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError(error.Description);
                        }
                    }
                }

                foreach (string permission in typeof(Shared.Core.Constants.Permissions).GetNestedClassesStaticStringValues())
                {
                    await _roleManager.AddPermissionClaimAsync(superAdminRoleInDb, permission);
                }
            }).GetAwaiter().GetResult();
        }

        private void AddStaff()
        {
            Task.Run(async () =>
            {
                // Check if Role Exists
                var basicRole = new FluentRole(RoleConstants.Staff);
                var basicRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.Staff);
                if (basicRoleInDb == null)
                {
                    await _roleManager.CreateAsync(basicRole);
                    basicRoleInDb = await _roleManager.FindByNameAsync(RoleConstants.Staff);
                }

                // Check if User Exists
                var basicUser = new FluentUser
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "staff@fluentpos.com",
                    UserName = "staff",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true,
                    StoreId = OrganizationConstants.DefaultStoreId
                };
                var basicUserInDb = await _userManager.FindByEmailAsync(basicUser.Email);
                if (basicUserInDb == null)
                {
                    await _userManager.CreateAsync(basicUser, UserConstants.DefaultPassword);
                    await _userManager.AddToRoleAsync(basicUser, RoleConstants.Staff);
                    _logger.LogInformation(_localizer["Seeded Default Staff."]);
                }
            }).GetAwaiter().GetResult();
        }
    }
}