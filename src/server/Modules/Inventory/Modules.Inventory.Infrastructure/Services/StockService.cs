// --------------------------------------------------------------------------------------------------
// <copyright file="StockService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentPOS.Modules.Inventory.Core.Abstractions;
using FluentPOS.Modules.Inventory.Core.Entities;
using FluentPOS.Modules.Inventory.Core.Enums;
using FluentPOS.Shared.Core.Enums;
using FluentPOS.Shared.Core.IntegrationServices.Inventory;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Inventory.Infrastructure.Services
{
    /// <inheritdoc/>
    public class StockService : IStockService
    {
        private readonly IInventoryDbContext _context;

        /// <summary>
        /// Stock Service.
        /// </summary>
        /// <param name="context">Context.</param>
        public StockService(
            IInventoryDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task RecordTransaction(Guid productId, decimal quantity, string referenceNumber, Guid storeId, StockTransactionKind kind = StockTransactionKind.Sale)
        {
            // TODO - Move this to MediatR, maybe? - Important, DO NOT make an API endpoint for this.

            var transactionType = kind switch
            {
                StockTransactionKind.Sale => TransactionType.Sale,
                StockTransactionKind.Purchase => TransactionType.Purchase,
                StockTransactionKind.Return => TransactionType.Return,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            var stockTransaction = new StockTransaction(productId, quantity, transactionType, referenceNumber, storeId);
            await _context.StockTransactions.AddAsync(stockTransaction);

            var stockRecord = _context.Stocks.FirstOrDefault(s => s.ProductId == productId && s.StoreId == storeId);
            if (stockRecord == null)
            {
                stockRecord = new Stock(productId, storeId);
                _context.Stocks.Add(stockRecord);
            }
            else
            {
                _context.Stocks.Update(stockRecord);
            }

            if (transactionType == TransactionType.Sale)
            {
                stockRecord.ReduceQuantity(quantity);
            }
            else
            {
                // Purchases and returns both put stock back on the shelf.
                stockRecord.IncreaseQuantity(quantity);
            }

            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<decimal> GetAvailableQuantityAsync(Guid productId, Guid storeId)
        {
            return await _context.Stocks.AsNoTracking()
                .Where(s => s.ProductId == productId && s.StoreId == storeId)
                .Select(s => s.AvailableQuantity)
                .FirstOrDefaultAsync();
        }
    }
}