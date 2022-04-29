﻿using System.ComponentModel.DataAnnotations;

namespace PutAway.Shared.Entities.Dto;

public class UserDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string EmailAddress { get; set; }
}