using Quanlicuahang.Repositories;
using Quanlicuahang.Models;
using AutoMapper;
using Quanlicuahang.DTOs;

namespace Quanlicuahang.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<UserResponse?> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> AddAsync(UserRequest request)
        {
            var user = _mapper.Map<User>(request);
            var newUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserResponse>(newUser);
        }

        public async Task<UserResponse?> UpdateAsync(string id, UserUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrEmpty(request.Password))
                user.Password = request.Password;

            if (request.Role != null && request.Role.Any())
                user.Role = request.Role;

            var updatedUser = await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserResponse>(updatedUser);
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                await _userRepository.DeleteAsync(user);
            }
        }
    }
}
