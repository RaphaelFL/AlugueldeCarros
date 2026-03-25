using AlugueldeCarros.DTOs.Payments;
using AlugueldeCarros.Repositories;
using AlugueldeCarros.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlugueldeCarros.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IReservationRepository _reservationRepository;

    public PaymentsController(PaymentService paymentService, IReservationRepository reservationRepository)
    {
        _paymentService = paymentService;
        _reservationRepository = reservationRepository;
    }

    [HttpPost("preauth")]
    [Authorize]
    public async Task<IActionResult> Preauth([FromBody] PreauthRequest request)
    {
        if (!await CanAccessReservationAsync(request.ReservationId))
            return Forbid();

        var payment = await _paymentService.PreauthorizePaymentAsync(request.ReservationId, request.Amount);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    [HttpPost("capture")]
    [Authorize]
    public async Task<IActionResult> Capture([FromBody] CaptureRequest request)
    {
        if (!await CanAccessPaymentAsync(request.PaymentId))
            return Forbid();

        var payment = await _paymentService.CapturePaymentAsync(request.PaymentId);
        return Ok(payment);
    }

    [HttpPost("refund")]
    [Authorize]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request)
    {
        if (!await CanAccessPaymentAsync(request.PaymentId))
            return Forbid();

        var payment = await _paymentService.RefundPaymentAsync(request.PaymentId);
        return Ok(payment);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        if (!await CanAccessPaymentAsync(id))
            return Forbid();

        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    private async Task<bool> CanAccessPaymentAsync(int paymentId)
    {
        var payment = await _paymentService.GetByIdAsync(paymentId);
        if (payment == null)
            return false;

        return await CanAccessReservationAsync(payment.ReservationId);
    }

    private async Task<bool> CanAccessReservationAsync(int reservationId)
    {
        if (User.IsInRole("Admin"))
            return true;

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return false;

        var reservation = await _reservationRepository.GetByIdAsync(reservationId);
        return reservation != null && reservation.UserId == userId;
    }
}
