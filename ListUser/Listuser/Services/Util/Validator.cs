using Domain.Entities;
using ListUser.Services.UserService;

namespace ListUser.Services.Util;

public class UserValidator
{
	IUserService _userService;
	public UserValidator(IUserService userService)
	{
		_userService = userService;
	}

	public async Task<ValidatorResponse> Validate(User? user, bool validateEmail = true)
	{
		if (user == null)
		{
			return new ValidatorResponse { Status = false, Message = "User is null"};
		}

		if (user.Email == null 
			|| user.Age == null 
			|| user.Name == null)
		{
			return new ValidatorResponse { Status = false, Message = "One of required field is empty" };
		}

		if (user.Age < 0)
		{
			return new ValidatorResponse { Status = false, Message = "Incorrect age" };
		}
		if (validateEmail) 
		{ 
			if (!(await _userService.IsEmailUnique(user.Email)))
			{
				return new ValidatorResponse { Status = false, Message = "Not unique email" };
			}
		}
		

		return new ValidatorResponse { Status = true, Message = "OK" }; ;
	}
}
