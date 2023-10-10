using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class User
{
	[Required]
	public int Id { get; set; }
	[Required]
	public string Name { get; set; } = string.Empty;
	[Required]
	public int Age { get; set; }
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;
	[Required]
	public string Password { get; set; } = string.Empty;
	[Required]
	public ICollection<Role> ?Roles { get; set; }

}
