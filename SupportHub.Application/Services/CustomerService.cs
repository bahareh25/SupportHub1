using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SupportHub.Application.Services;

public class CustomerService : ICustomerService
{
    private IApplicationDbContext _db;
    private ILogger<CustomerService> _logger;

    public CustomerService(IApplicationDbContext db, ILogger<CustomerService> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto customerCreateDto, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Name = customerCreateDto.Name.Trim(),
            Email = customerCreateDto.Email.Trim()
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created customer {CustomerId}", customer.Id);
        return new CustomerDto(customer.Id,customer.Name,customer.Email);
        
    }

    public async Task<bool> CustomerExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Customers.AnyAsync(x=>x.Id==id,cancellationToken);
    }

    public async Task<bool> CustomerHasTicketsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tickets.AnyAsync(c=> c.Id==id,cancellationToken);
    }

    public async Task DeleteCustomerAsync(int id, CancellationToken cancellationToken = default)
    {
       var customer= await _db.Customers.FirstOrDefaultAsync(c=>c.Id==id,cancellationToken);
        if (customer is null)
            return;
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted customer {CustomerId}", id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
    {
        var trime = email.Trim();
        var query = _db.Customers.Where(c => c.Email == trime);
        if (excludeCustomerId is not  null)
            query=query.Where(c=>c.Id!=excludeCustomerId);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Customers.AsNoTracking().OrderBy(c => c.Id).Select(c => new CustomerDto(c.Id, c.Name, c.Email)).ToListAsync(cancellationToken);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
       return await  _db.Customers
                    .AsNoTracking()
                    .Where(c=>c.Id == id)
                    .Select(c=>new CustomerDto(c.Id, c.Name,c.Email))
                    .FirstOrDefaultAsync(cancellationToken);
       
    }

    public async Task UpdateCustomerAsync(int id, CustomerUpdateDto customerUpdateDto, CancellationToken cancellationToken = default)
    {
        var customer=await  _db.Customers.FirstOrDefaultAsync(c=> c.Id == id,cancellationToken);
        if (customer is null)
            return;
        customer.Name = customerUpdateDto.Name.Trim();
        customer.Email = customerUpdateDto.Email.Trim();
        await  _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated customer {CustomerId}", id);
    }
}
