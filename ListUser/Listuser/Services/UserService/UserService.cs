using ListUser.Data;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ListUser.Services.Util;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;

namespace ListUser.Services.UserService;

public class UserService :	IUserService
{
	private readonly int _maxPageSize = 10;
	private readonly IConfiguration _configuration;
	private readonly IWebHostEnvironment _webHostEnvironment;
	private AppDbContext _context;
	UserValidator _validator;
	public UserService(AppDbContext context, [FromServices] IConfiguration configuration,
		IWebHostEnvironment webHostEnvironment)
	{
		_context = context;
		_configuration = configuration;
		_webHostEnvironment = webHostEnvironment;
		_validator = new UserValidator(this);
	}
	public async Task<ResponseData<ListModel<User>>> GetUserListAsync(
								   int? pageNo = null,
								   int? pageSize = null,
								   string? filterBy = null, string? filterValue = null,
								   string? sortBy = null, string? sortOrder = null)
	{
		//var query = _context.Users.AsQueryable();
		var query = _context.Users.Include(u => u.Roles).AsQueryable();
		//IQueryable<User> query = _context.Users.AsQueryable().Include(u => u.Roles);
		var dataList = new ListModel<User>();

		if ((filterBy == null && filterValue != null) || (filterBy != null && filterValue == null))
		{
			return new ResponseData<ListModel<User>>
			{
				Data = null,
				Success = false,
				ErrorMessage = "Incorrect filter options"
			};
		}

		if ((sortBy == null && sortOrder != null) || (sortBy != null && sortOrder == null))
		{
			return new ResponseData<ListModel<User>>
			{
				Data = null,
				Success = false,
				ErrorMessage = "Incorrect sort options"
			};
		}

		// количество элементов в списке
		var count = query.Count();
		if (count == 0)
		{
			return new ResponseData<ListModel<User>>
			{
				Data = dataList
			};
		}

		if (pageSize != null && pageNo == null)
		{
			pageNo = 1;
			if (pageSize > _maxPageSize)
				pageSize = _maxPageSize;
		}

		if (pageSize == null && pageNo != null)
		{
			pageSize = _maxPageSize;
		}

		if (pageSize == null && pageNo == null)
		{
			pageSize = count;
			pageNo = 1;
		}

		int totalPages = (int)Math.Ceiling(count / (double)(pageSize));
		if (pageNo > totalPages || pageNo < 1)
				return new ResponseData<ListModel<User>>
				{
					Data = null,
					Success = false,
					ErrorMessage = "No such page"
				};
		

		if (filterBy != null && filterValue != null)
		{
			if (this.DoesPropertyExist((User)query.First(), filterBy))
			{
				if (filterBy == "Roles")
				{
					int id;
					bool result = int.TryParse(filterValue, out id);

					if (result)
					{
						var role = await _context.Roles.FindAsync(id);
						if (role == null)
						{
							return new ResponseData<ListModel<User>>
							{
								Data = null,
								Success = false,
								ErrorMessage = "There's no such role: " + filterValue
							};
						}
						query = query.Include(u => u.Roles)
						.Where(u => u.Roles.Any(r => r.Id == id))
						.AsQueryable();
					}
					else
					{
						var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == filterValue);
						if (role == null)
						{
							return new ResponseData<ListModel<User>>
							{
								Data = null,
								Success = false,
								ErrorMessage = "There's no such role: " + filterValue
							};
						}
						if (role.Users == null)
						{
							return new ResponseData<ListModel<User>>
							{
								Data = null,
								Success = true,
							};
						}
						query = query.Include(u => u.Roles)
						.Where(u => u.Roles.Any(r => r.Name == filterValue))
						.AsQueryable();
					}
				}
				else
				{
					string s = $"x => x.{filterBy} == \"{filterValue}\"";
					query = query.Where(s);
				}
				
			}
			else
			{
				return new ResponseData<ListModel<User>>
				{
					Data = null,
					Success = false,
					ErrorMessage = "There's no such field in User: " + filterBy
				};
			}
		}


		if (sortBy != null && sortOrder != null)
		{
			if (this.DoesPropertyExist((User)query.First(), sortBy))
			{
				if (sortOrder == "ascending")
				{
					if (sortBy == "Roles")
					{
						query = query.OrderBy(user => user.Roles.Max(role => role.Id))
										.AsQueryable();
					}
					else
					{
						query = query.OrderBy(sortBy);
					}
				}
				else if (sortOrder == "descending")
				{
					if (sortBy == "Roles")
					{
						query = query.OrderByDescending(user => user.Roles.Max(role => role.Id))
										.AsQueryable();
					}
					else
					{
						query = query.OrderBy($"{sortBy} desc");
					}
					
				}
				else
				{
					return new ResponseData<ListModel<User>>
					{
						Data = null,
						Success = false,
						ErrorMessage = "Incorrect sort order"
					};
				}
			}
			else
			{
				return new ResponseData<ListModel<User>>
				{
					Data = null,
					Success = false,
					ErrorMessage = "There's no such field in User: " + sortBy
				};
			}
		}

		query = query.Skip(((int)pageNo - 1) * (int)pageSize).Take((int)pageSize);
		dataList.Items = query.ToList();
		dataList.CurrentPage = (int)pageNo;
		dataList.TotalPages = totalPages;

		var response = new ResponseData<ListModel<User>>
		{
			Data = dataList
		};
		return response;
	}
	public async Task<ResponseData<User>> GetUserByIdAsync(int id)
	{
		var user = await _context.Users.FindAsync(id);
		if (user is null)
		{
			return new ResponseData<User>()
			{
				Data = null,
				Success = false,
				ErrorMessage = "Нет пользователя с таким id"
			};
		}
		_context.Entry(user).Collection(u => u.Roles).Load();

		return new ResponseData<User>()
		{
			Data = user,
			Success = true,
			ErrorMessage = null
		};
	}
	public async Task<ResponseData<User>> UpdateUserAsync(int id, User user)
	{
		user.Password = HashCoder.HashPassword(user.Password);
		var _user = await _context.Users.FindAsync(id);

		if (_user == null)
		{
			return new ResponseData<User> 
			{ 
				Data = null,
				Success = false,
				ErrorMessage = "There's no such user"
			};
		}

		ValidatorResponse resp;
		if (_user.Email == user.Email)
			resp = await _validator.Validate(user, false);
		else
			resp = await _validator.Validate(user);

		if (!resp.Status)
		{
			return new ResponseData<User> 
			{
				Data = null,
				Success = false,
				ErrorMessage = resp.Message
			};
		}

		_context.Entry(_user).Collection(u => u.Roles).Load();

		_user.Name = user.Name;
		_user.Age = user.Age;
		_user.Email = user.Email;
		var roles = user.Roles.ToList();
		
		if (_user.Roles != null)
		{
			_user.Roles.Clear();
		}
		else
		{
			_user.Roles = new List<Role>();
		}
		

		foreach (var role in roles)
		{
			_user.Roles.Add(_context.Roles.Find(role.Id));
			//user.Roles.Append(_context.Roles.Find(role.Id));
		};

		_context.Entry(_user).State = EntityState.Modified;
		await _context.SaveChangesAsync();

		return new ResponseData<User> 
		{ 
			Data = _user,
			Success = true,
			ErrorMessage = null
		};
	}
	public async Task DeleteUserAsync(int id)
	{
		User? user = await _context.Users.FindAsync(id);

		if (user != null)
		{
			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
		}
	}
	public async Task<ResponseData<User>> CreateUserAsync(User user)
	{
		if ((await _context.Users.AnyAsync(u => u.Id == user.Id)))
		{
			return new ResponseData<User>()
			{
				Data = null,
				Success = false,
				ErrorMessage = "There's already user with such id: " + user.Id
			};
		}

		foreach (var role in user.Roles)
		{
			if (!(await _context.Roles.AnyAsync(u => u.Id == role.Id)))
			{
				return new ResponseData<User>()
				{
					Data = null,
					Success = false,
					ErrorMessage = "There's no such role: " + role.Id
				};
			}
		}

		user.Password = HashCoder.HashPassword(user.Password);
		var resp = await _validator.Validate(user);

		if (!resp.Status)
		{
			return new ResponseData<User>
			{
				Data = null,
				Success = false,
				ErrorMessage = resp.Message
			};
		}

		var roles = user.Roles.ToList();
		user.Roles.Clear();
		//user.Roles = Enumerable.Empty<Role>();

		foreach (var role in roles)
		{

			user.Roles.Add(_context.Roles.Find(role.Id));
			//user.Roles.Append(_context.Roles.Find(role.Id));
		};

		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		return new ResponseData<User>
		{
			Data = user,
			Success = true,
			ErrorMessage = null
		};
	}
	public async Task<ResponseData<User>> AddUserRoleAsync(int id, Role role)
	{
		var _user = await _context.Users.FindAsync(id);
		
		if (_user is null)
		{
			return new ResponseData<User>
			{
				Data = null,
				Success = false,
				ErrorMessage = "There's no such user"
			};
		}
		
		_context.Entry(_user).Collection(u => u.Roles).Load();

		_user.Roles.Add(_context.Roles.Find(role.Id));

		_context.Entry(_user).State = EntityState.Modified;
		await _context.SaveChangesAsync();

		return new ResponseData<User>
		{
			Data = _user,
			Success = true,
			ErrorMessage = null
		};
	}

	public async Task<bool> IsEmailUnique(string email)
	{
		var req = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
		return req == null;
	}

	public async Task<bool> DoesUserExistAsync(int id)
	{
		return await _context.Users.AnyAsync(u => u.Id == id);
	}

	public async Task<bool> DoesUserExistAsync(string email, string password)
	{
		return await _context.Users.AnyAsync(u => u.Email == email && u.Password == password);
	}

	public bool DoesPropertyExist(object obj, string propertyName)
	{
		return obj.GetType().GetProperty(propertyName) != null;
	}
}
