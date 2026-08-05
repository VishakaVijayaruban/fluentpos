// --------------------------------------------------------------------------------------------------
// <copyright file="ImportSupplierPriceFileCommand.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Wrapper;
using MediatR;

namespace FluentPOS.Modules.Purchasing.Core.Features.Suppliers.Commands
{
    /// <summary>
    /// Imports a wholesaler price file (Booker/Bestway-style CSV: barcode,cost[,price])
    /// and applies cost/price updates to products matched by barcode.
    /// </summary>
    public class ImportSupplierPriceFileCommand : IRequest<Result<PriceFileImportSummary>>
    {
        public Guid SupplierId { get; set; }

        // Raw CSV content. Lines: <barcode>,<cost>[,<sellPrice>]. Header rows are skipped.
        public string Csv { get; set; }
    }

    public class PriceFileImportSummary
    {
        public int TotalLines { get; set; }

        public int Updated { get; set; }

        public List<string> UnmatchedBarcodes { get; set; } = new();

        public List<string> InvalidLines { get; set; } = new();
    }
}
