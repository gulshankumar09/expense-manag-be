using AuthService.Application.Constants;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Constants;
using SharedLibrary.Models;
using SharedLibrary.Utility;

namespace AuthService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserService(
        IUserRepository userRepository,
        UserManager<User> userManager,
        IOtpService otpService,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _otpService = otpService;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<UserResponse> MapToUserResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(
            user.Id,
            user.Email!,
            user.Name,
            user.PhoneNumber ?? string.Empty,
            user.EmailConfirmed,
            user.IsActive,
            user.CreatedAt,
            roles);
    }

    public async Task<IResult<UserResponse>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<UserResponse>.Failure(Error.NotFound());

        var response = await MapToUserResponse(user);
        return Result<UserResponse>.Success(response);
    }

    public async Task<IResult<UserResponse>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Result<UserResponse>.Failure(Error.NotFound());

        var response = await MapToUserResponse(user);
        return Result<UserResponse>.Success(response);
    }

    public async Task<IResult<string>> RegisterUserAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            if (!existingUser.EmailConfirmed)
            {
                // Generate and send new OTP
                var newOtp = _otpService.GenerateOtp();
                existingUser.SetVerificationToken(newOtp, TimeSpan.FromMinutes(15));
                await _userManager.UpdateAsync(existingUser);
                await _otpService.SendOtpAsync(request.Email, newOtp);

                var existingUserResult = Result<string>.Success("Email already registered. Please verify your email.");
                var url = UrlUtility.GetAbsoluteUrl(_httpContextAccessor.HttpContext.Request, ApiEndpoints.Account.VerifyOtp);
                var redirectUrl = $"{url}?email={request.Email}&otp={newOtp}";
                existingUserResult.AddHeader(HeaderKeys.RedirectUrl, redirectUrl);
                return existingUserResult;
            }

            return Result<string>.Failure("Email already registered and verified.");
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = PersonName.Create(request.FirstName, request.LastName),
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM_USER"
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result<string>.Failure(createResult.Errors.First().Description);

        try
        {
            // Add to User role by default
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                // If role assignment fails, delete the user and return error
                await _userManager.DeleteAsync(user);
                return Result<string>.Failure("Failed to assign User role. Registration cancelled.");
            }

            // Generate and send OTP
            var registrationOtp = _otpService.GenerateOtp();
            user.SetVerificationToken(registrationOtp, TimeSpan.FromMinutes(15));
            await _userManager.UpdateAsync(user);
            await _otpService.SendOtpAsync(request.Email, registrationOtp);

            var successResult = Result<string>.Success("Registration successful. Please verify your email.");
            successResult.AddHeader(HeaderKeys.RedirectUrl, ApiEndpoints.Account.VerifyOtp);
            return successResult;
        }
        catch (Exception)
        {
            // If anything fails after user creation, clean up by deleting the user
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task<IResult<UserResponse>> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<UserResponse>.Failure(Error.NotFound());

        user.Name = PersonName.Create(request.FirstName, request.LastName);
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Result<UserResponse>.Failure(result.Errors.First().Description);

        var response = await MapToUserResponse(user);
        return Result<UserResponse>.Success(response);
    }

    public async Task<IResult> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(Error.NotFound());

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.First().Description);
    }

    public async Task<IResult> ReactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(Error.NotFound());

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.First().Description);
    }

    public async Task<IResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(Error.NotFound());

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.First().Description);
    }

    public async Task<IResult<PaginatedResponse<UserResponse>>> ListUsersAsync(UserListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .Where(u => !u.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u =>
                (u.Email != null && EF.Functions.Like(u.Email, $"%{request.SearchTerm}%")) ||
                (u.Name.FirstName != null && EF.Functions.Like(u.Name.FirstName, $"%{request.SearchTerm}%")) ||
                (u.Name.LastName != null && EF.Functions.Like(u.Name.LastName, $"%{request.SearchTerm}%")));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.Role);
            var userIds = usersInRole.Select(u => u.Id);
            query = query.Where(u => userIds.Contains(u.Id));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "name" => request.SortDescending
                ? query.OrderByDescending(u => u.Name.LastName)
                : query.OrderBy(u => u.Name.LastName),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var userResponses = new List<UserResponse>();
        foreach (var user in items)
        {
            userResponses.Add(await MapToUserResponse(user));
        }

        var response = new PaginatedResponse<UserResponse>(
            userResponses,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PaginatedResponse<UserResponse>>.Success(response);
    }
}