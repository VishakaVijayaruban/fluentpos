// --------------------------------------------------------------------------------------------------
// <copyright file="Order.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;
using FluentPOS.Shared.DTOs.People.Customers;

namespace FluentPOS.Modules.Sales.Core.Entities
{
    public class Order : BaseEntity, IMustHaveStore
    {
        public Guid StoreId { get; set; }

        public string ReferenceNumber { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public Guid CustomerId { get; private set; }

        public string CustomerName { get; private set; }

        public string CustomerPhone { get; private set; }

        public string CustomerEmail { get; private set; }

        public decimal SubTotal { get; private set; }

        public decimal Tax { get; private set; }

        public decimal Discount { get; private set; }

        public decimal Total { get; private set; }

        public bool IsPaid { get; private set; }

        public string Note { get; private set; }

        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public static Order InitializeOrder()
        {
            // UTC is required: the persisted column is 'timestamp with time zone'.
            return new Order { TimeStamp = DateTime.UtcNow };
        }

        public void MarkAsPaid()
        {
            IsPaid = true;
        }

        public void AddCustomer(GetCustomerByIdResponse customer)
        {
            CustomerId = customer.Id;
            CustomerName = customer.Name;
            CustomerEmail = customer.Email;
            CustomerPhone = customer.Phone;
        }

        public void SetReferenceNumber(string referenceNumber)
        {
            ReferenceNumber = referenceNumber;
        }

        public void AddProduct(Product product)
        {
            Products.Add(product);
        }

        internal void AddProduct(Guid productId, string name, int quantity, decimal rate, decimal vatRatePercent)
        {
            decimal linePrice = quantity * rate;
            decimal lineTax = linePrice * vatRatePercent / 100m;

            Products.Add(new Product
            {
                ProductId = productId,
                Quantity = quantity,
                Tax = lineTax,
                Price = linePrice,
                Total = linePrice + lineTax
            });

            SubTotal += linePrice;
            Tax += lineTax;
            Total = SubTotal + Tax - Discount;
        }
    }
}