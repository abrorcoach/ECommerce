using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrders();
    Task<IEnumerable<OrderDto>> GetUserOrders(int userId);
    Task<OrderDto?> GetOrderById(int id);
    Task<OrderDto> CreateOrder(int userId, CreateOrderRequest request);
    Task<OrderDto?> UpdateOrderStatus(int id, UpdateOrderStatusRequest request);
}
