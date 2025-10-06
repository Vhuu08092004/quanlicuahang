using Quanlicuahang.Repositories;
using Quanlicuahang.Models;
using Quanlicuahang.DTOs.Response;
using Quanlicuahang.Mapper;
using AutoMapper;
using Quanlicuahang.DTOs.Request;

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
            var users = await _userRepository.getAllAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _userRepository.getByIdAsync(id);
            return _mapper.Map<UserResponse?>(user);
        }

        public async Task<UserResponse> AddAsync(UserRequest request)
        {
            User? user = _mapper.Map<User>(request);
            var newUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserResponse>(newUser);
        }

        public async Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request)
        {
            var user = await _userRepository.getByIdAsync(id);
            if (user == null)
                return null;
            if (!string.IsNullOrEmpty(request.FullName))
            {
                user.FullName = request.FullName;
            }
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = request.Password;
            }
            if (request.Role != null && request.Role.Any())
            {
                user.Role.AddRange(request.Role);
            }
            var updatedUser = await _userRepository.Update(user);
            return _mapper.Map<UserResponse>(updatedUser);
        }

        public async Task DeleteAsync(int id)
        {
            User? user = await _userRepository.getByIdAsync(id);
            if (user != null)
            {
                _userRepository.Delete(user);
            }
        }
    }
}
