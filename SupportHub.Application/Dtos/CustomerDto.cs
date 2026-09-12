using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupportHub.Application.Dtos;

public record CustomerDto(int Id, string Name, string Email)
{

}

public class CustomerCreateDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = string.Empty;
}

public class CustomerUpdateDto
{
    [Required,MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [Required,EmailAddress,MaxLength(320)]
    public string Email { get; set; } = string.Empty;
}