// --------------------------------------------------------------------------------------------------
// <copyright file="Permissions.cs" company="FluentPOS">
// Copyright (c) FluentPOS. All rights reserved.
// The core team: Mukesh Murugan (iammukeshm), Chhin Sras (chhinsras), Nikolay Chebotov (unchase).
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

using System.ComponentModel;

namespace FluentPOS.Shared.Core.Constants
{
    public static class Permissions
    {
        [DisplayName("Users")]
        [Description("Users Permissions")]
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

        [DisplayName("Users Extended Attributes")]
        [Description("Users Extended Attributes Permissions")]
        public static class UsersExtendedAttributes
        {
            public const string View = "Permissions.Users.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Users.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Users.ExtendedAttributes.Add";
            public const string Update = "Permissions.Users.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Users.ExtendedAttributes.Remove";
        }

        [DisplayName("Roles")]
        [Description("Roles Permissions")]
        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
        }

        [DisplayName("Roles Extended Attributes")]
        [Description("Roles Extended Attributes Permissions")]
        public static class RolesExtendedAttributes
        {
            public const string View = "Permissions.Roles.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Roles.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Roles.ExtendedAttributes.Add";
            public const string Update = "Permissions.Roles.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Roles.ExtendedAttributes.Remove";
        }

        [DisplayName("Role Claims")]
        [Description("Role Claims Permissions")]
        public static class RoleClaims
        {
            public const string View = "Permissions.RoleClaims.View";
            public const string Create = "Permissions.RoleClaims.Create";
            public const string Edit = "Permissions.RoleClaims.Edit";
            public const string Delete = "Permissions.RoleClaims.Delete";
        }

        [DisplayName("Brands")]
        [Description("Brands Permissions")]
        public static class Brands
        {
            public const string View = "Permissions.Brands.View";
            public const string ViewAll = "Permissions.Brands.ViewAll";
            public const string Register = "Permissions.Brands.Register";
            public const string Update = "Permissions.Brands.Update";
            public const string Remove = "Permissions.Brands.Remove";
        }

        [DisplayName("Brands Extended Attributes")]
        [Description("Brands Extended Attributes Permissions")]
        public static class BrandsExtendedAttributes
        {
            public const string View = "Permissions.Brands.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Brands.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Brands.ExtendedAttributes.Add";
            public const string Update = "Permissions.Brands.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Brands.ExtendedAttributes.Remove";
        }

        [DisplayName("Customers")]
        [Description("Customers Permissions")]
        public static class Customers
        {
            public const string View = "Permissions.Customers.View";
            public const string ViewAll = "Permissions.Customers.ViewAll";
            public const string Register = "Permissions.Customers.Register";
            public const string Update = "Permissions.Customers.Update";
            public const string Remove = "Permissions.Customers.Remove";
        }

        [DisplayName("Customers Extended Attributes")]
        [Description("Customers Extended Attributes Permissions")]
        public static class CustomersExtendedAttributes
        {
            public const string View = "Permissions.Customers.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Customers.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Customers.ExtendedAttributes.Add";
            public const string Update = "Permissions.Customers.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Customers.ExtendedAttributes.Remove";
        }

        [DisplayName("Categories")]
        [Description("Categories Permissions")]
        public static class Categories
        {
            public const string View = "Permissions.Categories.View";
            public const string ViewAll = "Permissions.Categories.ViewAll";
            public const string Register = "Permissions.Categories.Register";
            public const string Update = "Permissions.Categories.Update";
            public const string Remove = "Permissions.Categories.Remove";
        }

        [DisplayName("Categories Extended Attributes")]
        [Description("Categories Extended Attributes Permissions")]
        public static class CategoriesExtendedAttributes
        {
            public const string View = "Permissions.Categories.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Categories.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Categories.ExtendedAttributes.Add";
            public const string Update = "Permissions.Categories.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Categories.ExtendedAttributes.Remove";
        }

        [DisplayName("Products")]
        [Description("Products Permissions")]
        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string ViewAll = "Permissions.Products.ViewAll";
            public const string Register = "Permissions.Products.Register";
            public const string Update = "Permissions.Products.Update";
            public const string Remove = "Permissions.Products.Remove";
        }

        [DisplayName("VatRates")]
        [Description("VAT Rates Permissions")]
        public static class VatRates
        {
            public const string ViewAll = "Permissions.VatRates.ViewAll";
        }

        [DisplayName("Organizations")]
        [Description("Organizations Permissions")]
        public static class Organizations
        {
            public const string ViewAll = "Permissions.Organizations.ViewAll";
            public const string Register = "Permissions.Organizations.Register";
            public const string Update = "Permissions.Organizations.Update";
        }

        [DisplayName("Reporting")]
        [Description("Reporting Permissions")]
        public static class Reporting
        {
            public const string View = "Permissions.Reporting.View";
            public const string Royalties = "Permissions.Reporting.Royalties";
        }

        [DisplayName("Stores")]
        [Description("Stores Permissions")]
        public static class Stores
        {
            public const string View = "Permissions.Stores.View";
            public const string ViewAll = "Permissions.Stores.ViewAll";
            public const string Register = "Permissions.Stores.Register";
            public const string Update = "Permissions.Stores.Update";
            public const string Remove = "Permissions.Stores.Remove";
        }

        [DisplayName("Terminals")]
        [Description("Terminals Permissions")]
        public static class Terminals
        {
            public const string ViewAll = "Permissions.Terminals.ViewAll";
            public const string Register = "Permissions.Terminals.Register";
        }

        [DisplayName("StoreProducts")]
        [Description("Store Products Permissions")]
        public static class StoreProducts
        {
            public const string ViewAll = "Permissions.StoreProducts.ViewAll";
            public const string Upsert = "Permissions.StoreProducts.Upsert";
            public const string Remove = "Permissions.StoreProducts.Remove";
        }

        [DisplayName("Suppliers")]
        [Description("Suppliers Permissions")]
        public static class Suppliers
        {
            public const string View = "Permissions.Suppliers.View";
            public const string ViewAll = "Permissions.Suppliers.ViewAll";
            public const string Register = "Permissions.Suppliers.Register";
            public const string Update = "Permissions.Suppliers.Update";
            public const string Remove = "Permissions.Suppliers.Remove";
            public const string ImportPrices = "Permissions.Suppliers.ImportPrices";
        }

        [DisplayName("PurchaseOrders")]
        [Description("Purchase Orders Permissions")]
        public static class PurchaseOrders
        {
            public const string View = "Permissions.PurchaseOrders.View";
            public const string ViewAll = "Permissions.PurchaseOrders.ViewAll";
            public const string Register = "Permissions.PurchaseOrders.Register";
            public const string Update = "Permissions.PurchaseOrders.Update";
            public const string Receive = "Permissions.PurchaseOrders.Receive";
            public const string Remove = "Permissions.PurchaseOrders.Remove";
        }

        [DisplayName("Replenishment")]
        [Description("Replenishment Permissions")]
        public static class Replenishment
        {
            public const string Run = "Permissions.Replenishment.Run";
        }

        [DisplayName("TillSessions")]
        [Description("Till Sessions Permissions")]
        public static class TillSessions
        {
            public const string View = "Permissions.TillSessions.View";
            public const string ViewAll = "Permissions.TillSessions.ViewAll";
            public const string Open = "Permissions.TillSessions.Open";
            public const string Close = "Permissions.TillSessions.Close";
            public const string RecordCashMovement = "Permissions.TillSessions.RecordCashMovement";
        }

        [DisplayName("Products Extended Attributes")]
        [Description("Products Extended Attributes Permissions")]
        public static class ProductsExtendedAttributes
        {
            public const string View = "Permissions.Products.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Products.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Products.ExtendedAttributes.Add";
            public const string Update = "Permissions.Products.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Products.ExtendedAttributes.Remove";
        }

        [DisplayName("Carts")]
        [Description("Carts Permissions")]
        public static class Carts
        {
            public const string View = "Permissions.Carts.View";
            public const string ViewAll = "Permissions.Carts.ViewAll";
            public const string Create = "Permissions.Carts.Create";
            public const string Remove = "Permissions.Carts.Remove";
        }

        [DisplayName("Carts Extended Attributes")]
        [Description("Carts Extended Attributes Permissions")]
        public static class CartsExtendedAttributes
        {
            public const string View = "Permissions.Carts.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.Carts.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.Carts.ExtendedAttributes.Add";
            public const string Update = "Permissions.Carts.ExtendedAttributes.Update";
            public const string Remove = "Permissions.Carts.ExtendedAttributes.Remove";
        }

        [DisplayName("Cart Items")]
        [Description("Cart Items Permissions")]
        public static class CartItems
        {
            public const string View = "Permissions.CartItems.View";
            public const string ViewAll = "Permissions.CartItems.ViewAll";
            public const string Add = "Permissions.CartItems.Add";
            public const string Update = "Permissions.CartItems.Update";
            public const string Remove = "Permissions.CartItems.Remove";
        }

        [DisplayName("Cart Items Extended Attributes")]
        [Description("Cart Items Extended Attributes Permissions")]
        public static class CartItemsExtendedAttributes
        {
            public const string View = "Permissions.CartItems.ExtendedAttributes.View";
            public const string ViewAll = "Permissions.CartItems.ExtendedAttributes.ViewAll";
            public const string Add = "Permissions.CartItems.ExtendedAttributes.Add";
            public const string Update = "Permissions.CartItems.ExtendedAttributes.Update";
            public const string Remove = "Permissions.CartItems.ExtendedAttributes.Remove";
        }

        [DisplayName("Event Logs")]
        [Description("Event Logs Permissions")]
        public static class EventLogs
        {
            public const string ViewAll = "Permissions.EventLogs.ViewAll";
            public const string Create = "Permissions.EventLogs.Create";
        }

        [DisplayName("Sales")]
        [Description("Sales Permissions")]
        public static class Sales
        {
            public const string View = "Permissions.Sales.View";
            public const string ViewAll = "Permissions.Sales.ViewAll";
            public const string Register = "Permissions.Sales.Register";
            public const string Update = "Permissions.Sales.Update";
            public const string Remove = "Permissions.Sales.Remove";
            public const string Refund = "Permissions.Sales.Refund";
        }
    }
}