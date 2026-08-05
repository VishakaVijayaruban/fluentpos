// --------------------------------------------------------------------------------------------------
// <copyright file="PriceFileImportHandler.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Purchasing.Core.Abstractions;
using FluentPOS.Modules.Purchasing.Core.Exceptions;
using FluentPOS.Shared.Core.IntegrationServices.Catalog;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Commands
{
    internal class PriceFileImportHandler : IRequestHandler<ImportSupplierPriceFileCommand, Result<PriceFileImportSummary>>
    {
        private readonly IPurchasingDbContext _context;
        private readonly IProductService _productService;
        private readonly IStringLocalizer<PriceFileImportHandler> _localizer;

        public PriceFileImportHandler(
            IPurchasingDbContext context,
            IProductService productService,
            IStringLocalizer<PriceFileImportHandler> localizer)
        {
            _context = context;
            _productService = productService;
            _localizer = localizer;
        }

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
        public async Task<Result<PriceFileImportSummary>> Handle(ImportSupplierPriceFileCommand command, CancellationToken cancellationToken)
#pragma warning restore RCS1046 // Asynchronous method name should end with 'Async'.
        {
            if (!await _context.Suppliers.AnyAsync(s => s.Id == command.SupplierId, cancellationToken))
            {
                throw new PurchasingException(_localizer["Supplier Not Found!"], HttpStatusCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(command.Csv))
            {
                throw new PurchasingException(_localizer["The price file is empty."], HttpStatusCode.BadRequest);
            }

            var summary = new PriceFileImportSummary();
            string[] lines = command.Csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 2)
                {
                    summary.InvalidLines.Add(line);
                    continue;
                }

                string barcode = parts[0];
                if (!decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal cost))
                {
                    // Header rows land here (e.g. "barcode,cost,price").
                    summary.InvalidLines.Add(line);
                    continue;
                }

                decimal? sellPrice = null;
                if (parts.Length >= 3 && decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedPrice))
                {
                    sellPrice = parsedPrice;
                }

                summary.TotalLines++;
                if (await _productService.UpdatePricingByBarcodeAsync(barcode, cost, sellPrice))
                {
                    summary.Updated++;
                }
                else
                {
                    summary.UnmatchedBarcodes.Add(barcode);
                }
            }

            return await Result<PriceFileImportSummary>.SuccessAsync(summary, string.Format(_localizer["Price file processed: {0} updated, {1} unmatched."], summary.Updated, summary.UnmatchedBarcodes.Count));
        }
    }
}
