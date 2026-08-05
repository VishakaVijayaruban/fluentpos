// --------------------------------------------------------------------------------------------------
// <copyright file="SalesProjectionHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Reporting.Core.Abstractions;
using FluentPOS.Modules.Reporting.Core.Entities;
using FluentPOS.Shared.Core.IntegrationEvents.Sales;
using FluentPOS.Shared.Core.IntegrationServices.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluentPOS.Modules.Reporting.Core.EventHandlers
{
    /// <summary>
    /// Projects sales integration events into the per-store daily read model, including
    /// franchise royalty accrual. Failures are logged but never break the sale itself.
    /// </summary>
    internal class SalesProjectionHandler :
        INotificationHandler<OrderRegisteredEvent>,
        INotificationHandler<OrderRefundedEvent>
    {
        private readonly IReportingDbContext _context;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<SalesProjectionHandler> _logger;

        public SalesProjectionHandler(
            IReportingDbContext context,
            IOrganizationService organizationService,
            ILogger<SalesProjectionHandler> logger)
        {
            _context = context;
            _organizationService = organizationService;
            _logger = logger;
        }

        public async Task Handle(OrderRegisteredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var row = await GetOrStartRowAsync(notification.StoreId, notification.OccurredOn, cancellationToken);
                row.ApplySale(notification.Total, notification.Tax);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to project OrderRegisteredEvent for order {OrderId}; the sale itself is unaffected.", notification.OrderId);
            }
        }

        public async Task Handle(OrderRefundedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                // Refunds are booked against the day they happen, not the day of the original sale.
                var row = await GetOrStartRowAsync(notification.StoreId, notification.OccurredOn, cancellationToken);
                row.ApplyRefund(notification.RefundedTotal);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to project OrderRefundedEvent for order {OrderId}; the refund itself is unaffected.", notification.OrderId);
            }
        }

        private async Task<DailyStoreSales> GetOrStartRowAsync(Guid storeId, DateTime occurredOn, CancellationToken cancellationToken)
        {
            var date = occurredOn.Date;
            var row = await _context.DailyStoreSales
                .FirstOrDefaultAsync(r => r.StoreId == storeId && r.Date == date, cancellationToken);
            if (row != null)
            {
                return row;
            }

            var orgInfo = await _organizationService.GetStoreOrganizationAsync(storeId)
                ?? new StoreOrganizationInfo(Guid.Empty, "Unknown", 0m);
            row = DailyStoreSales.Start(storeId, date, orgInfo.OrganizationId, orgInfo.OrganizationName, orgInfo.RoyaltyRatePercent);
            await _context.DailyStoreSales.AddAsync(row, cancellationToken);
            return row;
        }
    }
}
