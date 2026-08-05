// --------------------------------------------------------------------------------------------------
// <copyright file="IStockService.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentPOS.Shared.Core.Enums;

namespace FluentPOS.Shared.Core.IntegrationServices.Inventory
{
    /// <summary>
    /// Integration Services for Inventory Module.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Record Transaction.
        /// </summary>
        /// <param name="productId">Product Id.</param>
        /// <param name="quantity">Quantity.</param>
        /// <param name="referenceNumber">Reference Number.</param>
        /// <param name="storeId">Store the stock movement belongs to.</param>
        /// <param name="kind">Movement direction: Sale decreases, Purchase/Return increase.</param>
        /// <returns>Task Completed.</returns>
        public Task RecordTransaction(Guid productId, decimal quantity, string referenceNumber, Guid storeId, StockTransactionKind kind = StockTransactionKind.Sale);

        /// <summary>
        /// Current stock level for a product in a store (0 when no stock record exists).
        /// </summary>
        public Task<decimal> GetAvailableQuantityAsync(Guid productId, Guid storeId);
    }
}