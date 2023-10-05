using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class User
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public int Age { get; set; }
	public string Email { get; set; } = string.Empty;
	public IEnumerable<Role> ?roles { get; set; }

}
