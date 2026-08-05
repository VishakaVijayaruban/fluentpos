using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentPOS.Modules.Sales.Core.Abstractions;
using FluentPOS.Modules.Sales.Core.Exceptions;
using FluentPOS.Modules.Sales.Core.Entities;
using FluentPOS.Shared.Core.Extensions;
using FluentPOS.Shared.Core.Mappings.Converters;
using FluentPOS.Shared.Core.Wrapper;
using FluentPOS.Shared.DTOs.Sales.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace FluentPOS.Modules.Sales.Core.Features.Sales.Queries
{
    internal class SalesQueryHandler :
                IRequestHandler<GetSalesQuery, PaginatedResult<GetSalesResponse>>,
                IRequestHandler<GetOrderByIdQuery, Result<GetOrderByIdResponse>>
    {
        private readonly ISalesDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SalesQueryHandler> _localizer;

        public SalesQueryHandler(
            ISalesDbContext context,
            IMapper mapper,
            IStringLocalizer<SalesQueryHandler> localizer)
        {
            _context = context;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<GetSalesResponse>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
        {
            // Filter and order on the entity, then project: EF cannot translate ordering
            // applied on top of the ProjectTo projection.
            var queryable = _context.Orders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchString))
            {
                queryable = queryable.Where(x => EF.Functions.Like(x.ReferenceNumber.ToLower(), $"%{request.SearchString.ToLower()}%")
                || EF.Functions.Like(x.Id.ToString().ToLower(), $"%{request.SearchString.ToLower()}%")
                || EF.Functions.Like(x.CustomerId.ToString().ToLower(), $"%{request.SearchString.ToLower()}%")
                || EF.Functions.Like(x.CustomerName.ToLower(), $"%{request.SearchString.ToLower()}%")
                || EF.Functions.Like(x.CustomerEmail.ToLower(), $"%{request.SearchString.ToLower()}%")
                || EF.Functions.Like(x.CustomerPhone.ToString().ToLower(), $"%{request.SearchString.ToLower()}%"));
            }

            var saleList = await queryable
                .OrderBy(x => x.TimeStamp)
                .ProjectTo<GetSalesResponse>(_mapper.ConfigurationProvider)
                .ToPaginatedListAsync(request.PageNumber, request.PageSize);

            if (saleList == null)
            {
                throw new SalesException(_localizer["Sales Not Found!"], HttpStatusCode.NotFound);
            }

            return _mapper.Map<PaginatedResult<GetSalesResponse>>(saleList);

        }

        public async Task<Result<GetOrderByIdResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.AsNoTracking()
                .Include(x => x.Products)
                .OrderBy(x => x.TimeStamp)
                .SingleOrDefaultAsync(x => x.Id == request.Id);

            if (order == null)
            {
                throw new SalesException(_localizer["Order Not Found!"], HttpStatusCode.NotFound);
            }

            var mappedData = _mapper.Map<Order, GetOrderByIdResponse>(order);

            return await Result<GetOrderByIdResponse>.SuccessAsync(data: mappedData);

        }
    }
}