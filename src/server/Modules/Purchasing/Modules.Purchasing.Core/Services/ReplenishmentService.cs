// --------------------------------------------------------------------------------------------------
// <copyright file="ReplenishmentService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Entities;
using FluentPOS.Modules.Purchasing.Core.Enums;
using FluentPOS.Shared.Core.IntegrationServices.Application;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.Core.IntegrationServices.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluentPOS.Modules.Purchasing.Core.Services
{
    public class ReplenishmentService : IReplenishmentService
    {
        private readonly IPurchasingDbContext _context;
        private readonly IStoreProductService _storeProductService;
        private readonly IStockService _stockService;
        private readonly IEntityReferenceService _referenceService;
        private readonly ILogger<ReplenishmentService> _logger;

        public ReplenishmentService(
            IPurchasingDbContext context,
            IStoreProductService storeProductService,
            IStockService stockService,
            IEntityReferenceService referenceService,
            ILogger<ReplenishmentService> logger)
        {
            _context = context;
            _storeProductService = storeProductService;
            _stockService = stockService;
            _referenceService = referenceService;
            _logger = logger;
        }

        public async Task<ReplenishmentRunSummary> RunAsync()
        {
            var candidates = await _storeProductService.GetReorderCandidatesAsync();

            // Products already on an open purchase order must not be re-ordered.
            var openOrderLines = await _context.PurchaseOrders.AsNoTracking()
                .Where(po => po.Status == PurchaseOrderStatus.Draft || po.Status == PurchaseOrderStatus.Submitted)
                .SelectMany(po => po.Items.Select(i => new { po.StoreId, i.ProductId }))
                .ToListAsync();
            var openSet = openOrderLines.Select(l => (l.StoreId, l.ProductId)).ToHashSet();

            var toOrder = new List<(Guid StoreId, Guid? SupplierId, Guid ProductId, string ProductName, decimal Quantity, decimal UnitCost)>();
            foreach (var candidate in candidates)
            {
                if (openSet.Contains((candidate.StoreId, candidate.ProductId)))
                {
                    continue;
                }

                decimal available = await _stockService.GetAvailableQuantityAsync(candidate.ProductId, candidate.StoreId);
                if (available <= candidate.ReorderPoint)
                {
                    toOrder.Add((candidate.StoreId, candidate.PreferredSupplierId, candidate.ProductId, candidate.ProductName, candidate.ReorderQuantity, candidate.UnitCost));
                }
            }

            int ordersCreated = 0;
            int linesAdded = 0;
            foreach (var group in toOrder.GroupBy(l => (l.StoreId, l.SupplierId)))
            {
                var order = new PurchaseOrder
                {
                    StoreId = group.Key.StoreId,
                    SupplierId = group.Key.SupplierId,
                    Notes = "Auto-replenishment"
                };
                order.SetReferenceNumber(await _referenceService.TrackAsync(nameof(PurchaseOrder)));

                foreach (var line in group)
                {
                    order.AddItem(line.ProductId, line.ProductName, line.Quantity, line.UnitCost);
                    linesAdded++;
                }

                await _context.PurchaseOrders.AddAsync(order);
                ordersCreated++;
            }

            if (ordersCreated > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Replenishment run: {Candidates} candidates scanned, {Orders} draft orders created, {Lines} lines added.", candidates.Count, ordersCreated, linesAdded);
            return new ReplenishmentRunSummary(ordersCreated, linesAdded, candidates.Count);
        }
    }
}
